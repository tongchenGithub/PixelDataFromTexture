using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;


public class NewBehaviourScript : MonoBehaviour {

    public ComputeShader shader;
    public Texture2D texture;
    public RenderTexture renderTexture;
    uint[] pixels;
    ComputeBuffer buffer;
    int iter;
    byte[] bytes;
    int width = 512, height = 512;

    void Start()
    {
        Init();
    }

    void OnPostRender()
    {
        RunShader();
    }

    public void Init()
    {
        pixels = new uint[width * height];
        buffer = new ComputeBuffer(width * height, 4);
        bytes = new byte[pixels.Length * 4];
    }

    void RunShader()
    {

        //this.GetComponent<Camera>().Render();
     
        
        buffer.SetData(pixels);
        int kernel = shader.FindKernel("Multiply");
        shader.SetBuffer(kernel, "result", buffer);
        shader.SetTexture(kernel, "tex", renderTexture);
        shader.SetInt("width", width);
        shader.SetInt("height", height);
        shader.Dispatch(kernel, width/32, height/32, 1);
        
        //RenderTexture.active = renderTexture;

        
        buffer.GetData(pixels);

        Buffer.BlockCopy(pixels, 0, bytes, 0, pixels.Length * 4);

        using (var f = File.OpenWrite(iter.ToString()))
        {
            f.Write(bytes, 0, bytes.Length);
            f.Close();
        }
        iter++;
        
        //RenderTexture.active = null;

        //UnityEngine.Debug.Log("ok");

    }
}
