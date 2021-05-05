﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControlInWaitingRoom : MonoBehaviour
{
    private float WalkSpeed;
    private float JumpSpeed;
    private CharacterController PlayerController;
    private Vector3 NewMove;

    void Awake()
    {
        this.transform.position = new Vector3(-11, 29.4f, -8100);
        PlayerController = this.GetComponent<CharacterController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        this.transform.position = new Vector3(8, 30, 8);
        WalkSpeed = 5;
        JumpSpeed = 3;
    }

    // Update is called once per frame
    void Update()
    {
        ChangePosition();
    }

    //private void FixedUpdate()
    //{
    //    ChangePosition();
    //}

    private void ChangePosition()
    {
        float XDirection, ZDirection;

        if (PlayerController.isGrounded)
        {
            XDirection = Input.GetAxisRaw("Horizontal");
            ZDirection = Input.GetAxisRaw("Vertical");
            NewMove = transform.right * XDirection + transform.forward * ZDirection;
            if (Input.GetAxis("Jump") == 1)
            {
                NewMove.y = JumpSpeed;
            }
        }
        float g = 9;
        NewMove.y = NewMove.y - g * Time.deltaTime;

        PlayerController.Move(NewMove * WalkSpeed * Time.deltaTime);
    }
}
