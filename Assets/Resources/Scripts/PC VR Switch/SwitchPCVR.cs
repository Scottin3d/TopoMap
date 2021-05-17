using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class SwitchPCVR : MonoBehaviour
{
    public GameObject VR;
    public GameObject PC;

    // Start is called before the first frame update
    void Start()
    {
        VR.SetActive(false);
        //var XRDisplaySubsystems = new List<XRDisplaySubsystem>();
        //SubsystemManager.GetInstances<XRDisplaySubsystem>(XRDisplaySubsystems);
        //foreach (var XRDisplay in XRDisplaySubsystems)
        //{
        //    Debug.Log(XRDisplay);
        //    Debug.Log(XRDisplay.running);
        //    if (XRDisplay.running)
        //    {
        //        PC.SetActive(false);
        //    }
        //}

        //if (PC.activeSelf)
        //{
        //    VR.SetActive(false);
        //}
    }

    public void ToggleToVR()
    {
        VR.SetActive(true);
        PC.SetActive(false);
    }
}
