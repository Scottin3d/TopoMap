﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayerMarkerGenerator : MonoBehaviour
{
    private static PlayerMarkerGenerator generator;
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

    private GameObject DrawLineMarker;
    private GameObject DrawLine;
    private GameObject DrawOrigin;
    private float drawTime = 0.5f;

    public Color OriginColor;
    private Color SelectedColor = new Color(1f, 1f, 0f, 1f);

    private bool deleteMode = false;

    void Awake()
    {
        generator = this;
        //Find all Camera and MiniMap Display
        PlayerCamera = GameObject.Find("PCHandler/Player").GetComponentInChildren<Camera>();
        PlayerTableViewCamera = GameObject.Find("PCHandler/PlayerTopViewCamera").GetComponentInChildren<Camera>();
    }

    // Start is called before the first frame update
    void Start() {
        LargerMapCenter = LargerMapGenerator.transform.position;
        SmallMapCenter = SmallMapGenerator.transform.position;
        LargeMapSize = LargerMapGenerator.GetComponent<GenerateMapFromHeightMap>().mapSize;
        SmallMapSize = SmallMapGenerator.GetComponent<GenerateMapFromHeightMap>().mapSize;
        LocalProjectMarker = Instantiate(Resources.Load("MyPrefabs/PlayerMarker") as GameObject);
        Destroy(LocalProjectMarker.GetComponent<BoxCollider>());
        LocalProjectMarker.SetActive(false);

        DrawLine = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Destroy(DrawLine.GetComponent<CapsuleCollider>());
        DrawLine.GetComponent<MeshRenderer>().material.color = Color.cyan;
        DrawLine.layer = 11;
        DrawLine.SetActive(false);
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.R)) {
            deleteMode = !deleteMode;
        }

        ProjectMarker();
        SelectObjectByClick();
        WhileClickDown();
        ClickRelease();

        if (Input.GetKeyDown(KeyCode.Backspace)) {
            RemoveLastMarker();
        }
        if (Input.GetKeyDown(KeyCode.Minus))
        {
            if (DrawOrigin != null) {
                if(RouteDisplayV2.RemoveRouteMarker(DrawOrigin.transform, false)) RemoveMarker(DrawOrigin);
            } 
        }
    }

    //Project a local marker to the small map
    private void ProjectMarker() {
        if (PlayerCamera.isActiveAndEnabled == true) {
            Ray MouseRay = PlayerCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit Hit;
            if (Physics.Raycast(MouseRay, out Hit)) {
                if (Hit.collider.tag == "Chunk" && Hit.collider.transform.parent.tag == "SpawnSmallMap") {
                    LocalProjectMarker.SetActive(true);
                    LocalProjectMarker.transform.position = Hit.point;
                } else {
                    LocalProjectMarker.SetActive(false);
                }
            }
        }
        if (PlayerTableViewCamera.isActiveAndEnabled == true) {
            Ray MouseRay = PlayerTableViewCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit Hit;
            if (Physics.Raycast(MouseRay, out Hit)) {
                if (Hit.collider.tag == "Chunk" && Hit.collider.transform.parent.tag == "SpawnSmallMap") {
                    LocalProjectMarker.SetActive(true);
                    LocalProjectMarker.transform.position = Hit.point;
                } else {
                    LocalProjectMarker.SetActive(false);
                }
            }
        }
    }


    private void SelectObjectByClick() {
        //Click Left mouse
        if (Input.GetMouseButtonDown(0)) {
            //If player in first persion view
            if (PlayerCamera.isActiveAndEnabled == true) {
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
                            DropdownOpionValue = "Marker";
                        }
                        else
                        {
                            DropdownOpionValue = "PlayerRouteMarker";
                        }

                        Vector3 CenterToMarker = (Hit.point - SmallMapCenter) * (LargeMapSize / SmallMapSize);
                        Vector3 NewPositionOnLargeMap = CenterToMarker + LargerMapCenter;

                        if (Input.GetKey(KeyCode.LeftShift)) ASL.ASLHelper.InstantiateASLObject(DropdownOpionValue, NewPositionOnLargeMap, Quaternion.identity, "", "", GetLargerMapMarker);
                        else if (DrawOrigin != null)
                        {
                            DrawOrigin.GetComponent<MeshRenderer>().material.color = OriginColor;
                            DrawOrigin = null;
                        }
                        //GenerateMarkerOnLargerMap(Hit.point);

                    }
                    //If mouse hit the large map
                    else if (Hit.collider.tag == "Chunk" && Hit.collider.transform.parent.tag == "SpawnLargerMap")
                    {
                        DropdownOpionValue = MyDropdownList.options[MyDropdownList.value].text;
                        if(Input.GetKey(KeyCode.LeftShift)) ASL.ASLHelper.InstantiateASLObject(DropdownOpionValue, Hit.point, Quaternion.identity, "", "", GetLargerMapMarker);
                        else if(DrawOrigin != null)
                        {
                            DrawOrigin.GetComponent<MeshRenderer>().material.color = OriginColor;
                            DrawOrigin = null;
                        }
                        //GenerateMarkerOnSmallMap(Hit.point);
                    }
                    else if (Hit.collider.gameObject.layer == 6 /*&& DrawLineMarker == null*/)  //If we don't hit either map but do hit a marker
                    {
                        if (DrawOrigin != null)
                        {
                            if (DrawOrigin.GetComponent<MeshRenderer>() != null) DrawOrigin.GetComponent<MeshRenderer>().material.color = OriginColor;
                        }

                        DrawOrigin = Hit.collider.gameObject;
                        OriginColor = (DrawOrigin.GetComponent<MeshRenderer>() != null) ? 
                            DrawOrigin.GetComponent<MeshRenderer>().material.color : Color.white;
                        if (DrawOrigin.GetComponent<MeshRenderer>() != null) DrawOrigin.GetComponent<MeshRenderer>().material.color = SelectedColor;
                        CreateDrawLineMarker(Hit.collider.gameObject);
                    }
                    else if(DrawOrigin != null)
                    {
                        if (DrawOrigin.GetComponent<MeshRenderer>() != null) DrawOrigin.GetComponent<MeshRenderer>().material.color = OriginColor;
                        DrawOrigin = null;
                    }
                }
            }
            //If player in third persion view
            if (PlayerTableViewCamera.isActiveAndEnabled == true) {
                Ray MouseRay = PlayerTableViewCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit Hit;
                if (Physics.Raycast(MouseRay, out Hit)) {
                    if (Hit.collider.tag == "Chunk" && Hit.collider.transform.parent.tag == "SpawnSmallMap") {
                        string DropdownOpionValue = MyDropdownList.options[MyDropdownList.value].text;
                        if (DropdownOpionValue == "Marker")
                        {
                            DropdownOpionValue = "Marker";
                        }
                        else
                        {
                            DropdownOpionValue = "PlayerRouteMarker";
                        }
                        //ASL.ASLHelper.InstantiateASLObject(DropdownOpionValue, Hit.point, Quaternion.identity, "", "", GetSmallMapMarker);*/
                        Vector3 CenterToMarker = (Hit.point - SmallMapCenter) * (LargeMapSize / SmallMapSize);
                        Vector3 NewPositionOnLargeMap = CenterToMarker + LargerMapCenter;

                        if (Input.GetKey(KeyCode.LeftShift)) ASL.ASLHelper.InstantiateASLObject(DropdownOpionValue, NewPositionOnLargeMap, Quaternion.identity, "", "", GetLargerMapMarker);
                        //GenerateMarkerOnLargerMap(Hit.point);
                    }
                    else if (Hit.collider.gameObject.layer == 6 /*&& DrawLineMarker == null*/)  //If we don't hit either map but do hit a marker
                    {
                        if(DrawOrigin != null)
                        {
                            if (DrawOrigin.GetComponent<MeshRenderer>() != null) DrawOrigin.GetComponent<MeshRenderer>().material.color = OriginColor;
                        }

                        DrawOrigin = Hit.collider.gameObject;
                        OriginColor = (DrawOrigin.GetComponent<MeshRenderer>() != null) ?
                            DrawOrigin.GetComponent<MeshRenderer>().material.color : Color.white;
                        if (DrawOrigin.GetComponent<MeshRenderer>() != null) DrawOrigin.GetComponent<MeshRenderer>().material.color = SelectedColor;
                        CreateDrawLineMarker(Hit.collider.gameObject);
                    }
                    else
                    {
                        if (DrawOrigin != null)
                        {
                            if (DrawOrigin.GetComponent<MeshRenderer>() != null) DrawOrigin.GetComponent<MeshRenderer>().material.color = OriginColor;
                        }
                    }
                }
            }
        }
    }

    private void CreateDrawLineMarker(GameObject _g)
    {
        DrawLineMarker = Instantiate(_g) as GameObject;
        if (DrawLineMarker.GetComponent<ASL.ASLObject>() != null) Destroy(DrawLineMarker.GetComponent<ASL.ASLObject>());

        Destroy(DrawLineMarker.GetComponent<ASL.ASLObject>());
        Destroy(DrawLineMarker.GetComponent<BoxCollider>());
        DrawLineMarker.layer = 11;
    }

    private void WhileClickDown()
    {
        if (Input.GetMouseButton(0))
        {
            drawTime -= Time.deltaTime;
            if (drawTime < 0f) drawTime = 0f;
            if (PlayerCamera.isActiveAndEnabled)
            {
                Ray mouseRay = PlayerCamera.ScreenPointToRay(Input.mousePosition);
                //RaycastHit hit;

                //int layerMask = (LargerMapMarkerList.IndexOf(DrawOrigin) >= 0) ? LayerMask.GetMask("Ground") : LayerMask.GetMask("Holomap");
                float thickness = (LargerMapMarkerList.IndexOf(DrawOrigin) >= 0) ? 0.25f : 0.01f;
                //Debug.Log(layerMask);
                if (drawTime <= 0f) DragDrawCast(mouseRay, LayerMask.GetMask("Ground"), thickness);
            }
            if (PlayerTableViewCamera.isActiveAndEnabled)
            {
                Ray mouseRay = PlayerTableViewCamera.ScreenPointToRay(Input.mousePosition);
                //int layerMask = LayerMask.GetMask("Holomap");
                if (drawTime <= 0f) DragDrawCast(mouseRay, LayerMask.GetMask("Ground"), 0.01f);
            }
        }
    }

    private void DragDrawCast(Ray mouseRay, int layerMask, float thickness)
    {
        RaycastHit hit;
        if (Physics.Raycast(mouseRay, out hit, 1000f, layerMask))
        {
            if (EventSystem.current.IsPointerOverGameObject(-1)) return;

            if (DrawLineMarker != null)
            {
                DrawLine.SetActive(true);
                DrawLineMarker.SetActive(true);
                DrawLineMarker.transform.position = hit.point;

                Vector3 line = (DrawLineMarker.transform.position - DrawOrigin.transform.position);
                DrawLine.transform.localScale = new Vector3(thickness, line.magnitude * 0.5f, thickness);
                DrawLine.transform.position = DrawOrigin.transform.position + 0.5f * line;
                DrawLine.transform.up = line;
            }
        }
        else
        {
            DrawLine.SetActive(false);
            if (DrawLineMarker != null) DrawLineMarker.SetActive(false);
        }
    }

    private void ClickRelease()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if(DrawLineMarker != null)
            {
                if (PlayerCamera.isActiveAndEnabled)
                {
                    Ray mouseRay = PlayerCamera.ScreenPointToRay(Input.mousePosition);

                    //int layerMask = (LargerMapMarkerList.IndexOf(DrawOrigin) >= 0) ? LayerMask.GetMask("Ground") : LayerMask.GetMask("Holomap");
                    if (drawTime <= 0f) DragDrawFinish(mouseRay, LayerMask.GetMask("Ground"));
                }
                if (PlayerTableViewCamera.isActiveAndEnabled)
                {
                    Ray mouseRay = PlayerTableViewCamera.ScreenPointToRay(Input.mousePosition);
                    //int layerMask = (LargerMapMarkerList.IndexOf(DrawOrigin) >= 0) ? LayerMask.GetMask("Ground") : LayerMask.GetMask("Holomap");
                    if (drawTime <= 0f) DragDrawFinish(mouseRay, LayerMask.GetMask("Ground"));
                }
                

                DrawLine.SetActive(false);
                Destroy(DrawLineMarker);
            }
            drawTime = 0.5f;
        }
    }

    private void DragDrawFinish(Ray mouseRay, int layerMask)
    {
        RaycastHit hit;

        if (Physics.Raycast(mouseRay, out hit, 1000f, layerMask))
        {
            if (EventSystem.current.IsPointerOverGameObject(-1)) return;

            string DropdownOpionValue = MyDropdownList.options[MyDropdownList.value].text;
            if (hit.collider.transform.parent.tag == "SpawnLargerMap")
            {
                ASL.ASLHelper.InstantiateASLObject(DropdownOpionValue, hit.point, Quaternion.identity, "", "", InsertLargerMapMarker);
            }
            else
            {
                Vector3 CenterToMarker = (hit.point - SmallMapCenter) * (LargeMapSize / SmallMapSize);
                Vector3 NewPositionOnLargeMap = CenterToMarker + LargerMapCenter;

                ASL.ASLHelper.InstantiateASLObject(DropdownOpionValue, NewPositionOnLargeMap, Quaternion.identity, "", "", InsertLargerMapMarker);
            }

        }
    }

    private void SpawnRegularMarker(Vector3 HitPoint)
    {

    }

    private static void GetSmallMapMarker(GameObject _myGameObject)
    {
        ASLObjectTrackingSystem.AddObjectToTrack(_myGameObject.GetComponent<ASL.ASLObject>());
        RouteDisplayV2.AddRouteMarker(_myGameObject.transform);
        SmallMapMarkerList.Add(_myGameObject);
    }

    //Add the large map marker into the list and add it into ASLObjectTrackingSystem
    private static void GetLargerMapMarker(GameObject _myGameObject) {
        ASLObjectTrackingSystem.AddObjectToTrack(_myGameObject.GetComponent<ASL.ASLObject>());
        //MiniMapDisplayObject.GetComponent<MinimapDisplay>().AddRouteMarker(_myGameObject.transform.position);
        RouteDisplayV2.AddRouteMarker(_myGameObject.transform);
        LargerMapMarkerList.Add(_myGameObject);
    }

    private static void InsertLargerMapMarker(GameObject _myGameObject)
    {
        ASLObjectTrackingSystem.AddObjectToTrack(_myGameObject.GetComponent<ASL.ASLObject>());
        int insertNdx = RouteDisplayV2.InsertMarkerAt(generator.DrawOrigin.transform, _myGameObject.transform);
        //how to grab corresponding marker from tracked markers?
        Debug.Log(insertNdx);
        if (insertNdx < 0)
        {
            LargerMapMarkerList.Add(_myGameObject);
        } else
        {
            LargerMapMarkerList.Insert(insertNdx + 1, _myGameObject);
        }
    }

    //Get position from small map and comvert is to larger map and generate a new marker on larger map
    private void GenerateMarkerOnLargerMap(Vector3 MarkerPosition) {
        //(MarkerPosition - SmallMapCenter) will get the math vector from smallmapcenter to marker
        Vector3 CenterToMarker = (MarkerPosition - SmallMapCenter) * (LargeMapSize / SmallMapSize);
        Vector3 NewPositionOnLargeMap = CenterToMarker + LargerMapCenter;

        //ASL.ASLHelper.InstantiateASLObject("Marker", NewPositionOnLargeMap, Quaternion.identity, "", "", GetLargerMapMarker);
    }

    //Get position from larger map and convert is to small map and generate a new marker on small map
    private void GenerateMarkerOnSmallMap(Vector3 MarkerPosition) {
        //(MarkerPosition - LargerMapCenter) will get the math vector from largermapcenter to marker
        Vector3 CenterToMarker = (MarkerPosition - LargerMapCenter) / (LargeMapSize / SmallMapSize);
        Vector3 NewPositionOnLargeMap = CenterToMarker + SmallMapCenter;
        ASL.ASLHelper.InstantiateASLObject("PlayerMarker", NewPositionOnLargeMap, Quaternion.identity, "", "", GetSmallMapMarker);
    }

    private void RemoveLastMarker()
    {
        //if (SmallMapMarkerList.Count > 0)
        //{
        //    GameObject SMarker = SmallMapMarkerList[SmallMapMarkerList.Count - 1];
        //    ASLObjectTrackingSystem.RemoveObjectToTrack(SMarker.GetComponent<ASL.ASLObject>());
        //    SMarker.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
        //    {
        //        SMarker.GetComponent<ASL.ASLObject>().DeleteObject();
        //    });

        //    SmallMapMarkerList.RemoveAt(SmallMapMarkerList.Count - 1);
        //}
        if (LargerMapMarkerList.Count > 0)
        {
            GameObject LMarker = LargerMapMarkerList[LargerMapMarkerList.Count - 1];
            if (RouteDisplayV2.RemoveRouteMarker(LMarker.transform, false)) RemoveMarker(LMarker);
            /*ASLObjectTrackingSystem.RemoveObjectToTrack(LMarker.GetComponent<ASL.ASLObject>());
            LMarker.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
            {
                LMarker.GetComponent<ASL.ASLObject>().DeleteObject();
            });

            LargerMapMarkerList.RemoveAt(LargerMapMarkerList.Count - 1);*/
        }
    }

    public static void RemoveMarker(GameObject _marker)
    {
        if (LargerMapMarkerList.Remove(_marker))
        {
            ASLObjectTrackingSystem.RemoveObjectToTrack(_marker.GetComponent<ASL.ASLObject>());
            _marker.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
            {
                _marker.GetComponent<ASL.ASLObject>().DeleteObject();
            });
        }
    }
}

