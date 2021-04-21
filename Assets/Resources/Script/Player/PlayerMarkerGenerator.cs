using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMarkerGenerator : MonoBehaviour
{
    private GameObject SelectedMap;
    private Camera PlayerCamera;
    private static GameObject MarkerObject;
    public static List<GameObject> PlayerSetMarker = new List<GameObject>();
    public Dropdown MyDropdownList;

    public GameObject LargerMapGenerator;
    public GameObject SmallMapGenerator;

    private Vector3 LargerMapCenter;
    private Vector3 SmallMapCenter;
    private float LargerMapHeight;
    private float SmallMapHeight;
    private int LargeMapSize;
    private int SmallMapSize;
    private float SmallScaleUp;

    void Awake()
    {
        PlayerCamera = GameObject.Find("Player").GetComponentInChildren<Camera>();
    }

    // Start is called before the first frame update
    void Start()
    {
        LargerMapCenter = LargerMapGenerator.transform.position;
        SmallMapCenter = SmallMapGenerator.transform.position;
        LargerMapHeight = LargerMapGenerator.GetComponent<GenerateMapFromHeightMap>().meshHeight;
        SmallMapHeight = SmallMapGenerator.GetComponent<GenerateMapFromHeightMap>().meshHeight;
        LargeMapSize = LargerMapGenerator.GetComponent<GenerateMapFromHeightMap>().mapSize;
        SmallMapSize = SmallMapGenerator.GetComponent<GenerateMapFromHeightMap>().mapSize;
        SmallScaleUp = LargeMapSize / SmallMapSize;
    }

    // Update is called once per frame
    void Update()
    {
        SelectObjectByClick();
    }

    private void SelectObjectByClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray MouseRay = PlayerCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit Hit;
            if (Physics.Raycast(MouseRay, out Hit))
            {
                if (Hit.collider.tag == "Chunk")
                {
                    string DropdownOpionValue = MyDropdownList.options[MyDropdownList.value].text;
                    ASL.ASLHelper.InstantiateASLObject(DropdownOpionValue, Hit.point, Quaternion.identity, "", "", GetHoldObject);
                    GenerateMarkerOnLargerMap(Hit.point);
                }
                else
                {
                    return;
                }
            }
        }
    }

    private static void GetHoldObject(GameObject _myGameObject)
    {
        MarkerObject = _myGameObject;
        PlayerSetMarker.Add(_myGameObject);
    }

    private void GenerateMarkerOnLargerMap(Vector3 MarkerPosition)
    {
        Vector3 CenterToMarker = VectorFromSmallMapCenterToMarker(MarkerPosition) * SmallScaleUp / 2;
        Vector3 NewPositionOnLargeMap = CenterToMarker + LargerMapCenter;
        SpawnMarkerOnLargerMap(NewPositionOnLargeMap);
    }

    //Get the math vector
    private Vector3 VectorFromSmallMapCenterToMarker(Vector3 MarkerPosition)
    {
        //Direction of this Vector is from SmallMapCenter to Marker
        Vector3 V = MarkerPosition - SmallMapCenter;
        return V;
    }

    private void SpawnMarkerOnLargerMap(Vector3 PositionOnLargerMap)
    {
        ASL.ASLHelper.InstantiateASLObject("Marker", PositionOnLargerMap, Quaternion.identity);
    }
}
