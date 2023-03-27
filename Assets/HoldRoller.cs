using UnityEngine;
// using UnityEngine.Events;
using Ubiq.XR;
using Ubiq.Messaging; // new

public class HoldRoller : MonoBehaviour, IGraspable
{
    private NetworkContext context; // new
    private bool owner; // new
    private Hand controller;
    public Vector3 hand_roller_offset;
    
    public GameObject AvatarManager;
    public string myID;
    public string ownerID;

    private bool forceRelease;

    // new
    // 1. Define a message format. Let's us know what to expect on send and recv
    private struct Message
    {
        public Vector3 position;
        public Quaternion rotation;

        public string ownerID;

        public Message(Transform transform, string ownerID)
        {
            this.position = transform.position;
            this.rotation = transform.rotation;
            this.ownerID = ownerID;
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
        ownerID = data.ownerID;
        
    }

    // FixedUpdate 比 Update 早。OnTrigger 和 OnCollision 在 FixedUpdate 里
    private void FixedUpdate()
    {
        myID = AvatarManager.gameObject.transform.GetChild(0).gameObject.name.Substring(12);
        Debug.Log("my ID: "+myID);

        if (owner)
        {
            // 4. Send transform update messages if we are the current 'owner'
            context.SendJson(new Message(transform,myID));

            GetComponent<Collider>().isTrigger = true;
            // GetComponent<Rigidbody>().useGravity = false;
        }
        else
        {
            GetComponent<Collider>().isTrigger = false;
            // GetComponent<Rigidbody>().useGravity = true;
        }
    }

    // Update 在 FixedUpdate 之后，包含了 ProcessAnimation
    // private void Update()
    // {
    //     if (owner)
    //     {
    //         // 4. Send transform update messages if we are the current 'owner'
    //         // context.SendJson(new Message(transform,owner));
    //         GetComponent<Collider>().isTrigger = true;
    //         GetComponent<Rigidbody>().useGravity = false;
    //     }
    //     else
    //     {
    //         GetComponent<Collider>().isTrigger = false;
    //         GetComponent<Rigidbody>().useGravity = true;
    //     }
    // }

    // LateUpdate 在 Update 之后
    private void LateUpdate()
    {
        if (controller)
        {
            transform.position = controller.transform.position + hand_roller_offset;
            transform.rotation = controller.transform.rotation;
        }
        if(owner & (myID != ownerID))
        {
            forceRelease = true;
            Release();
        }
    }

    // Scene Rendering

    // 回到 FixedUpdate

    void IGraspable.Grasp(Hand controller)
    {
        // if(!pass)
        // {
        //     Debug.Log("grasp");
        //     // 5. Define ownership as 'who holds the item currently'
        //     owner = true; // new
        //     this.controller = controller;

        //     ownerID = myID;
        // }
        Debug.Log("grasp");
        // 5. Define ownership as 'who holds the item currently'
        owner = true; // new
        this.controller = controller;

        ownerID = myID;
        GetComponent<Rigidbody>().useGravity = false;

    }

    void IGraspable.Release(Hand controller)
    {
        print("release");
        // As 5. above, define ownership as 'who holds the item currently'
        owner = false; // new
        this.controller = null;

        ownerID = "";
        GetComponent<Rigidbody>().useGravity = true;

    }

    void Release() //被动release，因为别人拿走了
    {
        owner = false; // new
        this.controller = null;
    }
}