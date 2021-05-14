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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay(Collider other)
    {
        //Debug.Log("is it the collider? " + (other.gameObject == UICollider));
        //Debug.Log("in trigger stay gripstate = " + SteamVR_Actions.default_GrabGrip.state + ".");
        if (other.gameObject == UICollider && SteamVR_Actions.default_GrabGrip.state)
        {
            //trigger reciever
            reciever.recieveSwipeInput(gameObject);
        }
    }
}
