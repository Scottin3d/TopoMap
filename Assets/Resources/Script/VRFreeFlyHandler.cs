using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRFreeFlyHandler : MonoBehaviour
{
    private static GameObject VRCollider = null;



    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("waitForVRStart");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void updateFreeFly()
    {
        VRCollider.SetActive(!StaticVRVariables.inVRFreeFlight);
    }

    IEnumerator waitForVRStart()
    {
        while(VRStartupController.VRPlayerObject == null)
        {
            yield return new WaitForSeconds(0.1f);
        }
        VRCollider = VRStartupController.VRPlayerObject.transform.Find("SteamVRObjects/BodyCollider").gameObject;
    }
}
