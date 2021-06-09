using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class ASLVRTrack : MonoBehaviour
{
    //put this class on a VR hand and it will create a representative box synced across all clients which represents the hand of the VR player



    public static GameObject leftHandLocal = null; //left hand to be tracked
    public static GameObject rightHandLocal = null; //right hand to be tracked
    public GameObject leftHandLocalNS = null; //non static version to be set in editor
    public GameObject rightHandLocalNS = null; //non static version to be set in editor
    public static GameObject lHandStore = null; //temp storage for the left hand while ASLObject is instantiated
    public static GameObject rHandStore = null; //temp storage for the right hand while ASLObject is instantiated

    // Start is called before the first frame update
    //begins the coroutine which delays variable initialization.
    void Start()
    {
        StartCoroutine("delayInitialization");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //this function initializes behavior and variables dependant on the VR Player being active.
    private void Startup()
    {
        leftHandLocalNS = VRStartupController.VRPlayerObject.transform.Find("SteamVRObjects/LeftHand").gameObject;
        rightHandLocalNS = VRStartupController.VRPlayerObject.transform.Find("SteamVRObjects/RightHand").gameObject;
        leftHandLocal = leftHandLocalNS;
        rightHandLocal = rightHandLocalNS;
        ASL.ASLHelper.InstantiateASLObject("ASLVRHand", new Vector3(0, 0, 0), Quaternion.identity, "", "", SetLeftTrackedHand);
        ASL.ASLHelper.InstantiateASLObject("ASLVRHand", new Vector3(0, 0, 0), Quaternion.identity, "", "", SetRightTrackedHand);
        StartCoroutine("UpdatePositions");
    }

    //coroutine to wait for VR to start up, and then to push in the hand objects to be tracked
    IEnumerator delayInitialization()
    {
        while(VRStartupController.VRPlayerObject == null)
        {
            yield return new WaitForSeconds(0.1f);
        }
        Startup();
    }

    //this is a coroutine which tracks the VR Player's hands over ASL in order
    //to represent the VR Player's hands over all clients.
    IEnumerator UpdatePositions()
    {

        while (lHandStore == null)
        {
            yield return new WaitForSeconds(0.5f);
        }
        while (rHandStore == null)
        {
            yield return new WaitForSeconds(0.5f);
        }
        lHandStore.GetComponent<Renderer>().enabled = false;
        rHandStore.GetComponent<Renderer>().enabled = false;
        while (true)
        {
            //Debug.Log(leftHand.transform.position);
            //Debug.Log(leftHandLocal.transform.position);
            lHandStore.GetComponent<ASLObject>().SendAndSetClaim(() => { lHandStore.GetComponent<ASLObject>().SendAndSetLocalPosition(leftHandLocal.transform.position); lHandStore.GetComponent<ASLObject>().SendAndSetLocalRotation(leftHandLocal.transform.rotation); });
            rHandStore.GetComponent<ASLObject>().SendAndSetClaim(() => { rHandStore.GetComponent<ASLObject>().SendAndSetLocalPosition(rightHandLocal.transform.position); rHandStore.GetComponent<ASLObject>().SendAndSetLocalRotation(rightHandLocal.transform.rotation); });
            yield return new WaitForSeconds(0.1f); //update ten times per second
        }
    }

    //sets the left hand which is to be tracked.
    private static void SetLeftTrackedHand(GameObject newHand)
    {
        lHandStore = newHand;
    }

    //sets the right hand which is to be tracked.
    private static void SetRightTrackedHand(GameObject newHand)
    {
        rHandStore = newHand;
    }
}
