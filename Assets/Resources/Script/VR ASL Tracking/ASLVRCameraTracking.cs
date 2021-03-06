using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class ASLVRCameraTracking : MonoBehaviour
{
    //ASLVRCameraTracking is a script meant to track the VR Player's head position and relay that information over ASL
    //in order to create a representation of the VR player on all connected clients.

    //public VRStartupController VRController = null; //VR Controller to know the state of VR
    public static GameObject VRCameraToTrack = null; //ASL Synced object representing the VR camera
    public static GameObject LocalVRCamera = null; //local object representing the VR camera

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("TrackOverASL");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //this coroutine handles the behavior of both waiting to initialize behavior and variables reliant on the VR Player,
    //and updating the position of the VR player's representation over ASL.
    IEnumerator TrackOverASL()
    {
        Debug.Log(VRStartupController.isVRDetected());
        if (VRStartupController.isVRDetected()) //only checks for starting up the VR object at the start because if we are not in VR at this point, then the user does not have VR installed
        {
            while (VRStartupController.VRPlayerObject == null)
            {
                yield return new WaitForSeconds(0.1f);
            }
            LocalVRCamera = VRStartupController.VRPlayerObject.GetComponentInChildren<Camera>().gameObject; //should find the main camera for the VR player
            ASL.ASLHelper.InstantiateASLObject("ASLVRHead", new Vector3(0, 0, 0), Quaternion.identity, "", "", SetTrackedHead);
        }
        while (VRCameraToTrack == null)
        {
            yield return new WaitForSeconds(0.1f);
        }

        ASLObjectTrackingSystem.AddPlayerToTrack(VRCameraToTrack.GetComponent<ASLObject>());

        VRCameraToTrack.GetComponent<Renderer>().enabled = false;
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            if (VRStartupController.isInVR) //checks the VR state here rather than detected to know if the VR stuff needs to be tracked at this time or not
            {
                VRCameraToTrack.GetComponent<ASLObject>().SendAndSetClaim(() => { 
                    VRCameraToTrack.GetComponent<ASLObject>().SendAndSetLocalPosition(LocalVRCamera.transform.position); 
                    VRCameraToTrack.GetComponent<ASLObject>().SendAndSetLocalRotation(LocalVRCamera.transform.rotation);
                });
            }
            else
            {
                //currently if VR is off, sends the ASLObject to 000 with Quaternion identity as rotation, this may need to be changed in the future to a "safe spot" where the player cannot see
                VRCameraToTrack.GetComponent<ASLObject>().SendAndSetClaim(() => { VRCameraToTrack.GetComponent<ASLObject>().SendAndSetLocalPosition(new Vector3(0, 0, 0)); VRCameraToTrack.GetComponent<ASLObject>().SendAndSetLocalRotation(Quaternion.identity); });
            }
        }
    }

    //static function which gets the ASLObject's head
    private static void SetTrackedHead(GameObject newHead)
    {
        VRCameraToTrack = newHead;
    }

}
