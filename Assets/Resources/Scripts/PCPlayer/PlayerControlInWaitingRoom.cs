using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControlInWaitingRoom : MonoBehaviour
{
    private float WalkSpeed;
    private float JumpSpeed;
    private CharacterController PlayerController;
    private Vector3 NewMove;

    // Start is called before the first frame update
    void Start()
    {
        PlayerController = this.GetComponent<CharacterController>();
        PlayerController.enabled = false;
        this.transform.position = new Vector3(0, 43, -810);
        WalkSpeed = 6;
        JumpSpeed = 3;
        PlayerController.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        ChangeWalkSpeed();
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

    private void ChangeWalkSpeed()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            WalkSpeed = 12;
            JumpSpeed = 6;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            WalkSpeed = 6;
            JumpSpeed = 3;
        }
    }
}
