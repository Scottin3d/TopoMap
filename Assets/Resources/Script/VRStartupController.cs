using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

public class VRStartupController : MonoBehaviour
{
    //static reference to the one instance of VRStartupController
    public static VRStartupController staticReference;

    //variable that tracks whether or not VR has been initialized on program startup. Use this to know if your code needs to act for VR or PC.
    public static bool isInVR = false;

    //reference to the VR Player Object in the scene. This will remain null if VR is not turned on. Once turned on, if VR is turned off, this Object will simply be deactivated.
    public static GameObject VRPlayerObject = null;

    //reference to the PC canvas that needs to only be present when VR is active
    public Canvas VRPlayerCanvas = null;

    //this is the GameObject representing the spawnpoint of the VR Player. It is used as a position and rotation to put the player in.
    public GameObject VRSpawnPoint = null;

    //this is the gameobject that represents whatever needs to be turned off for VR to function in the scene.
    public GameObject PlayerToTurnOff = null;

    //this is the button that will switch between VR and non-VR modes
    public Button VRToggle = null;

    //boolean to store whether or not this class needs to restore a locked cursor when switching back to PC
    private bool reLockCursor = false;

    //this is the UI controller which needs the instantiated VR Player to start up
    //note: currently the VRUIController checks for the player object
    //public VRUIController VRUIControl = null;
    


    // Start is called before the first frame update
    void Start()
    {
        staticReference = this;
        VRToggle.onClick.AddListener(onVRToggleButtonPressed);
        if (UnityEngine.XR.XRSettings.isDeviceActive)
        {
            isInVR = true;
            if (PlayerToTurnOff != null)
            {
                StartVR();
            }
            else
            {
                isInVR = false;
                Debug.Log("VR Detected but no 2D Player Object given, waiting for runtime player");
            }
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
        if(Cursor.lockState == CursorLockMode.Locked)
        {
            reLockCursor = true;
            Cursor.lockState = CursorLockMode.None;
        }
        VRPlayerObject = (GameObject) Instantiate(Resources.Load("VR/PlayerVR"));
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
                if (Cursor.lockState == CursorLockMode.Locked)
                {
                    reLockCursor = true;
                    Cursor.lockState = CursorLockMode.None;
                }
                PlayerToTurnOff.SetActive(false);
                VRPlayerObject.SetActive(true);
            }
            return;
        }
        else
        {
            if (!isInVR)
            {
                Debug.Log("Attempted VR shutoff when already off.");
            }
            isInVR = false;
            if (reLockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                reLockCursor = false;
            }
            PlayerToTurnOff.SetActive(true);
            VRPlayerObject.SetActive(false);
            VRPlayerCanvas.enabled = false; //disables the double-canvas that was occurring when switching between VR and PC
        }
    }

    //this function is a listener to when the VR Toggle Button is pressed. it will switch between VR and Non-VR by caling enableVR()
    public void onVRToggleButtonPressed()
    {
        enableVR(!isInVR);
    }

    //this is a function intended to be called near the start of runtime to provide the 2D player object at runtime. It will essentially run start's logic for starting up VR once the player is created,
    //which may cause errors if called at another time
    public void setPlayer2D(GameObject player)
    {
        PlayerToTurnOff = player;
        if (UnityEngine.XR.XRSettings.isDeviceActive)
        {
            isInVR = true;
            StartVR();
        }
    }

    //this function will return whether or not VR has been detected on the PC (essentilly XR isDeviceActive)
    public static bool isVRDetected()
    {
        return UnityEngine.XR.XRSettings.isDeviceActive;
    }

    //this is a method to call when enabling VR from outside this class to re-enable the VR systems that need to be enabled
    public static void enable()
    {
        staticReference.VRPlayerCanvas.enabled = true;
        staticReference.enableVR(true);
    }
}
