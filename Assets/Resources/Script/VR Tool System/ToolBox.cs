using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class ToolBox : MonoBehaviour
{
    protected bool isActive;
    //toolbox is the controlling script for the toolbox the player pulls their tools out of.

    public GameObject UICollider;
    public VRToolSelector reciever;

    public void activate()
    {
        gameObject.GetComponent<Renderer>().enabled = true;
        isActive = true;
    }

    public void deactivate()
    {
        gameObject.GetComponent<Renderer>().enabled = false;
        isActive = false;
    }

    public bool isCurrentlyActive()
    {
        return isActive;
    }


    private void OnTriggerStay(Collider other)
    {
        //Debug.Log("is it the collider? " + (other.gameObject == UICollider));
        //Debug.Log("in trigger stay gripstate = " + SteamVR_Actions.default_GrabGrip.state + ".");
        if (other.gameObject == UICollider && SteamVR_Actions.default_GrabGrip.state)
        {
            //trigger reciever
            reciever.BoxActivated();
        }
    }
}
