using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadTool : MonoBehaviour
{
    //RoadTool is the class that places the VR Player's roads on the map.
    //alot of this class is modified parts of the playermarkergenerator class

    //there should be one and only one instance of this script in the unity scene.

    //reference to the script instance
    public static GameObject ThisGameObject;

    //bool which tracks whether or not this tool is active and should place objects on the map
    private static bool isActive = false;

    //references to the two maps
    public GameObject LargeMapGenerator;
    public GameObject SmallerMapGenerator;
    public static GameObject LargerMapGenerator;
    public static GameObject SmallMapGenerator;

    //positional map information
    private static Vector3 LargerMapCenter;
    private static Vector3 SmallMapCenter;
    private static int LargeMapSize;
    private static int SmallMapSize;

    //list of the road points on both maps
    private static List<GameObject> MySmallBrushList = new List<GameObject>();
    private static List<GameObject> MyLargerBrushList = new List<GameObject>();

    // Start is called before the first frame update
    //this will set many of the static references needed to run the class
    void Start()
    {
        ThisGameObject = this.gameObject;
        LargerMapCenter = LargeMapGenerator.transform.position;
        SmallMapCenter = SmallerMapGenerator.transform.position;
        LargeMapSize = LargeMapGenerator.GetComponent<GenerateMapFromHeightMap>().mapSize;
        SmallMapSize = SmallerMapGenerator.GetComponent<GenerateMapFromHeightMap>().mapSize;
        LargerMapGenerator = LargeMapGenerator;
        SmallMapGenerator = SmallerMapGenerator;
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

    //this function places roads along the map at the given position, it is in the VRTracedInput class for normal grip input
    public static void placeRoad(Vector3 position)
    {
        if (!isActive)
        {
            return;
        }
        ASL.ASLHelper.InstantiateASLObject("Brush", position, Quaternion.identity, "", "", GetEachBrushOnSmallMap);
        Vector3 NewPositionOneLargeMap = ((position - SmallMapGenerator.transform.position) * (LargeMapSize / SmallMapSize)) + LargerMapGenerator.transform.position;
        NewPositionOneLargeMap.y += 3f;
        ASL.ASLHelper.InstantiateASLObject("LargeBrush", NewPositionOneLargeMap, Quaternion.identity, "", "", GetEachBrushOnLargerMap);
    }

    //creates the road spot on the small map
    private static void GetEachBrushOnSmallMap(GameObject _myGameObject)
    {
        _myGameObject.transform.parent = ThisGameObject.transform;
        MySmallBrushList.Add(_myGameObject);
    }

    //creates the road spot on the big map
    private static void GetEachBrushOnLargerMap(GameObject _myGameObject)
    {
        _myGameObject.transform.parent = ThisGameObject.transform;
        MyLargerBrushList.Add(_myGameObject);
        //GenerateExtraLineOnMap(_myGameObject);
    }

    //generates the lines between points on the map
    //private static void GenerateExtraLineOnMap(GameObject _myGameObject)
    //{
    //    if (MyLargerBrushList.Count == 1)
    //    {
    //        return;
    //    }
    //    else
    //    {
    //        _myGameObject.GetComponent<MapBrushRePosition>().SecondToLastBrushPosition = MyLargerBrushList[MyLargerBrushList.Count - 2].transform.position;
    //    }
    //}
}
