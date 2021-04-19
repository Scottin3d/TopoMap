using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class VRStartupController : MonoBehaviour
{

    //variable that tracks whether or not VR has been initialized on program startup. Use this to know if your code needs to act for VR or PC.
    public static bool isInVR = false;

    //reference to the VR Player Object in the scene. This will remain null if VR is not turned on. Once turned on, if VR is turned off, this Object will simply be deactivated.
    public static GameObject VRPlayerObject = null;

    //this is the GameObject representing the spawnpoint of the VR Player. It is used as a position and rotation to put the player in.
    public GameObject VRSpawnPoint = null;

    //this is the gameobject that represents whatever needs to be turned off for VR to function in the scene.
    public GameObject PlayerToTurnOff = null;


    // Start is called before the first frame update
    void Start()
    {
        if (UnityEngine.XR.XRSettings.isDeviceActive)
        {
            isInVR = true;
            StartVR();
        }
        Debug.Log("VR Status is: " + isInVR);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //this function will start the VR portion of the application, and is called if a VR device is detected.
    //Currently, it will load in the VR player into a specified location in the scene, and will deactivate the PlayerToTurnOff GameObject.
    private void StartVR()
    {

        PlayerToTurnOff.SetActive(false);

        VRPlayerObject = (GameObject) Instantiate(Resources.Load("VR/Player"));
        VRPlayerObject.transform.position = VRSpawnPoint.transform.position;
        VRPlayerObject.transform.rotation = VRSpawnPoint.transform.rotation;
    }

    //this function will enable or disable VR on the project, based on the provided bool
    public void enableVR(bool turnOn)
    {
        if (turnOn)
        {
            if (isInVR)
            {
                Debug.Log("Attempted VR turnon when already on.");
                return;
            }
            isInVR = true;
            if(VRPlayerObject == null)
            {
                StartVR();
            }
            else
            {
                PlayerToTurnOff.SetActive(false);
                VRPlayerObject.SetActive(true);
            }
            return;
        }
        else
        {
            if (!isInVR)
            {
                Debug.Log("Attempted VR shutoff when already off");
            }
            isInVR = false;
            
        }
    }

}
