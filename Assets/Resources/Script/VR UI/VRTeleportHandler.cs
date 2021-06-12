using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRTeleportHandler : MonoBehaviour
{
    //this is a small class meant to handle teleporting the VR player between the tabletop map and the ground map
    //this script should be placed on a game object with empty children named in the start function
    //("Table Teleport position" and "Ground Teleport position")

    private static Transform tableTeleportPosition;
    private static Transform groundTeleportPosition;

    // Start is called before the first frame update
    //initializes variable references this class needs.
    void Start()
    {
        tableTeleportPosition = this.transform.Find("Table Teleport position");
        groundTeleportPosition = this.transform.Find("Ground Teleport position");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //method for sending the VR player to the set table position
    public static void TeleportVRPlayerToTable()
    {
        VRStartupController.VRPlayerObject.transform.position = tableTeleportPosition.position;
        VRStartupController.VRPlayerObject.transform.rotation = tableTeleportPosition.rotation;
    }

    //method for sending the VR player to the set large map position
    public static void TeleportVRPlayerToGround()
    {
        VRStartupController.VRPlayerObject.transform.position = groundTeleportPosition.position;
        VRStartupController.VRPlayerObject.transform.rotation = groundTeleportPosition.rotation;
    }
}
