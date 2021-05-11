using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    private GameObject rightHand = null;//right hand of the VR player

    private bool isActive = false; //whether or not the menu should be active (left hand is palm-up)

    //Hotspots (where the player can grab the important tools quickly)
    private GameObject HotSpot_1 = null;

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

    //this function takes the given position, rotation, scale, and text, and creates a new hotspot, replacing the spot given
    private void CreateHotSpot(Vector3 position, Vector3 rotation, Vector3 scale, string spotText, GameObject spot)
    {
        if (spot != null)
        {
            Destroy(spot);
            spot = null;
        }

        spot = (GameObject)Instantiate(Resources.Load("VR/VRHotSpot"));
        spot.transform.SetParent(leftHand.transform);
        spot.transform.localPosition = position;
        if (rotation != Vector3.zero)
        {
            spot.transform.localRotation = Quaternion.Euler(rotation);
        }
        else
        {
            spot.transform.localRotation = Quaternion.Euler(new Vector3(17.044f, 9.419f, 69.47f));
        }
        if (scale != Vector3.zero)
        {
            spot.transform.localScale = scale;
        }
        spot.GetComponentInChildren<Text>().text = spotText;
        spot.GetComponent<VRToolHotSpot>().UICollider = rightHand.transform.Find("UIInteractSphere").gameObject;
        spot.GetComponent<VRToolHotSpot>().reciever = this;
    }

    private void StartUp()
    {
        leftHand = VRStartupController.VRPlayerObject.transform.Find("SteamVRObjects/LeftHand").gameObject;
        rightHand = VRStartupController.VRPlayerObject.transform.Find("SteamVRObjects/RightHand").gameObject;
        CreateHotSpot(new Vector3(0.0506f, 0.0598f, -0.1993f), new Vector3(33.841f, 0.014f, -0.018f), Vector3.zero, "Marker Tool", HotSpot_1);
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

    public void recieveHotSpotInput(GameObject theSpot)
    {
        if(theSpot == HotSpot_1)
        {
            //activate marker tool
            Debug.Log("HotSpot1 active");
        }
        else
        {
            Debug.LogWarning("hotspot input called without hotspot!");
        }
    }
}
