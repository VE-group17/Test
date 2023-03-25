using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using static System.TimeZoneInfo;
using Ubiq.Messaging;

public class Combine : MonoBehaviour
{
    private NetworkContext context;

    private float Blue_num_particles;
    private float Green_num_particles;
    private float Red_num_particles;
    private float total_num;
    private Renderer myrenderer;
    private Color myColor;
    private float threshold;
    private float timespeed;
    private bool is_mixing;

    private struct Message
    {
        public Color color;
        public Message(Color color)
        {
            this.color = color;
        }
    }

    void Start()
    {
        context = NetworkScene.Register(this);

        myrenderer = GetComponent<Renderer>();  
        myColor = myrenderer.material.color;
        threshold = 200;
        timespeed = 10f;
   
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage msg)
    {
        var data = msg.FromJson<Message>();
        myColor = data.color;
    }

    private void LateUpdate()
    {
        if(is_mixing)
        {
            context.SendJson(new Message(myColor));
        }
    }
    
    void OnParticleCollision(GameObject other)
    {
        is_mixing = true;

        total_num++;
        if (myColor[3] < 1f)
        {
            myColor[3] = total_num / threshold;
        }
        else timespeed = 10f;



        if (other.CompareTag("blueDrop") && Blue_num_particles < (threshold+timespeed/100))
        {
            Blue_num_particles++;
            
            myColor[2] = Blue_num_particles / threshold;
            
            myrenderer.material.SetColor("_Color", Color.Lerp(myrenderer.material.color, myColor, Time.deltaTime*timespeed));
            myColor = myrenderer.material.color;
        }

        if (other.CompareTag("redDrop") && Red_num_particles < (threshold + timespeed / 100))
        {
            Red_num_particles++;
            total_num++;
            myColor[0] = Red_num_particles / threshold;
            myrenderer.material.SetColor("_Color", Color.Lerp(myrenderer.material.color, myColor, Time.deltaTime * timespeed));
            myColor = myrenderer.material.color;
        }
        if (other.CompareTag("greenDrop") && Green_num_particles < (threshold + timespeed / 100))
        {
            Green_num_particles++;
            total_num++;
            myColor[1] = Green_num_particles / threshold;
            myrenderer.material.SetColor("_Color", Color.Lerp(myrenderer.material.color, myColor, Time.deltaTime * timespeed));
            myColor = myrenderer.material.color;
        }

     
        //Rigidbody rb = other.GetComponent<Rigidbody>();
        // int i = 0;

        //while (i < numCollisionEvents)
        //{
        //    if (rb)
        //    {
        //        Vector3 pos = collisionEvents[i].intersection;
        //        Vector3 force = collisionEvents[i].velocity * 10;
        //        rb.AddForce(force);
        //    }
        //    i++;
        //}
    }

    void OnParticleCollisionExit(GameObject other)
    {
        is_mixing = false;
        Debug.Log("OnParticleCollisionExit");
    }

    void Transparent2Opaque(Color color)
    {

    }
}