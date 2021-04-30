using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchPCVR : MonoBehaviour
{
    public GameObject VR;
    public GameObject PC;

    private void Awake()
    {
        if (SystemInfo.deviceType == DeviceType.Desktop)
        {
            //VR.SetActive(false);
        }
        else
        {
            //PC.SetActive(false);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
