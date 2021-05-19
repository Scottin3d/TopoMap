using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class VRTracedInput : MonoBehaviour
{
    //this is a script meant to handle any input relating to tracing a line from the VR player's right hand.
    //this means pointing at the map for route and marker placement, for example

    //hand objects which I will need
    private GameObject leftHand = null; //left hand of the VR player
    private GameObject rightHand = null;//right hand of the VR player

    //taken from marker generation as well, but just denotes something to show when looking at the map
    private GameObject LocalProjectMarker;

    //need two objects to create a line along the index finger
    private GameObject backObject = null;//object at the base of the hand
    //T: 0.037f, -0.001f, -0.0657f
    private GameObject frontObject = null;//object at the tip of the finger
    //T: 0.02905f, -0.0572f, -0.0038f

    private bool initialGrip = true; //bool for differentiating between the first time a grip is detected and all subsequent frames


    // Start is called before the first frame update
    void Start()
    {
        LocalProjectMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        LocalProjectMarker.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        LocalProjectMarker.GetComponent<Renderer>().material.color = Color.red;
        LocalProjectMarker.GetComponent<SphereCollider>().enabled = false;
        backObject = new GameObject("BackOfFinger");
        frontObject = new GameObject("FrontOfFinger");
        StartCoroutine("delayInitialization");
    }

    private void StartUp()
    {
        leftHand = VRStartupController.VRPlayerObject.transform.Find("SteamVRObjects/LeftHand").gameObject;
        rightHand = VRStartupController.VRPlayerObject.transform.Find("SteamVRObjects/RightHand").gameObject;

        backObject.transform.SetParent(rightHand.transform);
        frontObject.transform.SetParent(rightHand.transform);
        backObject.transform.localPosition = new Vector3(0.037f, -0.001f, -0.0657f);
        frontObject.transform.localPosition = new Vector3(0.02905f, -0.0572f, -0.0038f);
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
        if (SteamVR_Actions.default_GrabGrip.state)
        {
            ProjectMarker();
            
        }
        else
        {
            initialGrip = true;
        }
    }



    //code from the current marker generation code (will be getting changed):
    //Project a local marker to the small map
    private void ProjectMarker()
    {
        Vector3 startPosition = frontObject.transform.position;
        Vector3 rayDirection = (frontObject.transform.position - backObject.transform.position).normalized;
        Ray MouseRay = new Ray(startPosition, rayDirection);
        RaycastHit Hit;
        if (Physics.Raycast(MouseRay, out Hit))
        {
            //Debug.Log("in raycast hit");
            if (Hit.collider.tag == "Chunk" && Hit.collider.transform.parent.tag == "SpawnSmallMap")
            {
                LocalProjectMarker.SetActive(true);
                LocalProjectMarker.transform.position = Hit.point;
                if (initialGrip)
                {
                    messageOnInitialGrip(Hit.point);
                    initialGrip = false;
                }
                else
                {
                    messageOnGrip(Hit.point);
                }
            }
            else
            {
                LocalProjectMarker.transform.position = Hit.point;
                //LocalProjectMarker.SetActive(false);
            }
        }
    }

    //this function will call classes which need to know when the player initially grips their controller
    private void messageOnInitialGrip(Vector3 position)
    {
        MarkerTool.placeMarker(position);
    }

    //this function will call classes which need to know when the player holds down grip
    private void messageOnGrip(Vector3 position)
    {

    }
}
