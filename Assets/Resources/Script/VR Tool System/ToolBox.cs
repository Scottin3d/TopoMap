using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

public class ToolBox : MonoBehaviour
{

    //ToolBox is the script responsible for the toolbox which appears when facing the left plam upwards.
    //it keeps track of the active status of the object, tells VRToolSelector when activated, and handles
    //the transitional animations when switching between tools in the box.

    protected bool isActive;
    //toolbox is the controlling script for the toolbox the player pulls their tools out of.

    public GameObject UICollider;
    public VRToolSelector reciever;

    private bool inAnimation = false;
    private const float timeToAnimate = 1f; //how long in seconds the swipe animation should take

    private GameObject primaryText;
    private GameObject leftTransitionText;
    private GameObject rightTransitionText;

    //start sets the child gameobjects, and assumes that the canvas is the first child under the toolbox,
    //and assumes the primary text and transition text are the first and second children of the canvas
    //object, respectively
    private void Start()
    {
        primaryText = transform.GetChild(0).GetChild(0).gameObject;
        leftTransitionText = transform.GetChild(1).GetChild(0).gameObject;
        rightTransitionText = transform.GetChild(2).GetChild(0).gameObject;
    }

    //activates the renderers for this object and allows interaction
    public void activate()
    {
        gameObject.GetComponent<Renderer>().enabled = true;
        gameObject.GetComponentInChildren<Canvas>().enabled = true;
        isActive = true;
    }

    //deactivates the renderers for this object and disallows interaction
    public void deactivate()
    {
        gameObject.GetComponent<Renderer>().enabled = false;
        gameObject.GetComponentInChildren<Canvas>().enabled = false;
        isActive = false;
    }

    //returns active status of the toolbox
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

    //this function will transition the text in a small animation of the box turning to the left.
    //the bool returns true if the toolbox is not already in an animation and begins.
    //otherwise it returns false
    public bool transitionLeft(string newText)
    {
        if (inAnimation)
        {
            return false;
        }

        inAnimation = true;
        leftTransitionText.GetComponent<Text>().text = newText;
        leftTransitionText.GetComponent<Text>().color = Color.black;

        StartCoroutine("incrementLeftChange");
        return true;
    }

    //this coroutine animates the object, 
    //now with rotation, spins the toolbox in the positive Z direction
    IEnumerator incrementLeftChange()
    {
        float currentTime = 0f;
        while (true)
        {
            if(currentTime >= timeToAnimate)
            {
                onAnimationEnd(true);
                break;
            }

            Quaternion newRot = transform.localRotation;
            newRot *= Quaternion.AngleAxis((Time.deltaTime * 90f), Vector3.forward);
            transform.localRotation = newRot;

            currentTime += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
        
    }

    //this function will transition the text in a small animation of the box turning to the right.
    //the bool returns true if the toolbox is not already in an animation and begins.
    //otherwise it returns false
    public bool transitionRight(string newText)
    {
        if (inAnimation)
        {
            return false;
        }

        inAnimation = true;
        rightTransitionText.GetComponent<Text>().text = newText;
        rightTransitionText.GetComponent<Text>().color = Color.black;

        StartCoroutine("incrementRightChange");
        return true;
    }

    //this coroutine animates the object, 
    //now with rotation, spins the toolbox in the negative Z direction
    IEnumerator incrementRightChange()
    {
        float currentTime = 0f;
        while (true)
        {
            if (currentTime >= timeToAnimate)
            {
                onAnimationEnd(false);
                break;
            }

            Quaternion newRot = transform.localRotation;
            newRot *= Quaternion.AngleAxis(-(Time.deltaTime * 90f), Vector3.forward);
            transform.localRotation = newRot;

            currentTime += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }

    }

    //function that resets objects at the end of the transitional animation.
    //what resetting means in this context is placing the box back in it's original rotation, and replacing
    //the main text with the new text.
    //
    //isLeft boolean indicates which function is calling the animation end in order to access the correct text.
    private void onAnimationEnd(bool isLeft)
    {
        if (isLeft)
        {
            primaryText.GetComponent<Text>().text = leftTransitionText.GetComponent<Text>().text;
        }
        else
        {
            primaryText.GetComponent<Text>().text = rightTransitionText.GetComponent<Text>().text;
        }

        Color colorToSet = Color.black;
        primaryText.GetComponent<Text>().color = colorToSet;
        colorToSet.a = 0f;
        leftTransitionText.GetComponent<Text>().color = colorToSet;
        rightTransitionText.GetComponent<Text>().color = colorToSet;

        primaryText.transform.localPosition = Vector3.zero;
        leftTransitionText.transform.localPosition = Vector3.zero;

        //reset rotation
        transform.localRotation = Quaternion.Euler(new Vector3(4.244f, 104.451f, 26.596f));

        inAnimation = false;
    }
}
