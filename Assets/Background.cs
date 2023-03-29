using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Background : MonoBehaviour
{
    //private GameObject background;
    public GameObject brushContainer; // Import game object brushcontainer
    public Camera canvasCam; // Import game object canvascamera
    public float offset_x; // Offset distance in x direction
    public float offset_y; // Offset distance in y direction
    // Start is called before the first frame update
    void Start()
    {
        offset_x = 0.24f; // In the center of wall
        offset_y = 0.0824f;
        this.transform.parent = brushContainer.transform; // Move to this game object transform
        this.transform.localPosition = new Vector3(-canvasCam.orthographicSize + offset_x, -canvasCam.orthographicSize + offset_y, 0.001f); // Local position move to center of wall
        this.transform.localScale = Vector3.one * 0.05f; // Scale of image
    }

    void Update()
    {
    }
}
