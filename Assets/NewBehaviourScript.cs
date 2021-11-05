using UnityEngine;
using System.IO;
using System;
using System.Collections;
using System.Text;


public class NewBehaviourScript : MonoBehaviour
{
    public ComputeShader shader;

    // ComputeBuffer, c#端与gpu数据通信的容器，我们组织好需要计算的数据，
    // 装在这个buffer里面，然后把这个buffer塞到computeShader里面，为gpu计算做数据准备
    private ComputeBuffer buffer;
    public Texture2D texture;
    public RenderTexture renderTexture;
    uint[] pixels;
    private Color a;
    int iter;
    byte[] bytes;
    int width = Screen.width, height = Screen.height;
    private string RecordPicPath;
    public int runTimes = 3;
    private ParticleData[] particleDatas;

    void Start()
    {
        Init();
        // TestReadWriteTexture();
    }

    private void TestReadWriteTexture()
    {
        var strPath = Path.Combine(Application.dataPath, "../" + 1 + ".jpg");
        var strPath1 = Path.Combine(Application.dataPath, "../" + 3 + ".jpg");
        var bt = ReadTexture(strPath);
        var sb = new StringBuilder();
        foreach (var b in bt)
        {
            sb.Append(b + "@");
        }

        Debug.Log("Len" + bytes.Length + "**" + sb.ToString());

        using (var f = File.OpenWrite(strPath1))
        {
            f.Write(bt, 0, bt.Length);
            f.Close();
        }
    }

    private byte[] ReadTexture(string path)
    {
        Debug.Log(" @ ! the texture path is + !!    " + path);
        FileStream fileStream = new FileStream(path, FileMode.Open, System.IO.FileAccess.Read);

        fileStream.Seek(0, SeekOrigin.Begin);

        byte[] buffer = new byte[fileStream.Length]; //创建文件长度的buffer   
        fileStream.Read(buffer, 0, (int) fileStream.Length);

        fileStream.Close();

        fileStream.Dispose();

        fileStream = null;

        return buffer;
    }

    void OnPostRender()
    {
        if (runTimes-- > 0)
        {
            RunShader();
        }
    }

    private void Init()
    {
        pixels = new uint[width * height];
        buffer = new ComputeBuffer(width * height, 4);
        bytes = new byte[pixels.Length * 4];
        texture = new Texture2D(width, height);
    }

    // //src和des是引擎创建的对象，我们只能使用，但是不要自己引用到本地。
    // private void OnRenderImage(RenderTexture src, RenderTexture des)
    // {
    //     Graphics.Blit(src, des); //这一句一定要有，保证画面传递下去。不然会中断camera画面，别的滤镜会失效。
    //
    //     //这里要拷贝渲染出一张rt
    //     if (renderTexture == null)
    //     {
    //         renderTexture = RenderTexture.GetTemporary(src.width, src.height); //unity自己在维护一个RenderTexture池，我们从缓存中取就行。
    //     }
    //
    //     Graphics.Blit(src, renderTexture); //拷贝图像
    // }
    //
    // private void OnDestroy()
    // {
    //     if (renderTexture != null)
    //     {
    //         RenderTexture.ReleaseTemporary(renderTexture); //记得释放引用
    //     }
    // }

    public struct ParticleData
    {
        public Color color; //等价于float4
    }

    private void RunShader()
    {
        //把我们准备好的数据塞给Buffer里
        buffer.SetData(pixels);
        //找到GPU真正执行的方法在computeShader里的索引
        int kernel = shader.FindKernel("SampleTexture");
        //把我们之前准备好的buffer数据塞给computeShader，这样就建立了一个gpu到cpu的数据连接，gpu在计算时
        //会使用当前这个buffer里的数据。
        //注意：下面方法中的第二个参数 必须与 shader 里对应的那个 buffer 的变量名一模一样
        // shader.SetBuffer(kernel, "ParticleBuffer", buffer);
        shader.SetBuffer(kernel, "result", buffer);
        // shader.SetTexture(kernel, "writer", texture);
        shader.SetTexture(kernel, "reader", renderTexture);
        shader.SetInt("width", width);
        shader.SetInt("height", height);
        shader.Dispatch(kernel, width / 32, height / 32, 1);

        buffer.GetData(pixels);
        // if (runTimes == 0)
        // {
        //     var a = 1;
        // }

        // var index = 0;
        // for (int j = height; j > 0; j--)
        // {
        //     for (int i = width; i > 0; i--)
        //     {
        //         var pixel = pixels[index];
        //         var r = pixel >> 16;
        //         var g = (pixel >> 8) - (r << 8);
        //         var b = pixel - ((pixel >> 8) << 8);
        //         var color = new Color(r, g, b) / 255.0f;
        //         texture.SetPixel(i, j, color);
        //
        //         index++;
        //     }
        // }

        Buffer.BlockCopy(pixels, 0, bytes, 0, pixels.Length * 4);

        if (bytes[0] == 0)
        {
            return;
        }
        
        
        var sb = new StringBuilder();
        foreach (var b in bytes)
        {
            sb.Append(b + "@");
        }

        Debug.Log("Len" + bytes.Length + "**" + sb.ToString());
        // Color[] colors = new Color[pixels.Length];
        // for (var i = 0; i < pixels.Length; i++)
        // {
        //     var pixel = pixels[i];
        //     var r = pixel >> 16;
        //     var g = (pixel >> 8) - (r << 8);
        //     var b = pixel - ((pixel >> 8) << 8);
        //     colors[i] = new Color(r, g, b);
        // }
        // texture.SetPixels(colors);
        // WriteBytesToFile(bytes, ".jpg");

        // texture = new Texture2D(width, height);
        // texture.LoadImage(bytes);
        // texture.Apply();
        // StartCoroutine(DelaySaveImg(bytes));

        // var strPath = Path.Combine(Application.dataPath, "../" + iter + ".jpg");
        // File.WriteAllBytes(strPath, texture.EncodeToJPG());
        // using (var f = File.OpenWrite(strPath))
        // {
        //     f.Write(bytes, 0, bytes.Length);
        //     f.Close();
        // }

        iter++;
    }

    private IEnumerator DelaySaveImg(byte[] bytes)
    {
        texture.LoadImage(bytes);
        yield return null;
    }

    void OnGUI()
    {
        GUI.DrawTexture(new Rect(0, 0, width, height), texture);
    }
}