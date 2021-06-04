using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandUIActivateable : MonoBehaviour
{
    //debug boolean which enables or disables the buttons turning
    //green when enabled, and red when disabled.
    private const bool DEBUG_COLORS = false;

    //this class is a component for the UI buttons that the VR player uses.
    //when this class is triggered by the VR user touching the button, it
    //activates the activatedUIButton method in the VRUIController class, sending
    //itself and enabling the behavior the button currently represents.

    //this class is intended to be created at runtime by VRUIController.


    //these public variables are set by the VRUIController class when created.
    public VRUIController reciever;
    public GameObject UICollider;

    public float triggerDistance = 0.005f; //this is the distance the other hand needs to be from the button to trigger it. (1.0f = 1 meter distance, which is why the number is so small)

    private bool isOn = false; //bool to know the state of the button.


    // Start is called before the first frame update
    void Start()
    {
        if (!DEBUG_COLORS)
        {
            this.GetComponent<Renderer>().material.color = Color.white;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //activates the button, and activates the behavior in VRUIController
    public void triggerOnState()
    {
        isOn = true;
        reciever.activatedUIButton(this.gameObject);
        if (DEBUG_COLORS)
        {
            this.GetComponent<Renderer>().material.color = new Color(0, 255, 0);
        }
    }

    //deactivates the button, allowing it to be activated again
    public void triggerOffState()
    {
        isOn = false;
        if (DEBUG_COLORS)
        {
            this.GetComponent<Renderer>().material.color = new Color(255, 0, 0);
        }
    }

    //when a trigger enters the collider, this function checks if it's the
    //trigger attatched to the hand, and activates the button if it is.
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == UICollider)
        {
            if (!isOn)
            {
                triggerOnState();
            }
        }
    }

    //deactivates the button when the UI trigger exits
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == UICollider)
        {
            if (isOn)
            {
                triggerOffState();
            }
        }
    }
}
