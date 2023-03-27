using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetId : MonoBehaviour
{
    public GameObject AvatarManager;
    public string myID;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        myID = AvatarManager.gameObject.transform.GetChild(0).gameObject.name.Substring(12);
        
    }
}
