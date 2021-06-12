using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRFreeFlyHandler : MonoBehaviour
{
    //VRFreeFlyHandler controls the VR player's movement script when they toggle free flight,
    //enabling a component object which enables the "walking" behavior for the player.

    //reference to the VR body collider in the VR player
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

    //toggles free flight for the player by enabling or disabling the gameobject referenced.
    public static void updateFreeFly()
    {
        VRCollider.SetActive(!StaticVRVariables.inVRFreeFlight);
    }

    //delays initialization of the variable until the VR player has been created
    IEnumerator waitForVRStart()
    {
        while(VRStartupController.VRPlayerObject == null)
        {
            yield return new WaitForSeconds(0.1f);
        }
        VRCollider = VRStartupController.VRPlayerObject.transform.Find("SteamVRObjects/BodyCollider").gameObject;
    }
}
