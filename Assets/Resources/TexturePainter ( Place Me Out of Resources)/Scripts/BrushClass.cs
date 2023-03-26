using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.XR;
using Ubiq.Messaging; // new
using System.Runtime.ConstrainedExecution;

public class BrushClass : MonoBehaviour
{
    public Sprite sp1, sp2, spnone;
    private Sprite edge_sp1, edge_sp2;

    private Sprite cur_sprite;
    private NetworkContext context; // new
    private bool owner; // new
    private Hand controller;
    private struct Message
    {
        public Vector3 position;
        public Quaternion rotation;

        public Message(Transform transform)
        {
            this.position = transform.position;
            this.rotation = transform.rotation;
        }
    }
    // Start is called before the first frame update

    Sprite getedge(Sprite cur_sprite)
    {
        Texture2D originalTexture = cur_sprite.texture;
        Texture2D modifiedTexture = new Texture2D(originalTexture.width, originalTexture.height);
        Sprite edge_sprite = Sprite.Create(modifiedTexture, new Rect(0, 0, originalTexture.width, originalTexture.height), Vector2.zero);

        // Apply edge detection algorithm to original texture and save result in modified texture
        for (int y = 0; y < originalTexture.height; y++)
        {
            for (int x = 0; x < originalTexture.width; x++)
            {
                Color modified_pixel = Color.white;
                Color pixelColour = originalTexture.GetPixel(x, y);
                if(y > 0)
                {
                    modified_pixel += pixelColour - originalTexture.GetPixel(x, y - 1);
                }
                modifiedTexture.SetPixel(x, y, modified_pixel);
                // Apply edge detection algorithm to pixel color and set the modified color in modified texture
                // ...
            }
        }
        modifiedTexture.Apply();
        return edge_sprite;
    }

    void Start()
    {
        edge_sp1 = getedge(sp1);
        edge_sp2 = getedge(sp2);
        context = NetworkScene.Register(this);
    }
    public void ProcessMessage (ReferenceCountedSceneGraphMessage msg)
    {
        // 3. Receive and use transform update messages from remote users
        // Here we use them to update our current position
        var data = msg.FromJson<Message>();
        transform.position = data.position;
        transform.rotation = data.rotation;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            cur_sprite = sp1;
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            cur_sprite = sp2;
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            cur_sprite = spnone;
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            cur_sprite = edge_sp1;
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            cur_sprite = edge_sp2;
        }

        GetComponent<SpriteRenderer>().sprite = cur_sprite;
    }
}
