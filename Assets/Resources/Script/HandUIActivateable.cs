using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandUIActivateable : MonoBehaviour
{
    /*
     * This is a short-sighted component for the buttons on the left hand in the VR demo. This script will change the color of the button when the designated hand is within close proximity to it,
     * but this behavior should be able to be applied to other uses in the future (though likely with some changes).
     */

    public Transform otherHand; //this is the hand that the button should interact with, all this transform needs to represent is the position of the hand, and does not need to be attatched to SteamVR's hand necessarily.

    public float triggerDistance = 0.01f; //this is the distance the other hand needs to be from the button to trigger it.

    private bool isOn = false; //bool to know the state of the button.


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        checkHandDistance();
    }

    public void checkHandDistance()
    {
        Vector3 distance = this.transform.position - otherHand.position;
        //Debug.Log("buttonlocation: " + otherHand.position);
        if (!isOn)
        {
            if (distance.magnitude < triggerDistance)
            {
                triggerOnState();
            }
        }
        else
        {
            if (distance.magnitude > triggerDistance)
            {
                triggerOffState();
            }
        }
    }

    public void triggerOnState()
    {
        isOn = true;
        this.GetComponent<Renderer>().material.color = new Color(255, 0, 0);
    }

    public void triggerOffState()
    {
        isOn = false;
        this.GetComponent<Renderer>().material.color = new Color(255, 255, 255);
    }
}
