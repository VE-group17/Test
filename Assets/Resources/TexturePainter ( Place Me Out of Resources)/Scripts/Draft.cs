using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.XR;
using Ubiq.Messaging; // new
using System.Runtime.ConstrainedExecution;
using System;

public class Draft : MonoBehaviour
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

    Texture2D Gaussian_denoise(Texture2D originalTexture)
    {
        Texture2D modifiedTexture = new Texture2D(originalTexture.width, originalTexture.height);
        Vector3 weight_dis = new Vector3(0.20417996f, 0.1238414f, 0.07511361f);
        int[,] offsets_d1 = new int[,]
        {
            {-1, 0},
            {1, 0},
            {0, -1},
            {0, 1}
        };
        int[,] offsets_d2 = new int[,]
        {
            {-1, -1},
            {-1, 1},
            {1, -1},
            {1, 1}
        };
        Color pixel_color;
        for (int y = 0; y < originalTexture.height; y++)
        {
            for (int x = 0; x < originalTexture.width; x++)
            {
                if (x == 0 || y == 0 || x == originalTexture.width - 1 || y == originalTexture.height - 1)
                {
                    pixel_color = originalTexture.GetPixel(x, y);
                }
                else
                {
                    pixel_color = originalTexture.GetPixel(x, y) * weight_dis[0];
                    for (int i = 0; i < 4; ++i)
                    {
                        int nx = x + offsets_d1[i, 0];
                        int ny = y + offsets_d1[i, 1];
                        pixel_color += originalTexture.GetPixel(nx, ny) * weight_dis[1];
                    }
                    for (int i = 0; i < 4; ++i)
                    {
                        int nx = x + offsets_d2[i, 0];
                        int ny = y + offsets_d2[i, 1];
                        pixel_color += originalTexture.GetPixel(nx, ny) * weight_dis[2];
                    }
                }
                modifiedTexture.SetPixel(x, y, pixel_color);
            }
        }
        modifiedTexture.Apply();
        return modifiedTexture;
    }

    double[,] Sobel_filter(Texture2D denoise_Texture)
    {
        double[,] gd = new double[denoise_Texture.width, denoise_Texture.height];
        int[] weight = new int[] { 2, -2, 1, 1, -1, -1 };
        int[,] offsets_d1 = new int[,]
        {
            {1, 0},
            {-1, 0},
            {1, 1},
            {1, -1},
            {-1, 1},
            {-1, -1}
        };
        int[,] offsets_d2 = new int[,]
        {
            {0, 1},
            {0, -1},
            {1, 1},
            {-1, 1},
            {1, -1},
            {-1, -1}
        };
        double cur_value = 0;
        double ma_value = -1;
        double mi_value = 10;
        for (int y = 0; y < denoise_Texture.height; y++)
        {
            for (int x = 0; x < denoise_Texture.width; x++)
            {
                if (x == 0 || y == 0 || x == denoise_Texture.width - 1 || y == denoise_Texture.height - 1)
                {
                    cur_value = 0.0f;
                }
                else
                {
                    cur_value = 0.0f;
                    double x_value = 0.0f;
                    double y_value = 0.0f;
                    for (int i = 0; i < 6; ++i)
                    {
                        int nx = x + offsets_d1[i, 0];
                        int ny = y + offsets_d1[i, 1];
                        x_value += denoise_Texture.GetPixel(nx, ny).grayscale * weight[i];
                    }
                    for (int i = 0; i < 6; ++i)
                    {
                        int nx = x + offsets_d2[i, 0];
                        int ny = y + offsets_d2[i, 1];
                        y_value += denoise_Texture.GetPixel(nx, ny).grayscale * weight[i];
                    }
                    cur_value = Math.Sqrt(x_value * x_value + y_value * y_value);
                }
                if(ma_value < cur_value)
                {
                    ma_value = cur_value;
                }
                if(mi_value > cur_value)
                {
                    mi_value = cur_value;
                }
                gd[x, y] = cur_value;
            }
        }
        //double residual_mami = ma_value - mi_value;
        //for (int y = 0; y < denoise_Texture.height; y++)
        //{
        //    for (int x = 0; x < denoise_Texture.width; x++)
        //    {
        //        gd[x, y] = (cur_value - mi_value) / residual_mami;
        //    }
        //}


        return gd;
    }

    Texture2D Canny_edge(Texture2D originalTexture)
    {
        Texture2D denoise_Texture = Gaussian_denoise(originalTexture);
        double[,] intensity_gradients = Sobel_filter(denoise_Texture);


        Texture2D modifiedTexture = new Texture2D(denoise_Texture.width, denoise_Texture.height);
        for (int y = 0; y < denoise_Texture.height; y++)
        {
            for (int x = 0; x < denoise_Texture.width; x++)
            {
                Color pixel_color;
                if (intensity_gradients[x, y] > 0.2)
                {
                    pixel_color = Color.black;
                }
                else
                {
                    pixel_color = Color.white;
                }
                modifiedTexture.SetPixel(x, y, pixel_color);
                // Apply edge detection algorithm to pixel color and set the modified color in modified texture
                // ...
            }
        }
        modifiedTexture.Apply();
        return modifiedTexture;
    }


    Sprite getedge(Sprite cur_sprite)
    {
        Texture2D originalTexture = cur_sprite.texture;
        //Texture2D modifiedTexture = new Texture2D(originalTexture.width, originalTexture.height);
        Texture2D modifiedTexture = Canny_edge(originalTexture);
        Sprite edge_sprite = Sprite.Create(modifiedTexture, new Rect(0, 0, originalTexture.width, originalTexture.height), Vector2.zero);

        // Apply edge detection algorithm to original texture and save result in modified texture
        modifiedTexture.Apply();
        return edge_sprite;
    }

    void Start()
    {
        edge_sp1 = getedge(sp1);
        edge_sp2 = getedge(sp2);
        context = NetworkScene.Register(this);
    }
    public void ProcessMessage(ReferenceCountedSceneGraphMessage msg)
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