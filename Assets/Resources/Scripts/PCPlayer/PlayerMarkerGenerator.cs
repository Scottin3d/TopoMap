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

    void Awake()
    {
        //Find all Camera and MiniMap Display
        PlayerCamera = GameObject.Find("PCHandler/Player").GetComponentInChildren<Camera>();
        PlayerTableViewCamera = GameObject.Find("PCHandler/PlayerTopViewCamera").GetComponentInChildren<Camera>();
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
        //Click Left mouse
        if (Input.GetMouseButtonDown(0))
        {
            //If player in first persion view
            if (PlayerCamera.isActiveAndEnabled == true)
            {
                Ray MouseRay = PlayerCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit Hit;
                if (Physics.Raycast(MouseRay, out Hit))
                {
                    //Test select of path display
                    PathDisplay.Select(Hit.transform);

                    string DropdownOpionValue = "";
                    //If mouse hit the small map
                    if (Hit.collider.tag == "Chunk" && Hit.collider.transform.parent.tag == "SpawnSmallMap")
                    {
                        DropdownOpionValue = MyDropdownList.options[MyDropdownList.value].text;
                        if (DropdownOpionValue == "Marker")
                        {
                            DropdownOpionValue = "PlayerMarker";
                        }
                        else
                        {
                            DropdownOpionValue = "PlayerRouteMarker";
                        }

                        Vector3 CenterToMarker = (Hit.point - SmallMapCenter) * (LargeMapSize / SmallMapSize);
                        Vector3 NewPositionOnLargeMap = CenterToMarker + LargerMapCenter;

                        ASL.ASLHelper.InstantiateASLObject(DropdownOpionValue, NewPositionOnLargeMap, Quaternion.identity, "", "", GetSmallMapMarker);
                        //GenerateMarkerOnLargerMap(Hit.point);

                    }
                    else if (Hit.collider.tag == "Chunk" && Hit.collider.transform.parent.tag == "SpawnLargerMap")
                    {
                        DropdownOpionValue = MyDropdownList.options[MyDropdownList.value].text;
                        ASL.ASLHelper.InstantiateASLObject(DropdownOpionValue, Hit.point, Quaternion.identity, "", "", GetLargerMapMarker);
                        //GenerateMarkerOnSmallMap(Hit.point);
                    }
                }
            }
            //If player in third persion view
            if (PlayerTableViewCamera.isActiveAndEnabled == true)
            {
                Ray MouseRay = PlayerTableViewCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit Hit;
                if (Physics.Raycast(MouseRay, out Hit))
                {
                    if (Hit.collider.tag == "Chunk" && Hit.collider.transform.parent.tag == "SpawnSmallMap")
                    {
                        /*string DropdownOpionValue = MyDropdownList.options[MyDropdownList.value].text;
                        if (DropdownOpionValue == "Marker")
                        {
                            DropdownOpionValue = "PlayerMarker";
                        }
                        else
                        {
                            DropdownOpionValue = "PlayerRouteMarker";
                        }
                        ASL.ASLHelper.InstantiateASLObject(DropdownOpionValue, Hit.point, Quaternion.identity, "", "", GetSmallMapMarker);*/
                        ASL.ASLHelper.InstantiateASLObject("PlayerMarker", Hit.point, Quaternion.identity, "", "", GetSmallMapMarker);
                        GenerateMarkerOnLargerMap(Hit.point);
                    }
                }
            }
        }
    }

    private void SpawnRegularMarker(Vector3 HitPoint)
    {

    }

    private static void GetSmallMapMarker(GameObject _myGameObject)
    {
        ASLObjectTrackingSystem.AddObjectToTrack(_myGameObject.GetComponent<ASL.ASLObject>(), _myGameObject.transform);
        SmallMapMarkerList.Add(_myGameObject);
    }

    //Add the large map marker into the list and add it into ASLObjectTrackingSystem
    private static void GetLargerMapMarker(GameObject _myGameObject)
    {
        ASLObjectTrackingSystem.AddObjectToTrack(_myGameObject.GetComponent<ASL.ASLObject>(), _myGameObject.transform);
        //MiniMapDisplayObject.GetComponent<MinimapDisplay>().AddRouteMarker(_myGameObject.transform.position);
        RouteDisplayV2.AddRouteMarker(_myGameObject.transform);
        LargerMapMarkerList.Add(_myGameObject);
    }

    //Get position from small map and comvert is to larger map and generate a new marker on larger map
    private void GenerateMarkerOnLargerMap(Vector3 MarkerPosition)
    {
        //(MarkerPosition - SmallMapCenter) will get the math vector from smallmapcenter to marker
        Vector3 CenterToMarker = (MarkerPosition - SmallMapCenter) * (LargeMapSize / SmallMapSize);
        Vector3 NewPositionOnLargeMap = CenterToMarker + LargerMapCenter;
        
        //ASL.ASLHelper.InstantiateASLObject("Marker", NewPositionOnLargeMap, Quaternion.identity, "", "", GetLargerMapMarker);
    }

    //Get position from larger map and convert is to small map and generate a new marker on small map
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
            RouteDisplayV2.RemoveRouteMarker(LMarker.transform, false);

            LMarker.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
            {
                LMarker.GetComponent<ASL.ASLObject>().DeleteObject();
            });

            LargerMapMarkerList.RemoveAt(LargerMapMarkerList.Count - 1);
        }
    }
}
