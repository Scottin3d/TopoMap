using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class Handaction : MonoBehaviour
{

    //public SteamVR_Action_Vector2 moveinput; //HTC Vive trackpad input (for smooth locomotion)
    public Transform player; //transform of the player to move
    public Transform controlHand; //hand for movement control
    //public SteamVR_Input_Sources inputDevice; //device to pull data from
    public float speed = 5f; //speed of player movement

    // Start is called before the first frame update
    void Start()
    {
        //moveinput = new SteamVR_Action_Vector2();
    }

    // Update is called once per frame
    void Update()
    {
        //Vector2 movement = moveinput.GetAxis(inputDevice);
        //Vector2 movement = moveinput.GetAxis(inputDevice);
        //movement = SteamVR_Input.GetVector2("platformer", "Move", SteamVR_Input_Sources.Any, true);
        //movement = SteamVR_Actions.hTCTrack_thumbpad.GetAxis(inputDevice);
        //movement = SteamVR_Actions.platformer_Move.GetAxisDelta(inputDevice);
        //movement = SteamVR_Actions.platformer_Move.GetAxis(inputDevice);
        //movement = SteamVR_Input.
        //Debug.Log(controlHand.eulerAngles);
        if (SteamVR_Actions.default_GrabPinch.state)
        {
            //Debug.Log("is set to true");

            //Debug.Log("movement is:" + movement);
            Quaternion handrotation = controlHand.rotation;
            Vector3 movement = handrotation.eulerAngles;

            float move = Mathf.Sin((movement.y * Mathf.Deg2Rad)) * speed * Time.deltaTime;
            Vector3 tempPos = player.position;
            tempPos.x += move;
            move = Mathf.Cos((movement.y * Mathf.Deg2Rad)) * speed * Time.deltaTime;
            tempPos.z += move;
            player.position = tempPos;
        }
    }


    private void whenToMove(Vector2 moveVal)
    {

    }
}
