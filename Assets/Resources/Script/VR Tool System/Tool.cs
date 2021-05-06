using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Tool : MonoBehaviour
{
    protected bool isActive;
    //tool is an abstract class meant to represent the various tools the VR player can use (like roads and markers).
    //it defines an activate and deactivate method to make the tool selection script easier to write.
    public void activate()
    {
        isActive = true;
    }

    public void deactivate()
    {
        isActive = false;
    }

    public bool isCurrentlyActive()
    {
        return isActive;
    }
}
