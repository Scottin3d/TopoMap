using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class VRToolSwipe : MonoBehaviour
{

    //this is the script for the behavior of the swiping zones of the toolbox that the VR player uses. They are very similar to
    //HandUIActivateable, as they just wait for the UI collider, and then trigger the tool selector class when they collide.

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

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == UICollider) //to avoid other things causing a second swipe
        {
            activated = false;
        }
    }
}
