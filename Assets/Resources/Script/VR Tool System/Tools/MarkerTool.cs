using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerTool : MonoBehaviour
{
    //MarkerTool is the class that places the VR Player's markers on the map.
    //alot of this class is modified parts of the playermarkergenerator class
    
    //there should be one and only one instance of this script in the unity scene.

    //bool which tracks whether or not this tool is active and should place objects on the map
    private static bool isActive = false;

    //references to the two maps
    public GameObject LargerMapGenerator;
    public GameObject SmallMapGenerator;

    //positional map information
    private static Vector3 LargerMapCenter;
    private static Vector3 SmallMapCenter;
    private static int LargeMapSize;
    private static int SmallMapSize;

    //list of markers on the map
    private static List<GameObject> LargerMapMarkerList = new List<GameObject>();

    // Start is called before the first frame update
    //this will set many of the static references needed to run the class
    void Start()
    {
        LargerMapCenter = LargerMapGenerator.transform.position;
        SmallMapCenter = SmallMapGenerator.transform.position;
        LargeMapSize = LargerMapGenerator.GetComponent<GenerateMapFromHeightMap>().mapSize;
        SmallMapSize = SmallMapGenerator.GetComponent<GenerateMapFromHeightMap>().mapSize;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //activate and deactivate change the status of this tool when called.
    public static void activate()
    {
        isActive = true;
    }
    public static void deactivate()
    {
        isActive = false;
    }

    //this function places a marker at the given position, it is called in the VRTracedInput class as a first grip frame action.
    //the position should be given in world coordinates
    public static void placeMarker(Vector3 position)
    {
        if (!isActive)
        {
            return;
        }
        Vector3 CenterToMarker = (position - SmallMapCenter) * (LargeMapSize / SmallMapSize);
        Vector3 NewPositionOnLargeMap = CenterToMarker + LargerMapCenter;
        ASL.ASLHelper.InstantiateASLObject("Marker", NewPositionOnLargeMap, Quaternion.identity, "", "", GetLargerFromSmaller);
    }

    private static void GetLargerFromSmaller(GameObject _myGameObject)
    {
        ASLObjectTrackingSystem.AddObjectToTrack(_myGameObject.GetComponent<ASL.ASLObject>());
        //MiniMapDisplayObject.GetComponent<MinimapDisplay>().AddRouteMarker(_myGameObject.transform.position);
        RouteDisplayV2.AddRouteMarker(_myGameObject.transform);
        LargerMapMarkerList.Add(_myGameObject);
    }
}
