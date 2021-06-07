using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportTool : MonoBehaviour
{
    //the teleport tool will allow the player to teleport around the map as a secondary means of movement
    //this tool works byh recieving the position the player's right hand is pointing at, and when the
    //grip is released, and this tool is active, it sets the VR player's position to the position pointed to.

    private static bool isActive; //boolean tracking the active state of the class
    private static bool firstGrip = true;//boolean for teleportation logic

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //these functions activate and deactivate the class, which enables and disables the tool's functionality.
    //these functions also serve to reset the firstGrip boolean, which prevents unintentional teleportation.
    public static void activate()
    {
        isActive = true;
        firstGrip = true;
    }
    public static void deactivate()
    {
        isActive = false;
    }

    //this function will teleport the player to the position they are pointing at, and is called on grip release in the VRTracedInput class
    public static void teleportPlayer(Vector3 position)
    {
        //Debug.Log("in teleport player");
        if (!isActive)
        {
            return;
        }
        if (firstGrip)
        {
            //this checks for an initial grip release, which is guaranteed to happen when selecting a tool, and leads to
            //unintentional teleportation when selecting the teleport tool with a quick grip.
            firstGrip = false;
            return;
        }
        VRStartupController.VRPlayerObject.transform.position = position;
    }
}
