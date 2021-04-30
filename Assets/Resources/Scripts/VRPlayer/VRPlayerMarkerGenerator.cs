using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VRPlayerMarkerGenerator : MonoBehaviour
{
    //======
    //PlayerCamera is the default VR Camera
    //PlayerTableViewCamera is the top-down view camera
    private Camera PlayerCamera;
    private Camera PlayerTableViewCamera;

    private static List<GameObject> SmallMapMarkerList = new List<GameObject>();
    private static List<GameObject> LargerMapMarkerList = new List<GameObject>();

    //=====
    //PCHandler/PC_Player_Canvas/PlacementDropdown
    public Dropdown MyDropdownList;
    //=========
    //LargerMapGenerator BigMap(Under PCPlane)
    //SmallMapGenerator is Small(Under VRPlane)
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
        //===================
        //VR player need to find the Camera here
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

        //====================
        //VR player need some key for the deleting.
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            RemoveLastMarker();
        }
    }

    //Project a local marker to the small map
    private void ProjectMarker()
    {
        //=======================
        //This function will keep running
        //This if statement if for the Player default camera
        if (PlayerCamera.isActiveAndEnabled == true)
        {
            //====================
            //This RaycastHit will keep shoot ray from playercamera
            //Once it hit the "Chunk" on SmallMap, it will shot the ProjectMarker
            //VR player may need to use the ray that shoot from controller.
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
        //==========================
        //This if statement if for the Player top table view camera
        //If VR player dont need the top-view camera, just delete this if statement.
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
        //=====================
        //VR player need some key to trigger this
        //VR player need to shoot the ray and get the hit.point
        //If VR player has some special way to get the hit.point, you need to replace all the hit.point.
        if (Input.GetMouseButtonDown(0))
        {
            //If player in first persion view
            if (PlayerCamera.isActiveAndEnabled == true)
            {
                Ray MouseRay = PlayerCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit Hit;
                if (Physics.Raycast(MouseRay, out Hit))
                {
                    //If mouse hit the small map
                    if (Hit.collider.tag == "Chunk" && Hit.collider.transform.parent.tag == "SpawnSmallMap")
                    {
                        string DropdownOpionValue = MyDropdownList.options[MyDropdownList.value].text;
                        ASL.ASLHelper.InstantiateASLObject(DropdownOpionValue, Hit.point, Quaternion.identity, "", "", GetSmallMapMarker);
                        GenerateMarkerOnLargerMap(Hit.point);

                    }
                    else if (Hit.collider.tag == "Chunk" && Hit.collider.transform.parent.tag == "SpawnLargerMap")
                    {
                        ASL.ASLHelper.InstantiateASLObject("Marker", Hit.point, Quaternion.identity, "", "", GetLargerMapMarker);
                        GenerateMarkerOnSmallMap(Hit.point);
                    }
                }
            }
            //If player in third persion view
            //==========================
            //This if statement if for the Player top table view camera
            //If VR player dont need the top-view camera, just delete this if statement.
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

    //Add the small map marker into the list.
    private static void GetSmallMapMarker(GameObject _myGameObject)
    {
        SmallMapMarkerList.Add(_myGameObject);
    }

    //Add the large map marker into the list and add it into ASLObjectTrackingSystem
    private static void GetLargerMapMarker(GameObject _myGameObject)
    {
        //Debug.Log("123");
        //ASLObjectTrackingSystem.AddObjectToTrack(_myGameObject.GetComponent<ASL.ASLObject>(), _myGameObject.transform);
        MinimapDisplay.AddRouteMarker(_myGameObject.transform);
        LargerMapMarkerList.Add(_myGameObject);
    }

    //Get position from small map and comvert is to larger map and generate a new marker on larger map
    private void GenerateMarkerOnLargerMap(Vector3 MarkerPosition)
    {
        //(MarkerPosition - SmallMapCenter) will get the math vector from smallmapcenter to marker
        Vector3 CenterToMarker = (MarkerPosition - SmallMapCenter) * (LargeMapSize / SmallMapSize);
        Vector3 NewPositionOnLargeMap = CenterToMarker + LargerMapCenter;
        ASL.ASLHelper.InstantiateASLObject("Marker", NewPositionOnLargeMap, Quaternion.identity, "", "", GetLargerMapMarker);
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
