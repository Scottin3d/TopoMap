using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

public class ToolBox : MonoBehaviour
{
    protected bool isActive;
    //toolbox is the controlling script for the toolbox the player pulls their tools out of.

    public GameObject UICollider;
    public VRToolSelector reciever;

    private bool inAnimation = false;
    private const float timeToAnimate = 1f; //how long in seconds the swipe animation should take

    private GameObject primaryText;
    private GameObject transitionText;

    //start sets the child gameobjects, and assumes that the canvas is the first child under the toolbox,
    //and assumes the primary text and transition text are the first and second children of the canvas
    //object, respectively
    private void Start()
    {
        primaryText = transform.GetChild(0).GetChild(0).gameObject;
        transitionText = transform.GetChild(0).GetChild(1).gameObject;
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

    //this function will transition the text leftwards in a small animation.
    //the bool returns true if the toolbox is not already in an animation and begins.
    //otherwise it returns false
    //
    //positional text information:
    //
    //animate moving in the negative x direction locally
    //
    //final position:
    //center pivot
    //T: 0f,0f,0f
    //R: 0f,0f,0f
    //S: 2f,1f,1f
    //width: 1000f
    //height: 100f
    //anchors and pivot min/max all at 0.5f
    //
    public bool transitionLeft(string newText)
    {
        if (inAnimation)
        {
            return false;
        }

        inAnimation = true;
        transitionText.GetComponent<Text>().text = newText;

        StartCoroutine("incrementLeftChange");
        return true;
    }

    //this coroutine animates the object, currently fading out old text and fading in new text
    IEnumerator incrementLeftChange()
    {
        float currentTime = 0f;
        Color colorToSet = Color.black;
        while (true)
        {
            if(currentTime >= timeToAnimate)
            {
                onAnimationEnd();
                break;
            }

            colorToSet.a = (timeToAnimate - currentTime) / timeToAnimate;
            primaryText.GetComponent<Text>().color = colorToSet;
            colorToSet.a = currentTime / timeToAnimate;
            transitionText.GetComponent<Text>().color = colorToSet;

            currentTime += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
        
    }

    //function that resets objects at the end of the transitional animation
    private void onAnimationEnd()
    {
        primaryText.GetComponent<Text>().text = transitionText.GetComponent<Text>().text;

        Color colorToSet = Color.black;
        primaryText.GetComponent<Text>().color = colorToSet;
        colorToSet.a = 0f;
        transitionText.GetComponent<Text>().color = colorToSet;

        primaryText.transform.localPosition = Vector3.zero;
        transitionText.transform.localPosition = Vector3.zero;

        inAnimation = false;
    }
}
