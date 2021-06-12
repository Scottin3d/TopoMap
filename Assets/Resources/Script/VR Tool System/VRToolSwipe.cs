using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class VRToolSwipe : MonoBehaviour
{

    //this is the script for the behavior of the swiping zones of the toolbox that the VR player uses. They are very similar to
    //HandUIActivateable, as they just wait for the UI collider, and then trigger the tool selector class when they collide.

    //these public variables need to be set by the controlling VRToolSelector script when swipe hitboxes are created.
    public VRToolSelector reciever;
    public GameObject UICollider;
    public VRGestureInterpretation.gesture triggerGesture;

    private bool activated = false; //bool to prevent too much swiping at once

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //when the player swipes through the swipe hitbox, this function will pick up on it and activate
    //the behavior by signaling VRToolSelector.
    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("is it the collider? " + (other.gameObject == UICollider));
        //Debug.Log("in trigger stay gripstate = " + SteamVR_Actions.default_GrabGrip.state + ".");
        if (other.gameObject == UICollider && (VRGestureInterpretation.reference.GetCurrentGesture() == triggerGesture) && !activated)
        {
            //trigger reciever
            reciever.recieveSwipeInput(gameObject);
            activated = true;
        }
    }

    //this function will reset the ability to swipe with this swipe hitbox
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == UICollider) //to avoid other things causing a second swipe
        {
            activated = false;
        }
    }
}
