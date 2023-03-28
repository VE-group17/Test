using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Ubiq.Messaging; // new
using Ubiq.XR;

public class Raytest : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject lasthit;
    public Vector3 collision = Vector3.zero;
    public LayerMask layer;
    private LineRenderer lr;
    private NetworkContext context; // new
    private bool owner; // new

    // new
    // 1. Define a message format. Let's us know what to expect on send and recv
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

    private void Start()
    {
        // 2. Register the object with the network scene. This provides a
        // NetworkID for the object and lets it get messages from remote users
        context = NetworkScene.Register(this);

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
        lr.SetWidth(0.01f, 0.01f);
        lr.SetPosition(0, this.transform.position);
        lr.SetPosition(1, this.transform.position + this.transform.forward);
        //myLine.AddComponent<Rigidbody>();
        //Rigidbody Rb = myLine.GetComponent<Rigidbody>();
        //Rb.useGravity = false;
        //Rb.isKinematic = true;
    }


    // new
    public void ProcessMessage(ReferenceCountedSceneGraphMessage msg)
    {
        // 3. Receive and use transform update messages from remote users
        // Here we use them to update our current position
        var data = msg.FromJson<Message>();
        transform.position = data.position;
        transform.rotation = data.rotation;
    }

    // new
    private void FixedUpdate()
    {
        // 4. Send transform update messages if we are the current 'owner'
        context.SendJson(new Message(transform));

    }

    private void LateUpdate()
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
