﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed;
    public GameObject Island;

    private Rigidbody2D rbplayer;
    private Vector2 moveVelocity;


    // Start is called before the first frame update
    void Start()
    {
        rbplayer = GetComponent<Rigidbody2D>();

    }

    // Update is called once per frame
    void Update()
    {

        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        moveVelocity = moveInput.normalized * speed;
        
    }

    private void FixedUpdate()
    {
        rbplayer.MovePosition(rbplayer.position + moveVelocity * Time.fixedDeltaTime);
    }
}