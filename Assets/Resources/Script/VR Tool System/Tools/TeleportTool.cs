using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportTool : MonoBehaviour
{
    //the teleport tool will allow the player to teleport around the map as a secondary means of movement

    private static bool isActive;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void activate()
    {
        isActive = true;
    }
    public static void deactivate()
    {
        isActive = false;
    }

    //this function will teleport the player to the position they are pointing at, and is called on grip release in the VRTracedInput class
    public static void teleportPlayer(Vector3 position)
    {
        Debug.Log("in teleport player");
        if (!isActive)
        {
            return;
        }
        VRStartupController.VRPlayerObject.transform.position = position;
    }
}
