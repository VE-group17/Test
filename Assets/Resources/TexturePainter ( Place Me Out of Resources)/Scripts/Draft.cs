using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.XR;
using Ubiq.Messaging; // new
using System.Runtime.ConstrainedExecution;
using System;
using System.Security.Cryptography;

public class Draft : MonoBehaviour
{
    public Sprite sp1, sp2, spnone;
    private Sprite edge_sp1, edge_sp2;
    double toAngle = 180.0 / Math.PI;
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

    double[,] Sobel_filter_nonmax(Texture2D denoise_Texture)
    {
        double[,] gd = new double[denoise_Texture.width, denoise_Texture.height];
        double[,] nonmax_gd = new double[denoise_Texture.width, denoise_Texture.height];
        byte[,] orients = new byte[denoise_Texture.width, denoise_Texture.height];
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
        double ma_value = -10000000;
        double mi_value = 10000000;
        for (int y = 0; y < denoise_Texture.height; y++)
        {
            for (int x = 0; x < denoise_Texture.width; x++)
            {
                double gx = 0;
                double gy = 0;
                double orientation = 0;
                if (x == 0 || y == 0 || x == denoise_Texture.width - 1 || y == denoise_Texture.height - 1)
                {
                    cur_value = 0.0f;
                }
                else
                {
                    cur_value = 0.0f;
                    for (int i = 0; i < 6; ++i)
                    {
                        int nx = x + offsets_d1[i, 0];
                        int ny = y + offsets_d1[i, 1];
                        gx += denoise_Texture.GetPixel(nx, ny).grayscale * weight[i];
                    }
                    for (int i = 0; i < 6; ++i)
                    {
                        int nx = x + offsets_d2[i, 0];
                        int ny = y + offsets_d2[i, 1];
                        gy += denoise_Texture.GetPixel(nx, ny).grayscale * weight[i];
                    }
                    cur_value = Math.Sqrt(gx * gx + gy * gy);
                }
                if (ma_value < cur_value)
                {
                    ma_value = cur_value;
                }
                if (mi_value > cur_value)
                {
                    mi_value = cur_value;
                }
                gd[x, y] = cur_value;

                if (gx == 0)
                {
                    // can not divide by zero
                    orientation = (gy == 0) ? 0 : 90;
                }
                else
                {
                    double div = (double)gy / gx;

                    // handle angles of the 2nd and 4th quads
                    if (div < 0)
                    {
                        orientation = 180 - System.Math.Atan(-div) * toAngle;
                    }
                    // handle angles of the 1st and 3rd quads
                    else
                    {
                        orientation = System.Math.Atan(div) * toAngle;
                    }

                    // get closest angle from 0, 45, 90, 135 set
                    if (orientation < 22.5)
                        orientation = 0;
                    else if (orientation < 67.5)
                        orientation = 45;
                    else if (orientation < 112.5)
                        orientation = 90;
                    else if (orientation < 157.5)
                        orientation = 135;
                    else orientation = 0;
                }
                orients[x, y] = (byte) orientation;
            }
        }
        double residual_mami = ma_value - mi_value + 0.0000000001;
        for (int y = 0; y < denoise_Texture.height; y++)
        {
            for (int x = 0; x < denoise_Texture.width; x++)
            {
                gd[x, y] = (gd[x, y] - mi_value) / residual_mami;
            }
        }


        for (int y = 1; y < denoise_Texture.height - 1; y++)
        {
            for (int x = 1; x < denoise_Texture.width - 1; x++)
            {
                double leftPixel = 0;
                double rightPixel = 0;
                switch (orients[x, y])
                {
                    case 0:
                        leftPixel = gd[x - 1, y];
                        rightPixel = gd[x + 1, y];
                        break;
                    case 45:
                        leftPixel = gd[x - 1, y + 1];
                        rightPixel = gd[x + 1, y - 1];
                        break;
                    case 90:
                        leftPixel = gd[x, y + 1];
                        rightPixel = gd[x, y - 1];
                        break;
                    case 135:
                        leftPixel = gd[x + 1, y + 1];
                        rightPixel = gd[x - 1, y - 1];
                        break;
                }
                if ((gd[x, y] < leftPixel) || (gd[x, y] < rightPixel))
                {
                    nonmax_gd[x, y] = 0;
                }
                else
                {
                    nonmax_gd[x, y] = gd[x, y];
                }
            }
        }
        return nonmax_gd;
    }

    Texture2D Canny_edge(Texture2D originalTexture)
    {
        Texture2D denoise_Texture = Gaussian_denoise(originalTexture);
        double[,] intensity_gradients = Sobel_filter_nonmax(denoise_Texture);
        double highThreshold = 0.15;
        double lowThreshold = 0.05;
        Texture2D modifiedTexture = new Texture2D(denoise_Texture.width, denoise_Texture.height);

        int[,] offsets = new int[,]
        {
            {1, 1},
            {1, 0},
            {1, -1},
            {0, -1},
            {0, 1},
            {1, -1},
            {1, 0},
            {1, 1}
        };
        for (int y = 0; y < denoise_Texture.height; y++)
        {
            for (int x = 0; x < denoise_Texture.width; x++)
            {
                Color pixel_color;
                if (intensity_gradients[x,y] < highThreshold)
                {
                    if (intensity_gradients[x, y] < lowThreshold)
                    {
                        // non edge
                        pixel_color = Color.white;
                    }
                    else
                    {
                        // check 8 neighboring pixels
                        bool neigh_high = false;
                        for (int i = 0; i < 8; ++i)
                        {
                            int nx = x + offsets[i, 0];
                            int ny = y + offsets[i, 1];
                            if (intensity_gradients[nx, ny] >= highThreshold)
                            {
                                neigh_high = true;
                            }
                        }
                        if (neigh_high == true)
                        {
                            pixel_color = Color.black;
                        } else
                        {
                            pixel_color = Color.white;
                        }
                    }
                }
                else
                {
                    pixel_color = Color.black;
                }
                pixel_color.a = 0.7f;
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