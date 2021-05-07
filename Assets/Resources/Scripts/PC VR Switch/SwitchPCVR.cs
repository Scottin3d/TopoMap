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
        var XRDisplaySubsystems = new List<XRDisplaySubsystem>();
        SubsystemManager.GetInstances<XRDisplaySubsystem>(XRDisplaySubsystems);
        foreach (var XRDisplay in XRDisplaySubsystems)
        {
            if (XRDisplay.running)
            {
                PC.SetActive(false);
            }
        }

        if (PC.activeSelf)
        {
            VR.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
