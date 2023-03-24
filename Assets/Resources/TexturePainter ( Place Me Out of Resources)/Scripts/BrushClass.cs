using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.XR;
using Ubiq.Messaging; // new
public class BrushClass : MonoBehaviour
{
    public Sprite sp1, sp2, spnone;
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
    void Start()
    {
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
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.U))
        {
            GetComponent<SpriteRenderer>().sprite = sp1;
        }
        if (Input.GetKey(KeyCode.I))
        {
            GetComponent<SpriteRenderer>().sprite = sp2;
        }
        if (Input.GetKey(KeyCode.P))
        {
            GetComponent<SpriteRenderer>().sprite = spnone;
        }
    }
}
