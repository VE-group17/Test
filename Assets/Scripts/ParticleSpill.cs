using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSpill : MonoBehaviour
{
    ParticleSystem myParticleSystem;
    //private bool isPouring = false;
    public int pourThreshold = 45;
    public Transform origin;
    private Quaternion initialRotation;
    private GameObject bucket;
    private float r;
    private float initial_z;
    bool last_pouring = false;
    bool isPouring = false;

    public GameObject mLiquid;
    // Start is called before the first frame update
    void Start()
    {
        myParticleSystem = GetComponent<ParticleSystem>();
        bucket = GameObject.FindGameObjectWithTag("Bucket");
        initialRotation = transform.rotation;
       // r = bucket.transform.localScale.x / 2;
       // initial_z = (float)(bucket.transform.localScale.z *0.5);

        //myParticleSystem.transform.localPosition = origin.localPosition + new Vector3(0, initial_z, 0);


        //print(myParticleSystem.transform.localPosition);
    }
    // Update is called once per frame
    void Update()
    {

        bool pourCheck = (origin.up.y * Mathf.Rad2Deg) < pourThreshold;

        // transform.rotation = initialRotation;

        if (pourCheck)
        {
            isPouring = true;
            //   print(bucket.transform.localRotation.y) ;
            if (!myParticleSystem.isPlaying) myParticleSystem.Play();
            // print(transform.up);

            if (last_pouring != isPouring)
            {
                transform.rotation = initialRotation;
               // if (origin.up.x < 0) transform.Rotate(new Vector3(0, 180, 0), Space.Self);

               // else if (origin.up.z < 0) transform.Rotate(new Vector3(0, 90, 0), Space.Self);

              //  else if (origin.up.z > 0) transform.Rotate(new Vector3(0, -90, 0), Space.Self);
             //   else transform.Rotate(new Vector3(0, 0, 0), Space.Self);
                last_pouring = true;

            }
        }
        else
        {
            //transform.rotation = initial_coord.rotation;

            isPouring = false;
            if (myParticleSystem.isPlaying) myParticleSystem.Stop();
           // if (last_pouring != isPouring) transform.rotation = initialRotation;

            last_pouring = false;


        }

    }

}
