using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CanvasUpdate : MonoBehaviour
{
    public Camera StrokeCamera;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RenderTexture renderTexture = StrokeCamera.targetTexture;

        Texture2D texture2 = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA64, false);
        RenderTexture.active = renderTexture;
        
        texture2.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height),0,0);
        texture2.Apply();

        Renderer m_Renderer = GetComponent<Renderer>();
        m_Renderer.material.SetTexture("_MainTex", texture2);
        
        // byte[] bytes = texture2.EncodeToPNG();
        // File.WriteAllBytes("screenshot.png", bytes);

        
    }
}
