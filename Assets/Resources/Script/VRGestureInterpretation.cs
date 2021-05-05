using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRGestureInterpretation : MonoBehaviour
{
    private const int BACKLOG_FRAME_SIZE = 15; //the size of the backlogs in both number of frames tracked and number of positions in the array
    private const float DEGREES_TO_CHECK = 20f; //number of degrees for movement to be recognised as a gesture
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
        None,
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
        GetCurrentGesture();
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
        //Debug.Log("Pos: " + handToTrack.localPosition + "| Rot: " + handToTrack.localEulerAngles);
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

    //this function will give a best guess as to what the gesture the player has made when called.
    //this function works by returning whatever pattern is noticed "first", this is done by iterating
    //backwards through the backlog until one of the following criteria is met, whereby it makes it's
    //decision and returns a gesture.
    //
    //these are the conditions which will cause a gesture to be returned:
    //DEGREES_TO_CHECK degrees decrease in the X rotation: gesture.SwipeUp
    //DEGREES_TO_CHECK degrees increase in the X rotation: gesture.SwipeDown
    //DEGREES_TO_CHECK degrees decrease in the Y rotation: gesture.SwipeLeft
    //DEGREES_TO_CHECK degrees increase in the Y rotation: gesture.SwipeRight
    //
    //Note that this is decrease and increase in order from old to new, however, because the code
    //proper actually iterates from new to old (in order to get the most recent gesture), it will
    //actually be looking for the opposite of the above.
    //
    //if Debug mode is turned on, the squares will be highlighted based on the current gesture:
    //Swipe up = blue
    //Swipe down = orange
    //Swipe left = green
    //Swipe right = red
    //None = white
    //
    //also note that because of the way that rotational data works, this method must account for
    //rotations that may cause the data to loop around from 360 -> 0 or from 0 -> 360, it does
    //this by checking if the new data has a greater than 180 degree difference from the most
    //previous point, and will adjust accordingly.
    //what this means is that if by some dark magics someone rotates their wrist more than 180 degrees
    //in a 90th of a second (or more accurately, if there is significant frame loss), this system will
    //not be reliable, as it assumes the arm twisted in the other direction.
    public gesture GetCurrentGesture()
    {
        Vector3 low, high, left, right; //vectors which represent the furthest points the hand has traveled (rotational)
        float currentX = 0f;
        float currentY = 0f;
        if (currentPosition == 0)
        {
            low = high = left = right = rotationBacklog[BACKLOG_FRAME_SIZE - 1];
        }
        else 
        {
            low = high = left = right = rotationBacklog[currentPosition - 1];
        }

        for(int i = currentPosition - 1; i >= 0; i--) //first loop (before wrapping around array)
        {
            currentX = rotationBacklog[i].x;
            if(currentX > low.x + 180) //this means we've gone 0 -> 360
            {
                currentX = currentX - 360; //"wrap around" to get the rotation we want to see
            }
            else if(currentX < high.x - 180) //this means we've gone 360 -> 0
            {
                currentX = currentX + 360; //"wrap around" to get the rotation we want to see
            }

            if(currentX < low.x) //new low point
            {
                low.x = currentX;
            }
            else if(currentX > high.x) //new high point
            {
                high.x = currentX;
            }

            if(currentX > low.x + DEGREES_TO_CHECK) //swiped down
            {
                if (DEBUG_MODE)
                {
                    colorCubes(new Color(1f, 0.5f, 0f));
                }
                return gesture.SwipeDown;
            }
            else if (currentX < high.x - DEGREES_TO_CHECK) //swiped up
            {
                if (DEBUG_MODE)
                {
                    colorCubes(Color.blue);
                }
                return gesture.SwipeUp;
            }

            currentY = rotationBacklog[i].y;
            if(currentY > left.y + 180) //this means we've gone 0 -> 360
            {
                currentY = currentY - 180; //"wrap around" to get the rotation we want to see
            }
            else if (currentY < right.y - 180) //this means we've gone 360 -> 0
            {
                currentY = currentY + 180; //"wrap around" to get the rotation we want to see
            }

            if(currentY < left.y) //new left point
            {
                left.y = currentY;
            }
            else if(currentY > right.y) //new right point
            {
                right.y = currentY;
            }

            if(currentY > left.y + DEGREES_TO_CHECK) //swiped left
            {
                if (DEBUG_MODE)
                {
                    colorCubes(Color.green);
                }
                return gesture.SwipeLeft;
            }
            else if(currentY < right.y - DEGREES_TO_CHECK) //swiped right
            {
                if (DEBUG_MODE)
                {
                    colorCubes(Color.red);
                }
                return gesture.SwipeRight;
            }
        }

        //----------------------------------Between-Loops------------------------------------------

        for (int i = BACKLOG_FRAME_SIZE - 1; i >= currentPosition; i--) //second loop (end of array through current position)
        {
            currentX = rotationBacklog[i].x;
            if (currentX > low.x + 180) //this means we've gone 0 -> 360
            {
                currentX = currentX - 360; //"wrap around" to get the rotation we want to see
            }
            else if (currentX < high.x - 180) //this means we've gone 360 -> 0
            {
                currentX = currentX + 360; //"wrap around" to get the rotation we want to see
            }

            if (currentX < low.x) //new low point
            {
                low.x = currentX;
            }
            else if (currentX > high.x) //new high point
            {
                high.x = currentX;
            }

            if (currentX > low.x + 10f) //swiped down
            {
                if (DEBUG_MODE)
                {
                    colorCubes(new Color(1f, 0.5f, 0f));
                }
                return gesture.SwipeDown;
            }
            else if (currentX < high.x - 10f) //swiped up
            {
                if (DEBUG_MODE)
                {
                    colorCubes(Color.blue);
                }
                return gesture.SwipeUp;
            }

            currentY = rotationBacklog[i].y;
            if (currentY > left.y + 180) //this means we've gone 0 -> 360
            {
                currentY = currentY - 180; //"wrap around" to get the rotation we want to see
            }
            else if (currentY < right.y - 180) //this means we've gone 360 -> 0
            {
                currentY = currentY + 180; //"wrap around" to get the rotation we want to see
            }

            if (currentY < left.y) //new left point
            {
                left.y = currentY;
            }
            else if (currentY > right.y) //new right point
            {
                right.y = currentY;
            }

            if (currentY > left.y + 10f) //swiped left
            {
                if (DEBUG_MODE)
                {
                    colorCubes(Color.green);
                }
                return gesture.SwipeLeft;
            }
            else if (currentY < right.y - 10f) //swiped right
            {
                if (DEBUG_MODE)
                {
                    colorCubes(Color.red);
                }
                return gesture.SwipeRight;
            }
        }

        //if we found no gestures:
        if (DEBUG_MODE)
        {
            colorCubes(Color.white);
        }
        return gesture.None;
    }

    private void colorCubes(Color newColor)
    {
        for(int i = 0; i < BACKLOG_FRAME_SIZE; i++)
        {
            if (debugCubes[i] != null)
            {
                debugCubes[i].GetComponent<Renderer>().material.color = newColor;
            }
        }
    }
}
