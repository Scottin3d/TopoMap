using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class ASLVRTrack : MonoBehaviour
{
    //put this class on a VR hand and it will create a representative box synced across all clients which represents the hand of the VR player



    private static ASLObject leftHand = null;
    private static ASLObject rightHand = null;
    public static GameObject leftHandLocal = null; //left hand to be tracked
    public static GameObject rightHandLocal = null; //right hand to be tracked
    public GameObject leftHandLocalNS = null; //non static version to be set in editor
    public GameObject rightHandLocalNS = null; //non static version to be set in editor
    public static GameObject lHandStore = null; //temp storage for the left hand while ASLObject is instantiated
    public static GameObject rHandStore = null; //temp storage for the right hand while ASLObject is instantiated
    //Add head when ready, for now focusing on hand tracking and sync
    //private static ASLObject Head = null;

    // Start is called before the first frame update
    void Start()
    {
        leftHandLocal = leftHandLocalNS;
        rightHandLocal = rightHandLocalNS;
        ASL.ASLHelper.InstantiateASLObject("ASLVRHand", new Vector3(0,0,0), Quaternion.identity, "", "", SetLeftTrackedHand);
        ASL.ASLHelper.InstantiateASLObject("ASLVRHand", new Vector3(0, 0, 0), Quaternion.identity, "", "", SetRightTrackedHand);
        StartCoroutine("UpdatePositions");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator UpdatePositions()
    {
        yield return new WaitForSeconds(0.5f); //very hack-ey but even the calls in the while loops were causing a null reference exception,
                                               //so until I can find a fix for this the .5s wait will have to suffice
        while (lHandStore.GetComponent<ASLObject>() == null)
        {
            yield return new WaitForSeconds(0.5f);
        }
        while (rHandStore.GetComponent<ASLObject>() == null)
        {
            yield return new WaitForSeconds(0.5f);
        }
        while (true)
        {
            //Debug.Log(leftHand.transform.position);
            //Debug.Log(leftHandLocal.transform.position);
            //handToTrack.SendAndSetClaim(SendAndSetLocalPosition(this.transform.position));
            lHandStore.GetComponent<ASLObject>().SendAndSetClaim(() => { lHandStore.GetComponent<ASLObject>().SendAndSetLocalPosition(leftHandLocal.transform.position); lHandStore.GetComponent<ASLObject>().SendAndSetLocalRotation(leftHandLocal.transform.rotation); });
            rHandStore.GetComponent<ASLObject>().SendAndSetClaim(() => { rHandStore.GetComponent<ASLObject>().SendAndSetLocalPosition(rightHandLocal.transform.position); rHandStore.GetComponent<ASLObject>().SendAndSetLocalRotation(rightHandLocal.transform.rotation); });
            yield return new WaitForSeconds(0.5f); //update twice per second
                                                   //.SendAndSetClaim(() =>{Cube.GetComponent<ASL.ASLObject>().SendAndSetWorldRotation(PlayerObject.transform.rotation);Cube.GetComponent<ASL.ASLObject>().SendAndSetWorldPosition(PlayerObject.transform.position);
        }
    }

    private static void SetLeftTrackedHand(GameObject newHand)
    {
        lHandStore = newHand;
        //if(newHand.GetComponent<ASLObject>() == null)
        //{
        //    StartCoroutine("waitForLInit");
        //    
        //}
        //leftHand = newHand.GetComponent<ASLObject>();
    }

    private static void SetRightTrackedHand(GameObject newHand)
    {
        rHandStore = newHand;
        //rightHand = newHand.GetComponent<ASLObject>();
    }

    IEnumerator waitForLInit()
    {
        yield return new WaitForSeconds(0.5f);
        SetLeftTrackedHand(lHandStore);
    }

    IEnumerator waitForRInit()
    {
        yield return new WaitForSeconds(0.5f);
        SetRightTrackedHand(rHandStore);
    }
}
