using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

public class RulerTool : MonoBehaviour
{

    //RoadTool is the class that handles the VR player's ruler which they can use to get the scale of the map they are using

    //there should be one and only one instance of this script in the unity scene.

    //reference to the script instance
    public static GameObject ThisGameObject;

    //references to the VR player's objects
    public static Transform leftHandTransform;
    public static Transform rightHandTransform;
    //public static Transform headTransform;

    private static GameObject theRuler;
    private static float rulerSize = 1f;//scale of the ruler

    //references to the big and small map
    public GenerateMapFromHeightMap localBigMap;
    public GenerateMapFromHeightMap localSmallMap;
    public static GenerateMapFromHeightMap BigMap;
    public static GenerateMapFromHeightMap SmallMap;

    //bool which tracks whether or not this tool is active and should place objects on the map
    private static bool isActive = false;


    // Start is called before the first frame update
    void Start()
    {
        ThisGameObject = this.gameObject;
        theRuler = (GameObject)Instantiate(Resources.Load("VR/VRRuler"));
        BigMap = localBigMap;
        SmallMap = localSmallMap;
        StartCoroutine("delayInitialization");
    }

    private void StartUp()
    {
        leftHandTransform = VRStartupController.VRPlayerObject.transform.Find("SteamVRObjects/LeftHand");
        rightHandTransform = VRStartupController.VRPlayerObject.transform.Find("SteamVRObjects/RightHand");
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
        if(isActive && SteamVR_Actions.default_GrabGrip.state)
        {
            //place ruler between hands
            Vector3 vectorFromLeftToRight = rightHandTransform.position - leftHandTransform.position;
            Vector3 newPosition = leftHandTransform.position + (vectorFromLeftToRight / 2);
            Vector3 newScale = theRuler.transform.localScale;
            newScale.z = vectorFromLeftToRight.magnitude;
            rulerSize = newScale.z;
            theRuler.transform.position = newPosition;
            theRuler.transform.localScale = newScale;
            theRuler.transform.LookAt(rightHandTransform);
            updateRulerText();
        }
        
    }

    public static void activate()
    {
        isActive = true;
    }
    public static void deactivate()
    {
        isActive = false;
    }

    //scale of ruler * size of large map / size of small map = length in meters
    //this function finds (in km) the size of the ruler and displays it on the ruler.
    //this function assumes that the big map is 1-1 with real life
    private static void updateRulerText()
    {
        float totalKM = (rulerSize * (BigMap.mapSize / SmallMap.mapSize))/1000f;
        theRuler.GetComponentInChildren<Text>().text = "" + totalKM + " KM";
    }
}
