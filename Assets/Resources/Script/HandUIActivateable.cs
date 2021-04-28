using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandUIActivateable : MonoBehaviour
{
    private const bool DEBUG_COLORS = true;
    //this is a component for the UI Buttons that the VR player uses. when triggered it sends itself to the VRUIController in order to activate its action

    public Transform otherHand; //this is the hand that the button should interact with, all this transform needs to represent is the position of the hand, and does not need to be attatched to SteamVR's hand necessarily.

    public VRUIController reciever;

    public GameObject UICollider;

    public float triggerDistance = 0.005f; //this is the distance the other hand needs to be from the button to trigger it. (1.0f = 1 meter distance, which is why the number is so small)

    private bool isOn = false; //bool to know the state of the button.


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //checkHandDistance();
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
        reciever.activatedUIButton(this.gameObject);
        if (DEBUG_COLORS)
        {
            this.GetComponent<Renderer>().material.color = new Color(0, 255, 0);
        }
    }

    public void triggerOffState()
    {
        isOn = false;
        if (DEBUG_COLORS)
        {
            this.GetComponent<Renderer>().material.color = new Color(255, 0, 0);
        }
    }

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
