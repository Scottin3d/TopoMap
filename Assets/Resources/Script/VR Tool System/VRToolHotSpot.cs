using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class VRToolHotSpot : MonoBehaviour
{

    //this is the code for the VRTool Hot Spot, these are the object along the player's arm which can be used to quickly select the basic tools for the player.
    //these spots act similarly to HandUIActivateable in that they wait to be triggered by the UI collider and then tell a controlling class when triggered.

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
        Debug.Log("in trigger stay " + other.gameObject == UICollider + " " + SteamVR_Actions.default_GrabGrip.state + ".");
        if(other.gameObject == UICollider && SteamVR_Actions.default_GrabGrip.state)
        {
            //trigger reciever
            reciever.recieveHotSpotInput(gameObject);
        }
    }
}
