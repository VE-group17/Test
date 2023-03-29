using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Background : MonoBehaviour
{
    //private GameObject background;
    public GameObject brushContainer;
    public Camera canvasCam;
    public float offset_x;
    public float offset_y;
    // Start is called before the first frame update
    void Start()
    {
        offset_x = 0.24f;
        offset_y = 0.0824f;
        this.transform.parent = brushContainer.transform;
        this.transform.localPosition = new Vector3(-canvasCam.orthographicSize + offset_x, -canvasCam.orthographicSize + offset_y, 0.001f);
        this.transform.localScale = Vector3.one * 0.05f;
    }

    // Update is called once per frame
    void Update()
    {
    }
}
