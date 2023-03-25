using UnityEngine;
// using UnityEngine.Events;
using Ubiq.XR;
using Ubiq.Messaging; // new

public class PickUp : MonoBehaviour, IGraspable
{
    private NetworkContext context; // new
    private bool owner; // new
    private Hand controller;
    public Vector3 hand_bucket_offset;

    // private InputAction rotateAction;
    private Quaternion previousRotation;
    // private Quaternion rotated_angle;

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

    // new
    private void Start()
    {
        // 2. Register the object with the network scene. This provides a
        // NetworkID for the object and lets it get messages from remote users
        context = NetworkScene.Register(this);
    }

    // new
    public void ProcessMessage (ReferenceCountedSceneGraphMessage msg)
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
        if (owner)
        {
            // 4. Send transform update messages if we are the current 'owner'
            context.SendJson(new Message(transform));
            GetComponent<Collider>().isTrigger = true;
        }
        else
        {
            GetComponent<Collider>().isTrigger = false;
        }
    }

    private void LateUpdate()
    {
        if (controller)
        {
            Quaternion currentRotation = controller.transform.rotation;
            Quaternion rotationChange = currentRotation * Quaternion.Inverse(previousRotation);
            previousRotation = currentRotation;
            
            transform.rotation *= rotationChange;

            transform.position = controller.transform.position + hand_bucket_offset;
            // transform.rotation = controller.transform.rotation;

        }
    }

    void IGraspable.Grasp(Hand controller)
    {
        Debug.Log("grasp");
        // 5. Define ownership as 'who holds the item currently'
        owner = true; // new
        this.controller = controller;

        previousRotation = controller.transform.rotation;

        GetComponent<Rigidbody>().useGravity = false;
    }

    void IGraspable.Release(Hand controller)
    {
        print("release");
        // As 5. above, define ownership as 'who holds the item currently'
        owner = false; // new
        this.controller = null;
        GetComponent<Rigidbody>().useGravity = true;
    }

    // private void OnEnable()
    // {
    //     // rotationAction = new InputAction("Rotation",binding:"<XRController}{LeftHand}/rotation");
    //     // rotationAction.Enable();
    //     previousRotation = transform.rotation;
    // }
    // private void OnDisable()
    // {
    //     rotateAction.Disable();
    // }
    // private void Update()
    // {
    //     Quaternion currentRotation = rotateAction.ReadValue<Quaternion>();
    //     Quaternion rotationChange = currentRotation * Quaternion.Inverse(previousRotation);
    //     previousRotation = currentRotation;

    //     transform.rotation *= rotationChange;

    // }

     // Note about ownership: 'ownership' is just one way of designing this
     // kind of script. It's sometimes a useful pattern, but has no special
     // significance outside of this file or in Ubiq more generally.
}