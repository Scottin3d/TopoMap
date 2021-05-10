using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ToolBox : MonoBehaviour
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
}
