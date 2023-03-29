using UnityEngine;
using Ubiq.XR;
using Ubiq.Messaging; // new

public class HoldRoller : MonoBehaviour, IGraspable
{    
    // context holds address of this object on the network, allow send messages
    private NetworkContext context; 
    // if the local user is the owner of this object
    private bool owner; 
    private Hand controller;
    // for developer to adjust the offset between hand and object
    public Vector3 hand_roller_offset;
    
    // drag the GameObject in the unity to have the access of avatar spawned
    public GameObject AvatarManager;
    // store different ID and keep track of the ownerID for the ability of grasping object directly from one's hand
    public string myID;
    public string ownerID;
    private string player1;
    private string player2;
    private int myPlayerID;
    // if the object that local user currently holding is grasped by another user, it passively release
    private bool released;

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
    }

    public void ProcessMessage (ReferenceCountedSceneGraphMessage msg)
    {
        myID = AvatarManager.gameObject.transform.GetChild(0).gameObject.name.Substring(12);
        // 3. Receive and use transform update messages from remote users
        // Here we use them to update our current position
        var data = msg.FromJson<Message>();
        transform.position = data.position;
        transform.rotation = data.rotation;
    
        // if both are holding
        if (data.player1 != "" && data.player2 != "")
        {
            // track who is the one previously own the object
            var prev_ID = ownerID;
            // assign the new ownerID, since one will process message only if others are sending message
            // and the purpose of this is to check if I was previously holding it and other is taking from us
            // if that is the case, passively release it
            ownerID = data.ownerID;
            if (owner && (prev_ID != ownerID) && ownerID != "" && myID != ownerID && myID == prev_ID)
            {
                Release();
                // Debug.Log("Released! ");
            }
            //GetComponent<Collider>().isTrigger = true;
            //GetComponent<Rigidbody>().useGravity = false;
        }
        // if I am the only one who holding it
        else if ((data.player2 == myID && data.player1 == "") || (data.player1 == myID && data.player2 == ""))
        {
            transform.position = data.position;
            transform.rotation = data.rotation;
            ownerID = myID;

            // GetComponent<Collider>().isTrigger = true;
            //GetComponent<Rigidbody>().useGravity = false;
        }
        // if I am not holding it, other does
        else if ((data.player2 != "" && data.player2 != myID && data.player1 == "") || (data.player1 != "" && data.player1 != myID && data.player2 == ""))
        {
            transform.position = data.position;
            transform.rotation = data.rotation;

            ownerID = data.ownerID;

            // GetComponent<Collider>().isTrigger = true;
            // GetComponent<Rigidbody>().useGravity = false;
        }
        // if no one is holding
        else if (player1 == "" && player2 == "")
        {
            // GetComponent<Collider>().isTrigger = false;
            // GetComponent<Rigidbody>().useGravity = false;
        }


    }

    // FixedUpdate runs earlier than Update. OnTrigger and OnCollision update in FixedUpdate
    private void FixedUpdate()
    {
        myID = AvatarManager.gameObject.transform.GetChild(0).gameObject.name.Substring(12);
        // if I am the one holding it, or no one is holding it. pass the ownership
        if (player1 == myID || player2 == myID || (player2 == "" && player1 == ""))
        {
            // 4. Send transform update messages if we are the current 'owner'
            context.SendJson(new Message(transform,owner,myID, this.player1, this.player2));
        }
    }

    // LateUpdate is after Update
    private void LateUpdate()
    {
        // controller will be true only if user do the Grasp action, so when user grasp object, the object follows controller transform
        if (controller)
        {
            transform.position = controller.transform.position + hand_roller_offset;
            transform.rotation = controller.transform.rotation;
        }

    }

    // Scene Rendering

    // play is grasping
    void IGraspable.Grasp(Hand controller)
    {
        if (player1 == "") // if previously no one is holding it
        {
            player1 = myID;
            myPlayerID = 1;
        }
        else if (player2 == "") // if someone is already holding it, place me to the second order
        {
            player2 = myID;
            myPlayerID = 2;
        }
        // if no one is taking from me
        if (!released)
        {
            owner = true;
            this.controller = controller;

            ownerID = myID;
            FixedUpdate();
            // GetComponent<Rigidbody>().useGravity = false;
        }
        else // if someone is taking from me
        {
            ownerID = "";

            this.controller = null;
        }
    }

    void IGraspable.Release(Hand controller) // release controll by the contoller trigger
    {
        // empty my playerID
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

    void Release() //passively release, since someone taking from me
    {
        owner = false;
        this.controller = null;
        released = true;
    }
}