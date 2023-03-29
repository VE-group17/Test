using UnityEngine;
using Ubiq.XR;
using Ubiq.Messaging; // new

public class PickUp : MonoBehaviour, IGraspable
{
    // context holds address of this object on the network, allow send messages
    private NetworkContext context;
    // if the local user is the owner of this object
    private bool owner; 
    private Hand controller;
    // for developer to adjust the offset between hand and object
    public Vector3 hand_bucket_offset;

    private Quaternion previousRotation;

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
    }

    public void ProcessMessage (ReferenceCountedSceneGraphMessage msg)
    {
        // 3. Receive and use transform update messages from remote users
        // Here we use them to update our current position
        var data = msg.FromJson<Message>();
        transform.position = data.position;
        transform.rotation = data.rotation;
    }

    private void FixedUpdate()
    {
        // only send message when local user is holding the object
        if (owner)
        {
            // 4. Send transform update messages if we are the current 'owner'
            context.SendJson(new Message(transform));
            // if user is holding, set collider trigger to be true such that is will not give the collider on the user body and hand an unwanted force
            GetComponent<Collider>().isTrigger = true;
        }
        else
        {
            GetComponent<Collider>().isTrigger = false;
        }
    }

    private void LateUpdate()
    {
        // controller will be true only if user do the Grasp action, so when user grasp object, the object follows controller transform
        if (controller)
        {
            // this pick up function is for picking up and pooling the paints in bucket, so the rotation changes will be more nature if the rotation apply to previous rotation
            Quaternion currentRotation = controller.transform.rotation;
            Quaternion rotationChange = currentRotation * Quaternion.Inverse(previousRotation);
            previousRotation = currentRotation;
            transform.rotation *= rotationChange;

            transform.position = controller.transform.position + hand_bucket_offset;
        }
    }

    void IGraspable.Grasp(Hand controller)
    {
        Debug.Log("grasp");
        // 5. Define ownership as 'who holds the item currently'
        owner = true; 
        this.controller = controller;
        previousRotation = controller.transform.rotation;
        // if user grasp the object, disable the gravity
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


}