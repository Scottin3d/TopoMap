using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMarkerGenerator : MonoBehaviour
{
    private Camera PlayerCamera;
    private Camera PlayerTableViewCamera;

    private static List<GameObject> SmallMapMarkerList = new List<GameObject>();
    private static List<GameObject> LargerMapMarkerList = new List<GameObject>();

    public Dropdown MyDropdownList;
    public GameObject LargerMapGenerator;
    public GameObject SmallMapGenerator;

    private Vector3 LargerMapCenter;
    private Vector3 SmallMapCenter;
    private int LargeMapSize;
    private int SmallMapSize;

    private GameObject LocalProjectMarker;
    private static GameObject MiniMapDisplayObject;

    void Awake()
    {
        //Find all Camera and MiniMap Display
        PlayerCamera = GameObject.Find("PCHandler/Player").GetComponentInChildren<Camera>();
        PlayerTableViewCamera = GameObject.Find("PCHandler/PlayerTopViewCamera").GetComponentInChildren<Camera>();
        MiniMapDisplayObject = GameObject.Find("PCHandler/MiniMapDisplay");
    }

    // Start is called before the first frame update
    void Start()
    {
        LargerMapCenter = LargerMapGenerator.transform.position;
        SmallMapCenter = SmallMapGenerator.transform.position;
        LargeMapSize = LargerMapGenerator.GetComponent<GenerateMapFromHeightMap>().mapSize;
        SmallMapSize = SmallMapGenerator.GetComponent<GenerateMapFromHeightMap>().mapSize;
        LocalProjectMarker = Instantiate(Resources.Load("MyPrefabs/PlayerMarker") as GameObject);
        LocalProjectMarker.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        ProjectMarker();
        SelectObjectByClick();

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            RemoveLastMarker();
        }
    }

    //Project a local marker to the small map
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

    private void SelectObjectByClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (PlayerCamera.isActiveAndEnabled == true)
            {
                Ray MouseRay = PlayerCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit Hit;
                if (Physics.Raycast(MouseRay, out Hit))
                {
                    if (Hit.collider.tag == "Chunk" && Hit.collider.transform.parent.tag == "SpawnSmallMap")
                    {
                        string DropdownOpionValue = MyDropdownList.options[MyDropdownList.value].text;
                        ASL.ASLHelper.InstantiateASLObject(DropdownOpionValue, Hit.point, Quaternion.identity, "", "", GetSmallMapMarker);
                        GenerateMarkerOnLargerMap(Hit.point);

                    }
                    else if (Hit.collider.tag == "Chunk" && Hit.collider.transform.parent.tag == "SpawnLargerMap")
                    {
                        string DropdownOpionValue = MyDropdownList.options[MyDropdownList.value].text;
                        ASL.ASLHelper.InstantiateASLObject(DropdownOpionValue, Hit.point, Quaternion.identity, "", "", GetLargerMapMarker);
                        GenerateMarkerOnSmallMap(Hit.point);
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
                        string DropdownOpionValue = MyDropdownList.options[MyDropdownList.value].text;
                        ASL.ASLHelper.InstantiateASLObject(DropdownOpionValue, Hit.point, Quaternion.identity, "", "", GetSmallMapMarker);
                        GenerateMarkerOnLargerMap(Hit.point);
                    }
                }
            }
        }
    }

    private static void GetSmallMapMarker(GameObject _myGameObject)
    {
        SmallMapMarkerList.Add(_myGameObject);
    }

    private static void GetLargerMapMarker(GameObject _myGameObject)
    {
        ASLObjectTrackingSystem.AddObjectToTrack(_myGameObject.GetComponent<ASL.ASLObject>(), _myGameObject.transform);
        //MiniMapDisplayObject.GetComponent<MinimapDisplay>().AddRouteMarker(_myGameObject.transform.position);
        MinimapDisplay.AddRouteMarker(_myGameObject.transform);
        LargerMapMarkerList.Add(_myGameObject);
    }

    //Get position from small map and comvert is to larger map
    private void GenerateMarkerOnLargerMap(Vector3 MarkerPosition)
    {
        //(MarkerPosition - SmallMapCenter) will get the math vector from smallmapcenter to marker
        Vector3 CenterToMarker = (MarkerPosition - SmallMapCenter) * (LargeMapSize / SmallMapSize);
        Vector3 NewPositionOnLargeMap = CenterToMarker + LargerMapCenter;
        ASL.ASLHelper.InstantiateASLObject("Marker", NewPositionOnLargeMap, Quaternion.identity, "", "", GetLargerMapMarker);
    }

    //Get position from larger map and convert is to small map
    private void GenerateMarkerOnSmallMap(Vector3 MarkerPosition)
    {
        //(MarkerPosition - LargerMapCenter) will get the math vector from largermapcenter to marker
        Vector3 CenterToMarker = (MarkerPosition - LargerMapCenter) / (LargeMapSize / SmallMapSize);
        Vector3 NewPositionOnLargeMap = CenterToMarker + SmallMapCenter;
        ASL.ASLHelper.InstantiateASLObject("PlayerMarker", NewPositionOnLargeMap, Quaternion.identity, "", "", GetSmallMapMarker);
    }

    private void RemoveLastMarker()
    {
        if (SmallMapMarkerList.Count > 0)
        {
            GameObject SMarker = SmallMapMarkerList[SmallMapMarkerList.Count - 1];

            SMarker.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
            {
                SMarker.GetComponent<ASL.ASLObject>().DeleteObject();
            });

            SmallMapMarkerList.RemoveAt(SmallMapMarkerList.Count - 1);
        }
        if (LargerMapMarkerList.Count > 0)
        {
            GameObject LMarker = LargerMapMarkerList[LargerMapMarkerList.Count - 1];
            MinimapDisplay.RemoveRouteMarker(LMarker.transform);

            LMarker.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
            {
                LMarker.GetComponent<ASL.ASLObject>().DeleteObject();
            });

            LargerMapMarkerList.RemoveAt(LargerMapMarkerList.Count - 1);
        }
    }
}
