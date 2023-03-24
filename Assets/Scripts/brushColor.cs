using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class brushColor : MonoBehaviour
{

    
    // Start is called before the first frame update
    void Start()
    {
        //var myRenderer = GetComponent<Renderer>();
        //myRenderer.material.color = Color.blue;
    }

    // Update is called once per frame
    void Update()
    {

            // GetComponent<Renderer>().material.SetColor("_color", Color.blue);
        }

    private void OnCollisionEnter(Collision collision)
    {
      
        if (collision.gameObject.tag == "MixPaint")
        {
            print("collide");
            
            Color color = collision.gameObject.GetComponent<Color>();
            Transform brush = transform.Find("collidePoint");
            Debug.Log(brush);
            var myRenderer = brush.GetComponent<Renderer>();
            myRenderer.material.color = Color.blue;
            

        }
    }
}
