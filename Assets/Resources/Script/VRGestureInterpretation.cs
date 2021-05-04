using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRGestureInterpretation : MonoBehaviour
{
    private const int BACKLOG_FRAME_SIZE = 30; //the size of the backlogs in both number of frames tracked and number of positions in the array
    private const bool DEBUG_MODE = true;

    //this is the VR Gesture Interpretation system, it's meant to interperet physical gestures (basically a fancy way to say moving your hands in 3D space),
    //and allow other classes or systems to know to react to a gesture when it occurrs.
    //the question is how to do this in an efficient but open way. I think the best way to go about doing this is to sample the positional and rotational data of the hand(s), and then
    //construct a most likely gesture from this data when another class calls for a gesture.

    //from a data structure standpoint the backlogs will essentially be queues as the array will have to constantly be filled with the most recent data, and this is (to my knowledge) the most
    //efficient way to have the last 30 data points always available.

    private Vector3[] positionBacklog; //positions of the last BACKLOG_FRAME_SIZE positions the hand has been in
    private Vector3[] rotationBacklog; //rotations (euler angles) of the last BACKLOG_FRAME_SIZE rotations the hand has been in
    private int currentPosition = 0; //current position in the array to fill

    private GameObject[] debugCubes; //list of gameobjects that represent the backlog positions when in debug mode. this is for figuring out how the script needs to look at the data and interpret it

    private Transform handToTrack; //hand that is being tracked for gestures, currently the right hand

    private bool active; //boolean for when the handToTrack has been instantiated and can have data pulled from

    //NOTE: there should currently only ever be ONE VRGestureInterpretation script in use in the scene at a time.
    public static VRGestureInterpretation reference; //reference to the current gesture interpreter

    //enumerator to represent the identified gesture
    public enum gesture
    {
        SwipeUp,
        SwipeDown,
        SwipeLeft,
        SwipeRight,
        SpinClockwise,
        SpinCounterClockwise
    }

    // Start is called before the first frame update
    void Start()
    {
        positionBacklog = new Vector3[BACKLOG_FRAME_SIZE];
        rotationBacklog = new Vector3[BACKLOG_FRAME_SIZE];
        if (DEBUG_MODE)
        {
            debugCubes = new GameObject[BACKLOG_FRAME_SIZE];
        }
        reference = this;
        StartCoroutine("DelayStartup");
    }

    IEnumerator DelayStartup()
    {
        while (VRStartupController.VRPlayerObject == null)
        {
            yield return new WaitForSeconds(0.1f);
        }
        handToTrack = VRStartupController.VRPlayerObject.transform.Find("SteamVRObjects/RightHand");
        active = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (VRStartupController.isInVR && active)
        {
            gatherData();
        }
    }

    //this function will take the current position and rotation of the hand and place it into the backlogs
    private void gatherData()
    {
        if(currentPosition == BACKLOG_FRAME_SIZE)
        {
            currentPosition = 0;
        }
        positionBacklog[currentPosition] = handToTrack.localPosition; //local position because we are interested in the hand as an isolated system, rather than it's position in the world
        rotationBacklog[currentPosition] = handToTrack.localEulerAngles; //local rotation for the same reason
        if (DEBUG_MODE)
        {
            Destroy(debugCubes[currentPosition]);
            debugCubes[currentPosition] = GameObject.CreatePrimitive(PrimitiveType.Cube);
            debugCubes[currentPosition].transform.position = handToTrack.position; //not the local position (I want to see it follow my hand)
            debugCubes[currentPosition].transform.rotation = handToTrack.rotation; //need proper rotation for nicely aligned cubes
            debugCubes[currentPosition].transform.localScale = new Vector3(0.01f, 0.01f, 0.01f); //1cm cube
        }
        currentPosition++;
    }

    //loop that goes through the array to calculate the gesture:
    //start at pos - 1, then go through until the position is <0
    //then go to 29, and go back through until pos (as it will be the oldest datapoint)
}
