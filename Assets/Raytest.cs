using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Raytest : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject lasthit;
    public Vector3 collision = Vector3.zero;
    public LayerMask layer;
    private LineRenderer lr;

    void Start()
    {
        GameObject myLine = new GameObject();
        Material lineMaterial = new Material(Shader.Find("Transparent/Diffuse"));
        lineMaterial.color = new Color(1f, 1f, 1f, 0.5f);
        lineMaterial.SetInt("_ZWrite", 0);
        lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        myLine.AddComponent<LineRenderer>();
        myLine.transform.position = this.transform.position;
        lr = myLine.GetComponent<LineRenderer>();
        lr.material = lineMaterial;
        lr.SetWidth(0.1f, 0.1f);
        lr.SetPosition(0, this.transform.position);
        lr.SetPosition(1, this.transform.position + this.transform.forward);

    }

    // Update is called once per frame

    //void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0.2f)
    //{
    //    //GameObject.Destroy(myLine, duration);
    //}

    void Update()
    {
        var ray = new Ray(this.transform.position, this.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100))
        {
            lasthit = hit.transform.gameObject;
            collision = hit.point;
        }
        lr.SetPosition(0, this.transform.position);
        lr.SetPosition(1, collision);
    }

    
}
