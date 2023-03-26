using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CanvasUpdate : MonoBehaviour
{
    public KeyCode screenShotButton;
    public Camera StrokeCamera;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // currentSceneImage = camera.targetTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        // ImageConversion.EncodeToPNG(texture);
        // RenderTexture renderTexture = StrokeCamera.targetTexture;
        // // StrokeCamera.Render();

        // Texture2D texture2 = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA64, false);
        // texture2.Apply();

        // texture2.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height),0,0);

        // byte[] bytes = texture2.EncodeToPNG();
        // File.WriteAllBytes("screenshot.png", bytes);
    }
}
