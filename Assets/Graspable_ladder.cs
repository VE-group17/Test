using UnityEngine;
using Ubiq.XR;
using Ubiq.Messaging;

// Implement Graspable interface, part of Ubiq XR interaction
// You can use any interaction toolkit you like with Ubiq!
// For the sake of keeping this tutorial simple, we use our simple in-built
// option.
public class Graspable_ladder : MonoBehaviour, IGraspable
{
    private Hand controller;
    private NetworkContext context; // new
    private bool owner; // new

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
        if (owner)
        {
            // 4. Send transform update messages if we are the current 'owner'
            context.SendJson(new Message(transform));
        }
    }


    private void LateUpdate()
    {
        if (controller)
        {
            Vector3 position_new = controller.transform.position;
            position_new.y = 0.0f;
            transform.position = position_new;
        }
    }

    void IGraspable.Grasp(Hand controller)
    {
        // 5. Define ownership as 'who holds the item currently'
        owner = true; // new
        this.controller = controller;
    }

    void IGraspable.Release(Hand controller)
    {
        // As 5. above, define ownership as 'who holds the item currently'
        owner = false; // new
        this.controller = null;
    }

    // Note about ownership: 'ownership' is just one way of designing this
    // kind of script. It's sometimes a useful pattern, but has no special
    // significance outside of this file or in Ubiq more generally.
}