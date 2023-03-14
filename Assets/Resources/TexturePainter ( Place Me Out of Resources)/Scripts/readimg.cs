using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class readimg : MonoBehaviour
{
    // Start is called before the first frame update
    public TextAsset imageAsset;
    
    void Start()
    {
        Texture2D tex = new Texture2D(2, 2);
        var m_Texture2D = new Texture2D(16, 16, TextureFormat.RGBA32, true);
        tex.LoadImage(imageAsset.bytes);
        var mip0Data = tex.GetPixelData<Color32>(0);


       // byte[] imageData = File.ReadAllBytes(Application.dataPath + "/path/to/image.png"); // read the image data from file
        //Unity.Collections.NativeArray<int> img = imageAsset.GetData<int>();
        GetComponent<Renderer>().material.mainTexture = tex;

       // Debug.Log(mip0Data.Length);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

internal struct T
{
}