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
    public Sprite sp1, sp2, sp3, spnone; // original sprite image
    private Sprite edge_sp1, edge_sp2, edge_sp3; // edge sprite image
    double toAngle = 180.0 / Math.PI; // pi to angle
    private Sprite cur_sprite; // sprite for this gameobject
    private NetworkContext context; 
    private bool owner; 
    private Hand controller;
    public int selectnumber; // for UI to choose which sprite 
    public bool use_edge = false; // edge or original
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
        // Gaussian fillter weights
        Vector3 weight_dis = new Vector3(0.20417996f, 0.1238414f, 0.07511361f);
        // x direction
        int[,] offsets_d1 = new int[,]
        {
            {-1, 0},
            {1, 0},
            {0, -1},
            {0, 1}
        };
        // y direction
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
                    // boudary
                    pixel_color = originalTexture.GetPixel(x, y);
                }
                else
                {
                    // wighted sum of gaussian filtter
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
        // Sobel weight
        int[] weight = new int[] { 2, -2, 1, 1, -1, -1 };
        // Sobel x direction with weight
        int[,] offsets_d1 = new int[,]
        {
            {1, 0},
            {-1, 0},
            {1, 1},
            {1, -1},
            {-1, 1},
            {-1, -1}
        };
        // Sobel y direction with weight
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

        // sobel filter to get gradient
        for (int y = 0; y < denoise_Texture.height; y++)
        {
            for (int x = 0; x < denoise_Texture.width; x++)
            {
                double gx = 0;
                double gy = 0;
                double orientation = 0;
                // boundary is not a edge
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
                    // sobel filter to get gradient gx
                    for (int i = 0; i < 6; ++i)
                    {
                        int nx = x + offsets_d2[i, 0];
                        int ny = y + offsets_d2[i, 1];
                        gy += denoise_Texture.GetPixel(nx, ny).grayscale * weight[i];
                    }
                    // sobel filter to get gradient gy

                    // final gradient l2 norm
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
        // normalise pixel value
        double residual_mami = ma_value - mi_value + 0.0000000001;
        for (int y = 0; y < denoise_Texture.height; y++)
        {
            for (int x = 0; x < denoise_Texture.width; x++)
            {
                gd[x, y] = (gd[x, y] - mi_value) / residual_mami;
            }
        }

        // Non-maximum Suppression
        for (int y = 1; y < denoise_Texture.height - 1; y++)
        {
            for (int x = 1; x < denoise_Texture.width - 1; x++)
            {
                double leftPixel = 0;
                double rightPixel = 0;
                switch (orients[x, y]) // which direction 
                {
                    case 0: // 0 degree
                        leftPixel = gd[x - 1, y];
                        rightPixel = gd[x + 1, y];
                        break;
                    case 45: // 45 degree
                        leftPixel = gd[x - 1, y + 1];
                        rightPixel = gd[x + 1, y - 1];
                        break;
                    case 90: // 90 degree
                        leftPixel = gd[x, y + 1];
                        rightPixel = gd[x, y - 1];
                        break;
                    case 135: // 135 degree
                        leftPixel = gd[x + 1, y + 1];
                        rightPixel = gd[x - 1, y - 1];
                        break;
                }
                if ((gd[x, y] < leftPixel) || (gd[x, y] < rightPixel))
                {
                    nonmax_gd[x, y] = 0; // direction neigbours higher than it
                }
                else
                {
                    nonmax_gd[x, y] = gd[x, y]; // non-maximum suppression
                }
            }
        }
        return nonmax_gd;
    }

    Texture2D Canny_edge(Texture2D originalTexture)
    {
        Texture2D denoise_Texture = Gaussian_denoise(originalTexture);
        double[,] intensity_gradients = Sobel_filter_nonmax(denoise_Texture);
        double highThreshold = 0.2; // high threshold
        double lowThreshold = 0.09; // low threshold
        bool[,] is_edge = new bool[denoise_Texture.width, denoise_Texture.height];
        Texture2D modifiedTexture = new Texture2D(denoise_Texture.width, denoise_Texture.height);

        // 8 8 neighboring offset
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

        // Hysteresis Thresholding
        for (int y = 0; y < denoise_Texture.height; y++)
        {
            for (int x = 0; x < denoise_Texture.width; x++)
            {
                Color pixel_color;
                // lower than hight treshold, not sure
                if (intensity_gradients[x,y] < highThreshold)
                {
                    if (intensity_gradients[x, y] < lowThreshold)
                    {
                        // lower than low treshold, it is not edge
                        is_edge[x, y] = false;
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
                        if (neigh_high == true) // if one neighour is higher than high threshold
                        {
                            is_edge[x, y] = true; // it is edge
                        } else
                        {
                            is_edge[x, y] = false; // it is not edge
                        }
                    }
                }
                else // higher than hight treshold, it is edge
                {
                    is_edge[x, y] = true;
                }
            }
        }

        // Enhance edge
        for (int y = 0; y < denoise_Texture.height; y++)
        {
            for (int x = 0; x < denoise_Texture.width; x++)
            {
                Color pixel_color = Color.white;
                if (is_edge[x, y] == true)
                {
                    pixel_color = Color.black; // edge to black color
                }
                else
                {
                    for (int i = 0; i < 8; ++i) // search 8-neigbours connections
                    {
                        int nx = x + offsets[i, 0];
                        int ny = y + offsets[i, 1];
                        // Within image size
                        if (nx >= 0 && ny >= 0 && nx < denoise_Texture.width && ny < denoise_Texture.height)
                        {
                            if (is_edge[nx, ny] == true)
                            {
                                pixel_color = Color.black; // bolded
                            }
                        }
                    }
                }
                pixel_color.a = 0.7f; // Translucent
                modifiedTexture.SetPixel(x, y, pixel_color);
            }
        }
        modifiedTexture.Apply();
        return modifiedTexture;
    }

    Sprite getedge(Sprite cur_sprite)
    {
        Texture2D originalTexture = cur_sprite.texture; // Image texture
        Texture2D modifiedTexture = Canny_edge(originalTexture); // Get edge by modified Canny edge detection algorithm
        // Create a new sprite as edge sprite
        Sprite edge_sprite = Sprite.Create(modifiedTexture, new Rect(0, 0, originalTexture.width, originalTexture.height), new Vector2(0.5f, 0.5f));
        // Apply edge texture
        modifiedTexture.Apply(); 
        return edge_sprite;
    }

    void Start()
    {
        // precalculate edge of an image
        edge_sp1 = getedge(sp1);
        edge_sp2 = getedge(sp2);
        edge_sp3 = getedge(sp3);
        // Uibq network register
        context = NetworkScene.Register(this);
        // Initialise sprite as none
        cur_sprite = spnone;
        selectnumber = 0;
        use_edge = false;
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage msg)
    {
        // Receive and use transform update messages from remote users
        // Here we use them to update our current position
        var data = msg.FromJson<Message>();
        transform.position = data.position;
        transform.rotation = data.rotation;
    }

    // UI need function
    public void None_image()
    {
        selectnumber = 0;
    }

    public void image_2()
    {
        selectnumber = 1;
    }

    public void image_3()
    {
        selectnumber = 2;
    }
    public void image_4()
    {
        selectnumber = 3;
    }

    public void Original()
    {
        use_edge = false;
    }

    public void edge()
    {
        use_edge = true;
    }
    //


    // Keybord change draft
    public void keybord_operator()
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
    }


    // UI change sprite
    void UI_operate()
    {
        if (selectnumber == 0)
        {
            cur_sprite = spnone;
        } else if (selectnumber == 1){
            cur_sprite = sp1;
        } else if (selectnumber == 2)
        {
            cur_sprite = sp2;
        } else if (selectnumber == 3)
        {
            cur_sprite = sp3;
        }

        if (use_edge)
        {
            if (selectnumber == 0)
            {
                cur_sprite = spnone;
            }
            else if (selectnumber == 1)
            {
                cur_sprite = edge_sp1;
            }
            else if (selectnumber == 2)
            {
                cur_sprite = edge_sp2;
            }
            else if (selectnumber == 3)
            {
                cur_sprite = edge_sp3;
            }
        }
    }

    void Update()
    {
        //keybord_operator(); // If we want to use keybord to controll draft
        UI_operate(); // change sprite by UI
        GetComponent<SpriteRenderer>().sprite = cur_sprite;
    }
}