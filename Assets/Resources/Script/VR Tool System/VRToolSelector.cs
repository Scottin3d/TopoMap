using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRToolSelector : MonoBehaviour
{
    //this is the VR Tool Selector class, it is intended to handle the VR Player choosing from the types of handheld
    //tools they may need to use in or on the map. Currently this includes:
    //the road system
    //the marker system
    //
    //selection is done by facing the left hand plam-up, where a selection menu will display, the player will then
    //swipe left or right to their desired tool, up and down for the variations of the tool, and grab to select the tool.
    //
    //here is a horizontal and vertical layout of the menu so far:
    //
    //
    //
    //
    //
    //
    // None <-> Road <-> Marker
    //
    //info for the swipe hitboxes:
    //tool:
    //   T: 0.176f, -0.027f, -0.102f
    //   R: 0f, 0f, 0f
    //   S: 0.1f, 0.1f, 0.1f
    //left:
    //   T: 0.176f, 0.0357f, -0.102f
    //   R: 0f, 0f, 0f
    //   S: 0.1f, 0.025f, 0.1f
    //right:
    //    T: 0.176f, -0.0885f, -0.1021f
    //    R: 0f, 0f, 0f
    //    S: 0.1f, 0.1f, 0.1f
    //
    //
    //HotSpot positions:
    //1:
    //T: 0.0506f, 0.0598f, -0.1993f
    //R: 33.841f, 0.014f, -0.018f
    //S: 0.05f, 0.05f, 0.05f

    private GameObject leftHand = null; //left hand of the VR player

    private bool isActive = false; //whether or not the menu should be active (left hand is palm-up)

    public enum toolSelectionState
    {
        None,
        Road,
        Marker,
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("delayInitialization");
    }

    private void StartUp()
    {
        leftHand = VRStartupController.VRPlayerObject.transform.Find("SteamVRObjects/LeftHand").gameObject;
    }

    IEnumerator delayInitialization()
    {
        while(VRStartupController.VRPlayerObject == null)
        {
            yield return new WaitForSeconds(0.1f);
        }
        StartUp();
    }

    // Update is called once per frame
    void Update()
    {
        checkActive(); //see if we need to activate/deactivate
    }

    private void checkActive()
    {

    }
}
