using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Messaging; // new
using System.Runtime.ConstrainedExecution;
using System;
using System.Security.Cryptography;
using UnityEngine.UI;


public class DraftSelector : MonoBehaviour
{
    public Button btn;
    public Sprite btn_sprite;
    public GameObject draft_sprite;

    // Start is called before the first frame update
    void Start()
    {
        // btn = GetComponent<Button>();
        btn_sprite = GetComponent<Image>().sprite;
        btn.onClick.AddListener(TaskOnClick);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void TaskOnClick()
    {
        draft_sprite.GetComponent<SpriteRenderer>().sprite = btn_sprite;
    }
}
