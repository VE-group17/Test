using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CanvasUpdate : MonoBehaviour
{
    public Camera StrokeCamera;
    private Texture2D texture2;
    private RenderTexture renderTexture;
    public GameObject brushContainer;
    private int count;
    // Start is called before the first frame update
    void Start()
    {
        renderTexture = StrokeCamera.targetTexture;
        texture2 = new Texture2D(renderTexture.width, renderTexture.height);

        count = 0;
    }

    // Update is called once per frame
    void Update()
    {
        RenderTexture.active = renderTexture;
        texture2.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height),0,0);
        texture2.Apply();
        RenderTexture.active = null;


        // Graphics.Blit(texture2, renderTexture);
        Renderer m_Renderer = GetComponent<Renderer>();
        m_Renderer.material.SetTexture("_MainTex", texture2);


        // if (brushContainer.transform.childCount > 100)
        // {
        //     List<GameObject> childrenObjects = new List<GameObject>();
        //     for(int i = 0; i < 100; i++)
        //     {
        //         childrenObjects.Add(brushContainer.transform.GetChild(i).gameObject);
        //     }
        //     foreach (GameObject brush in childrenObjects)
        //     {
        //         Destroy(brush);
        //     }
        //     count = count +1;
        // }
        // if (count ==1)
        // {
        //     byte[] bytes = texture2.EncodeToPNG();
        //     File.WriteAllBytes("screenshot.png", bytes);
        // }
        count = count + 1;
        if (count == 1200)
        {
            byte[] bytes = texture2.EncodeToPNG();
            File.WriteAllBytes("screenshot.png", bytes);
            Debug.Log("hahahahahaah");
        }
        
    }
}
