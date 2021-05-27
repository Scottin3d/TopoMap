using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VRToolSelector : MonoBehaviour
{
    //turning this on will cause the swipeboxes to appear based on the color swipe they represent
    private const bool DEBUG_SHOW_SWIPEBOXES = false;

    //degrees of error for which to calculate the VR player's palm facing upwards
    private const float DEGREES_ERROR = 20f;

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
    // None <-> Road <-> Marker <-> Ruler
    //
    //
    //info for the swipe hitboxes:
    //tool:
    //   T: 0.179f, -0.0233f, -0.07689f
    //   R: 4.244f, 104.451f, 26.596f
    //   S: 0.1f, 0.1f, 0.1f
    //left:
    //   T: 0.1866f, 0.0098f, -0.0573f
    //   R: 4.244f, 104.451f, 26.596f
    //   S: 0.1f, 0.025f, 0.1f
    //right:
    //    T: 0.1646f, -0.1018f, -0.1095f
    //    R: 4.244f, 104.451f, 26.596f
    //    S: 0.1f, 0.025f, 0.1f
    //
    //
    //HotSpot positions:
    //0:
    //T: 0.0506f, 0.0886f, -0.2423f
    //R: 33.841f, 0.014f, -45f
    //S: 0.05f, 0.05f, 0.05f
    //1:
    //T: 0.0506f, 0.0598f, -0.1993f
    //R: 33.841f, 0.014f, -45f
    //S: 0.05f, 0.05f, 0.05f
    //2:
    //T: 0.0506f, 0.0308f, -0.1562f
    //R: 33.841f, 0.014f, -45f
    //S: 0.05f, 0.05f, 0.05f
    //3:
    //T: 0.0506f, 0.0019f, -0.1129f
    //R: 33.841f, 0.014f, -45f
    //S: 0.05f, 0.05f, 0.05f

    private toolSelectionState currentState = toolSelectionState.None;

    private GameObject leftHand = null; //left hand of the VR player
    private GameObject rightHand = null;//right hand of the VR player

    private bool isActive = false; //whether or not the menu should be active (left hand is palm-up)

    //Hotspots (where the player can grab the important tools quickly)
    private GameObject HotSpot_0 = null;
    private GameObject HotSpot_1 = null;
    private GameObject HotSpot_2 = null;
    private GameObject HotSpot_3 = null;

    //toolBox objects
    private GameObject ToolBox = null; //toolbox the player selects their tool from
    private GameObject LeftPad = null; //object that will detect the player swiping left
    private GameObject RightPad = null;//object that will detect the player swiping right

    public enum toolSelectionState
    {
        None,
        Road,
        Marker,
        Ruler
    }

    private const string NoneText = "None";
    private const string RoadText = "Road";
    private const string MarkerText = "Marker";
    private const string RulerText = "Ruler";

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("delayInitialization");
    }

    //this function takes the given position, rotation, scale, and text, and creates a new hotspot, replacing the spot given
    //
    //Current HotSpot positions:
    //0:
    //T: 0.0506f, 0.0886f, -0.2423f
    //R: 33.841f, 0.014f, -45f
    //S: 0.05f, 0.05f, 0.05f
    //1:
    //T: 0.0506f, 0.0598f, -0.1993f
    //R: 33.841f, 0.014f, -45f
    //S: 0.05f, 0.05f, 0.05f
    //2:
    //T: 0.0506f, 0.0308f, -0.1562f
    //R: 33.841f, 0.014f, -45f
    //S: 0.05f, 0.05f, 0.05f
    //3:
    //T: 0.0506f, 0.0019f, -0.1129f
    //R: 33.841f, 0.014f, -45f
    //S: 0.05f, 0.05f, 0.05f
    private void CreateHotSpot(Vector3 position, Vector3 rotation, Vector3 scale, string spotText, GameObject spot, int spotNum)
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
        switch (spotNum)
        {
            case 0:
                HotSpot_0 = spot;
                break;
            case 1:
                HotSpot_1 = spot;
                break;
            case 2:
                HotSpot_2 = spot;
                break;
            case 3:
                HotSpot_3 = spot;
                break;
            default:
                Debug.LogWarning("unknown hotspot number passed to create hotspot");
                break;
        }
    }

    //this function will create the toolbox that the player uses according to these specifications:
    //tool:
    //   T: 0.179f, -0.0233f, -0.07689f
    //   R: 4.244f, 104.451f, 26.596f
    //   S: 0.1f, 0.1f, 0.1f
    //left:
    //   T: 0.1866f, 0.0098f, -0.0573f
    //   R: 4.244f, 104.451f, 26.596f
    //   S: 0.1f, 0.025f, 0.1f
    //right:
    //    T: 0.1646f, -0.1018f, -0.1095f
    //    R: 4.244f, 104.451f, 26.596f
    //    S: 0.1f, 0.025f, 0.1f
    private void createToolBox()
    {
        ToolBox = (GameObject)Instantiate(Resources.Load("VR/VRToolbox"));
        LeftPad = (GameObject)Instantiate(Resources.Load("VR/VRSwipeBox"));
        RightPad = (GameObject)Instantiate(Resources.Load("VR/VRSwipeBox"));

        ToolBox.transform.SetParent(leftHand.transform);
        LeftPad.transform.SetParent(leftHand.transform);
        RightPad.transform.SetParent(leftHand.transform);

        ToolBox.transform.localPosition = new Vector3(0.179f, -0.0233f, -0.07689f);
        LeftPad.transform.localPosition = new Vector3(0.1866f, 0.0098f, -0.0573f);
        RightPad.transform.localPosition = new Vector3(0.1646f, -0.1018f, -0.1095f);

        ToolBox.transform.localRotation = Quaternion.Euler(new Vector3(4.244f, 104.451f, 26.596f));
        LeftPad.transform.localRotation = Quaternion.Euler(new Vector3(4.244f, 104.451f, 26.596f));
        RightPad.transform.localRotation = Quaternion.Euler(new Vector3(4.244f, 104.451f, 26.596f));

        //toolbox public fields
        ToolBox.GetComponent<ToolBox>().UICollider = rightHand.transform.Find("UIInteractSphere").gameObject;
        ToolBox.GetComponent<ToolBox>().reciever = this;
        changeToolBoxText(NoneText);
        ToolBox.GetComponent<Renderer>().enabled = false;
        ToolBox.GetComponentInChildren<Canvas>().enabled = false;

        //leftpad public fields
        LeftPad.GetComponent<VRToolSwipe>().UICollider = rightHand.transform.Find("UIInteractSphere").gameObject;
        LeftPad.GetComponent<VRToolSwipe>().reciever = this;
        LeftPad.GetComponent<VRToolSwipe>().triggerGesture = VRGestureInterpretation.gesture.SwipeLeft;

        //rightpad public fields
        RightPad.GetComponent<VRToolSwipe>().UICollider = rightHand.transform.Find("UIInteractSphere").gameObject;
        RightPad.GetComponent<VRToolSwipe>().reciever = this;
        RightPad.GetComponent<VRToolSwipe>().triggerGesture = VRGestureInterpretation.gesture.SwipeRight;

        if (DEBUG_SHOW_SWIPEBOXES)
        {
            LeftPad.GetComponent<Renderer>().enabled = true;
            RightPad.GetComponent<Renderer>().enabled = true;
            LeftPad.GetComponent<Renderer>().material.color = Color.green;
            RightPad.GetComponent<Renderer>().material.color = Color.red;
        }
        else
        {
            LeftPad.GetComponent<Renderer>().enabled = false;
            RightPad.GetComponent<Renderer>().enabled = false;
        }
    }

    private void StartUp()
    {
        leftHand = VRStartupController.VRPlayerObject.transform.Find("SteamVRObjects/LeftHand").gameObject;
        rightHand = VRStartupController.VRPlayerObject.transform.Find("SteamVRObjects/RightHand").gameObject;

        CreateHotSpot(new Vector3(0.0506f, 0.0886f, -0.2423f), new Vector3(33.841f, 0.014f, -45f), Vector3.zero, NoneText, HotSpot_0, 0);
        CreateHotSpot(new Vector3(0.0506f, 0.0598f, -0.1993f), new Vector3(33.841f, 0.014f, -45f), Vector3.zero, RoadText, HotSpot_1, 1);
        CreateHotSpot(new Vector3(0.0506f, 0.0308f, -0.1562f), new Vector3(33.841f, 0.014f, -45f), Vector3.zero, MarkerText, HotSpot_2, 2);
        CreateHotSpot(new Vector3(0.0506f, 0.0019f, -0.1129f), new Vector3(33.841f, 0.014f, -45f), Vector3.zero, RulerText, HotSpot_3, 3);

        createToolBox();
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

    //checks for the left palm facing upwards (z is around 90 degrees), and activates toolbox if so.
    private void checkActive()
    {
        if(leftHand == null)
        {
            return;
        }
        Vector3 currentRot = leftHand.transform.rotation.eulerAngles;
        //Debug.Log("Rotation is:" + currentRot);
        if (currentRot.z > (90f - DEGREES_ERROR) && currentRot.z < (90f + DEGREES_ERROR))
        {
            if (!isActive)
            {
                activateBox();
                isActive = true;
            }
        }
        else
        {
            if (isActive)
            {
                deactivateBox();
                isActive = false;
            }
        }
    }
    
    // activates the toolbox for the VR player to use
    private void activateBox()
    {
        ToolBox.GetComponent<ToolBox>().activate();
    }

    //deactivates the toolbox when it is no longer needed
    private void deactivateBox()
    {
        ToolBox.GetComponent<ToolBox>().deactivate();
    }

    //this is a public function to be called by the hotspots in order to activate a tool
    public void recieveHotSpotInput(GameObject theSpot)
    {
        if(theSpot == HotSpot_0)
        {
            //activate tool
            Debug.Log("HotSpot0 active");
            MarkerTool.deactivate();
            RoadTool.deactivate();
            RulerTool.deactivate();
        }
        else if(theSpot == HotSpot_1)
        {
            //activate tool
            Debug.Log("HotSpot1 active");
            RoadTool.activate();
            MarkerTool.deactivate();
            RulerTool.deactivate();
        }
        else if (theSpot == HotSpot_2)
        {
            //activate tool
            Debug.Log("HotSpot2 active");
            MarkerTool.activate();
            RoadTool.deactivate();
            RulerTool.deactivate();
        }
        else if (theSpot == HotSpot_3)
        {
            //activate tool
            Debug.Log("HotSpot3 active");
            RulerTool.activate();
            MarkerTool.deactivate();
            RoadTool.deactivate();
        }
        else
        {
            Debug.LogWarning("hotspot input called without hotspot!");
        }
    }

    //this function indicates that the toolbox has been grabbed, and will activate the respective tool.
    public void BoxActivated()
    {
        if (!isActive)
        {
            return;
        }
        switch (currentState)
        {
            case toolSelectionState.None:
                //remove current tool
                MarkerTool.deactivate();
                RoadTool.deactivate();
                RulerTool.deactivate();
                break;
            case toolSelectionState.Road:
                //activate road tool
                RoadTool.activate();
                MarkerTool.deactivate();
                RulerTool.deactivate();
                break;
            case toolSelectionState.Marker:
                //activate marker tool
                MarkerTool.activate();
                RoadTool.deactivate();
                RulerTool.deactivate();
                break;
            case toolSelectionState.Ruler:
                //activate Ruler
                RulerTool.activate();
                MarkerTool.deactivate();
                RoadTool.deactivate();
                break;
            default:
                break;
        }
    }

    public void recieveSwipeInput(GameObject swipeObject)
    {
        if (!isActive)
        {
            return;
        }

        if(swipeObject == LeftPad) //swipe left
        {
            switch (currentState)
            {
                case toolSelectionState.None:
                    break;
                case toolSelectionState.Road:
                    currentState = toolSelectionState.None;
                    changeToolBoxText(NoneText);
                    break;
                case toolSelectionState.Marker:
                    currentState = toolSelectionState.Road;
                    changeToolBoxText(RoadText);
                    break;
                case toolSelectionState.Ruler:
                    currentState = toolSelectionState.Marker;
                    changeToolBoxText(MarkerText);
                    break;
                default:
                    break;
            }
        }
        else if(swipeObject == RightPad) //swipe right
        {
            switch (currentState)
            {
                case toolSelectionState.None:
                    currentState = toolSelectionState.Road;
                    changeToolBoxText(RoadText);
                    break;
                case toolSelectionState.Road:
                    currentState = toolSelectionState.Marker;
                    changeToolBoxText(MarkerText);
                    break;
                case toolSelectionState.Marker:
                    currentState = toolSelectionState.Ruler;
                    changeToolBoxText(RulerText);
                    break;
                case toolSelectionState.Ruler:
                    break;
                default:
                    break;
            }
        }
        else
        {
            Debug.LogWarning("Swipe input recieved with unknown swipe object!");
        }
    }

    private void changeToolBoxText(string text)
    {
        ToolBox.GetComponentInChildren<Text>().text = text;
    }
}
