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
    private string player1;
    private string player2;
    private int myPlayerID;
    private bool released;

    // new
    // 1. Define a message format. Let's us know what to expect on send and recv
    private struct Message
    {
        public Vector3 position;
        public Quaternion rotation;

        public string ownerID;
        public bool isHolding; // someone is holding
        public string player1;
        public string player2;

        public Message(Transform transform, bool isHolding, string ownerID, string player1, string player2)
        {
            this.position = transform.position;
            this.rotation = transform.rotation;

            this.isHolding = isHolding;
            this.ownerID = ownerID;
            this.player1 = player1;
            this.player2 = player2;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        player1 = "";
        player2 = "";
        released = false;
        context = NetworkScene.Register(this);
        myID = AvatarManager.gameObject.transform.GetChild(0).gameObject.name.Substring(12);
    }

    public void ProcessMessage (ReferenceCountedSceneGraphMessage msg)
    {
        // 3. Receive and use transform update messages from remote users
        // Here we use them to update our current position
        var data = msg.FromJson<Message>();
        transform.position = data.position;
        transform.rotation = data.rotation;
    

        if (data.player1 != "" && data.player2 != "")
        {
            var prev_ID = ownerID;
            ownerID = data.ownerID;
            if (owner && (prev_ID != ownerID) && ownerID != "" && myID != ownerID && myID == prev_ID)
            {
                Release();
                // Debug.Log("Released! ");
            }
            //GetComponent<Collider>().isTrigger = true;
            //GetComponent<Rigidbody>().useGravity = false;
        }
        else if ((data.player2 == myID && data.player1 == "") || (data.player1 == myID && data.player2 == ""))
        {
            transform.position = data.position;
            transform.rotation = data.rotation;
            ownerID = myID;

            // GetComponent<Collider>().isTrigger = true;
            //GetComponent<Rigidbody>().useGravity = false;
        }
        else if ((data.player2 != "" && data.player2 != myID && data.player1 == "") || (data.player1 != "" && data.player1 != myID && data.player2 == ""))
        {
            transform.position = data.position;
            transform.rotation = data.rotation;

            ownerID = data.ownerID;

            // GetComponent<Collider>().isTrigger = true;
            // GetComponent<Rigidbody>().useGravity = false;
        }
        else if (player1 == "" && player2 == "")
        {
            // GetComponent<Collider>().isTrigger = false;
            // GetComponent<Rigidbody>().useGravity = false;
        }


    }

    // FixedUpdate 比 Update 早。OnTrigger 和 OnCollision 在 FixedUpdate 里
    private void FixedUpdate()
    {
        if (player1 == myID || player2 == myID || (player2 == "" && player1 == ""))
        {
            // 4. Send transform update messages if we are the current 'owner'
            context.SendJson(new Message(transform,owner,myID, this.player1, this.player2));
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

    }

    // Scene Rendering

    // 回到 FixedUpdate

    void IGraspable.Grasp(Hand controller)
    {
        if (player1 == "")
        {
            player1 = myID;
            myPlayerID = 1;
        }
        else if (player2 == "")
        {
            player2 = myID;
            myPlayerID = 2;
        }
        if (!released)
        {
            owner = true;
            this.controller = controller;

            ownerID = myID;
            FixedUpdate();
            // GetComponent<Rigidbody>().useGravity = false;
        }
        else
        {
            ownerID = "";

            this.controller = null;
        }
    }

    void IGraspable.Release(Hand controller)
    {
        if (myPlayerID == 1)
        {
            player1 = "";
        }
        else if (myPlayerID == 2)
        {
            player2 = "";
        }
        myPlayerID = 0;

        owner = false;
        this.controller = null;
        // released = false;
        // if (!released) 
        // {
        ownerID = "";
        //     Debug.Log("ownerid = kong in release");
        // }
        // GetComponent<Rigidbody>().useGravity = true;

    }

    void Release() //被动release，因为别人拿走了
    {
        owner = false; // new
        this.controller = null;
        released = true;
    }
}