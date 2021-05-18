using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRMarkerTestingScript : MonoBehaviour
{
    //this is a script meant for testing various things with the marker system in VR.
    //first test is to see if I can use raycasts from the player's hand to get a position on the small map

    //hand objects which I will need
    private GameObject leftHand = null; //left hand of the VR player
    private GameObject rightHand = null;//right hand of the VR player

    //taken from marker generation as well, but just denotes something to show when looking at the map
    private GameObject LocalProjectMarker;


    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("delayInitialization");
        LocalProjectMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        LocalProjectMarker.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        LocalProjectMarker.GetComponent<Renderer>().material.color = Color.red;
    }

    private void StartUp()
    {
        leftHand = VRStartupController.VRPlayerObject.transform.Find("SteamVRObjects/LeftHand").gameObject;
        rightHand = VRStartupController.VRPlayerObject.transform.Find("SteamVRObjects/RightHand").gameObject;
    }

    IEnumerator delayInitialization()
    {
        while (VRStartupController.VRPlayerObject == null)
        {
            yield return new WaitForSeconds(0.1f);
        }
        StartUp();
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    //code from the current marker generation code (will be getting changed):
    //Project a local marker to the small map
    /*
    private void ProjectMarker()
    {
        if (PlayerCamera.isActiveAndEnabled == true)
        {
            Ray MouseRay = PlayerCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit Hit;
            if (Physics.Raycast(MouseRay, out Hit))
            {
                if (Hit.collider.tag == "Chunk" && Hit.collider.transform.parent.tag == "SpawnSmallMap")
                {
                    LocalProjectMarker.SetActive(true);
                    LocalProjectMarker.transform.position = Hit.point;
                }
                else
                {
                    LocalProjectMarker.SetActive(false);
                }
            }
        }

        if (PlayerTableViewCamera.isActiveAndEnabled == true)
        {
            Ray MouseRay = PlayerTableViewCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit Hit;
            if (Physics.Raycast(MouseRay, out Hit))
            {
                if (Hit.collider.tag == "Chunk" && Hit.collider.transform.parent.tag == "SpawnSmallMap")
                {
                    LocalProjectMarker.SetActive(true);
                    LocalProjectMarker.transform.position = Hit.point;
                }
                else
                {
                    LocalProjectMarker.SetActive(false);
                }
            }
        }
    }
    */
}
