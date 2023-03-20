using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.XR;

public class Move_ladder : MonoBehaviour
{
    // Start is called before the first frame update
    public float movementSpeed = 2f;
    bool inside = false;
    void Start()
    {
        inside = false;
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Player")
        {
            inside = !inside;
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if (col.gameObject.tag == "Player")
        {
            inside = !inside;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (inside)
        {
            GameObject temp = GameObject.Find("Player");
            Transform PlayerTransform = temp.GetComponent<Transform>();

            if (Input.GetKey(KeyCode.J))
            {
                Vector3 movement = new Vector3(0f, 0f, 0f);
                if (Input.GetKey(KeyCode.A))
                {
                    movement += new Vector3(-1f, 0f, 0f);
                }
                if (Input.GetKey(KeyCode.D))
                {
                    movement += new Vector3(1f, 0f, 0f);
                }
                if (Input.GetKey(KeyCode.W))
                {
                    movement += new Vector3(0f, 0f, 1f);
                }
                if (Input.GetKey(KeyCode.S))
                {
                    movement += new Vector3(0f, 0f, -1f);
                }
                movement = movement.normalized * (movementSpeed) * Time.deltaTime;
                movement.y = 0f;
                transform.position += movement;

            }
        }
    }
}
