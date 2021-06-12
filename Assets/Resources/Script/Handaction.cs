using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class Handaction : MonoBehaviour
{
    //Handaction is the class which handles the VR player's smooth locomotion.
    //this class does two main things to achieve this.
    //- first, it takes the player's right hand's direction, and when the "pinch" status is activated,
    //  moves the VR player in the direction of their right hand. this includes vertical movement if the
    //  VR player has toggled it.
    //- second, it keeps track of the player's set speed. the player moves at whatever their speed is set
    //  to, in unity units per second (which roughly scales into meters per second).

    public Transform player; //transform of the player to move
    public Transform controlHand; //hand for movement control
    public static float speed = 5f; //speed of player movement

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    // Update currently checks for the "pinch" state of both hands, and if it is active,
    // will perform the action of moving the player.
    void Update()
    {
        
        //Debug.Log(controlHand.eulerAngles);
        if (SteamVR_Actions.default_GrabPinch.state)
        {
            //Debug.Log("is set to true");

            //Debug.Log("movement is:" + movement);
            Quaternion handrotation = controlHand.rotation;
            Vector3 movement = handrotation.eulerAngles;

            //horizontal movement
            float move = Mathf.Sin((movement.y * Mathf.Deg2Rad)) * speed * Time.deltaTime;
            Vector3 tempPos = player.position;
            tempPos.x += move;
            move = Mathf.Cos((movement.y * Mathf.Deg2Rad)) * speed * Time.deltaTime;
            tempPos.z += move;

            //vertical movement
            if (StaticVRVariables.allowVerticalVRMovement)
            {
                move = -Mathf.Sin((movement.x * Mathf.Deg2Rad)) * speed * Time.deltaTime;
                tempPos.y += move;
            }

            player.position = tempPos;
        }
    }

    //this function will increase the speed of the player, and is referenced in the
    //VRUIController script.
    public static void increaseSpeed(float increaseAmount)
    {
        speed += increaseAmount;
    }

    //this function will decrease the speed of the player, and is referenced in the
    //VRUIController script.
    public static void decreaseSpeed(float decreaseAmount)
    {
        speed -= decreaseAmount;
    }

}
