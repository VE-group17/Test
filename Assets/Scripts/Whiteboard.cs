using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Whiteboard : MonoBehaviour
{
    public Texture2D texture;
    public Vector2 textureSize = new Vector2(2048, 2048);
    void Start()
    {
        var r = GetComponent<Renderer>();
        texture = new Texture2D((int)textureSize.x, (int)textureSize.y);
        print(r.materials.Length);
        r.material.SetTexture("_DetailAlbedoMap", texture);
     //  r.materials[1].mainTexture = texture;
      //  r.material.mainTexture = texture;
    }
}
