using UnityEngine;
using Ubiq.XR;
using Ubiq.Messaging;

// Implement Graspable interface, part of Ubiq XR interaction
// You can use any interaction toolkit you like with Ubiq!
// For the sake of keeping this tutorial simple, we use our simple in-built
// option.
public class Graspable_ladder : MonoBehaviour, IGraspable
{
    public bool grap_ladder = false; // If someone grab ladder (It used in player movement script -- make player cannont move and grab at same time)
    private Hand controller; // VR hand controller
    private NetworkContext context; // Network
    private bool owner; // If someone grab ladder 
    private Vector3 hand_offset;

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
        grap_ladder = false;
        hand_offset = new Vector3(-1f, 0f, 0.3f);
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage msg)
    {
        // 3. Receive and use transform update messages from remote users
        // Here we use them to update our current position
        var data = msg.FromJson<Message>();
        transform.position = data.position;
        transform.rotation = data.rotation;
    }

    // 4. Send transform update messages if we are the current 'owner'
    private void FixedUpdate()
    {
        if (owner)
        {
            context.SendJson(new Message(transform));
        }
    }

    // Update position of ladder
    private void LateUpdate()
    {
        if (controller)
        {
            Vector3 position_new = controller.transform.position + hand_offset; // same position with hand with offset
            position_new.y = transform.position.y; // cannot change y direction, just move on ground
            transform.position = position_new;  // update
        }
    }

    void IGraspable.Grasp(Hand controller)
    {
        // 5. Define ownership as 'who holds the item currently'
        owner = true;
        this.controller = controller;
        grap_ladder = true;
    }

    void IGraspable.Release(Hand controller)
    {
        // As 5. above, define ownership as 'who holds the item currently'
        owner = false;
        this.controller = null;
        grap_ladder = false;
    }
}