using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Background : MonoBehaviour
{
    private GameObject background;
    public GameObject brushContainer;
    public Camera canvasCam;
    // Start is called before the first frame update
    void Start()
    {
        background = (GameObject)Instantiate(Resources.Load("TexturePainter-Instances/UCL"));
        background.transform.parent = brushContainer.transform;
        background.transform.localPosition = new Vector3(-canvasCam.orthographicSize, -canvasCam.orthographicSize, 0.001f);
        background.transform.localScale = Vector3.one * 0.05f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
