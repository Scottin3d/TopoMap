using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VRPlayerDrawRoute : MonoBehaviour
{
    //=========
    //LargerMapGenerator BigMap(Under PCPlane)
    //SmallMapGenerator is Small(Under VRPlane)
    public GameObject LargerMapGenerator;
    public GameObject SmallMapGenerator;
    private Camera PlayerCamera;
    private Camera PlayerTableViewCamera;
    private static List<GameObject> MyBrushList = new List<GameObject>();
    private static List<GameObject> MySmallBrushList = new List<GameObject>();
    private static List<GameObject> MyLargerBrushList = new List<GameObject>();
    //==============
    //This Dropdown contain two optitions which are EraseAll and EraseLastTen
    //You can find this element under PCHandler/PC_Player_Canvas/EraseDropdown
    public Dropdown MyEraseDropDown;
    private static GameObject ThisGameObject;
    //Awake function
    void Awake()
    {
        ThisGameObject = this.gameObject;
        //==========
        //Need to find the VR player camera
        //PlayerCamera = GameObject.Find("PCHandler/Player").GetComponentInChildren<Camera>();

        //===============
        //If you want let VR Player have the top-view, add a extra fixed TableCamera(Transform.position just above table) for VR player
        //PlayerTableViewCamera = GameObject.Find("PCHandler/PlayerTopViewCamera").GetComponentInChildren<Camera>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        DrawTheLine();
    }

    private void DrawTheLine()
    {
        //====================
        //Get your VR button input
        if (Input.GetMouseButton(1))
        {
            if (PlayerCamera.isActiveAndEnabled == true)
            {
                //==========================
                //You need some thing similar with the RaycastHit to pass a position
                //If you use some spical way to get the Hit.point. You have to replace all the Hit.point down here.
                Ray MouseRay = PlayerCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit Hit;
                if (Physics.Raycast(MouseRay, out Hit))
                {
                    //======================
                    //Players are able to draw on the whiteboard and on map chunk.
                    //You need some statement to figure out the tag.
                    if (Hit.collider.tag == "WhiteBoard")
                    {
                        ASL.ASLHelper.InstantiateASLObject("Brush", Hit.point, Quaternion.identity, "", "", GetEachBrushOnWhiteBoard);
                    }
                    if (Hit.collider.tag == "Chunk" && Hit.collider.transform.parent.tag == "SpawnSmallMap")
                    {
                        ASL.ASLHelper.InstantiateASLObject("Brush", Hit.point, Quaternion.identity, "", "", GetEachBrushOnSmallMap);
                        int LargeMapSize = LargerMapGenerator.GetComponent<GenerateMapFromHeightMap>().mapSize;
                        int SmallMapSize = SmallMapGenerator.GetComponent<GenerateMapFromHeightMap>().mapSize;
                        Vector3 NewPositionOneLargeMap = ((Hit.point - SmallMapGenerator.transform.position) * (LargeMapSize / SmallMapSize)) + LargerMapGenerator.transform.position;
                        ASL.ASLHelper.InstantiateASLObject("LargeBrush", NewPositionOneLargeMap, Quaternion.identity, "", "", GetEachBrushOnLargerMap);
                    }
                }
            }

            //===========
            //If VR player don't need this Table View Camera. You can delete this if statement.
            if (PlayerTableViewCamera.isActiveAndEnabled == true)
            {
                Ray MouseRay = PlayerTableViewCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit Hit;
                if (Physics.Raycast(MouseRay, out Hit))
                {
                    if (Hit.collider.tag == "Chunk")
                    {
                        ASL.ASLHelper.InstantiateASLObject("Brush", Hit.point, Quaternion.identity, "", "", GetEachBrushOnSmallMap);
                        int LargeMapSize = LargerMapGenerator.GetComponent<GenerateMapFromHeightMap>().mapSize;
                        int SmallMapSize = SmallMapGenerator.GetComponent<GenerateMapFromHeightMap>().mapSize;
                        Vector3 NewPositionOneLargeMap = ((Hit.point - SmallMapGenerator.transform.position) * (LargeMapSize / SmallMapSize)) + LargerMapGenerator.transform.position;
                        ASL.ASLHelper.InstantiateASLObject("LargeBrush", NewPositionOneLargeMap, Quaternion.identity, "", "", GetEachBrushOnLargerMap);
                    }
                }
            }
        }
    }

    private static void GetEachBrushOnWhiteBoard(GameObject _myGameObject)
    {
        _myGameObject.transform.parent = ThisGameObject.transform;
        MyBrushList.Add(_myGameObject);
    }

    private static void GetEachBrushOnSmallMap(GameObject _myGameObject)
    {
        _myGameObject.transform.parent = ThisGameObject.transform;
        MySmallBrushList.Add(_myGameObject);
    }

    private static void GetEachBrushOnLargerMap(GameObject _myGameObject)
    {
        _myGameObject.transform.parent = ThisGameObject.transform;
        MyLargerBrushList.Add(_myGameObject);
    }

    //==================
    //PCHandler/PC_Player_Canvas/EraseButton will call EraseLine.
    //You may need to user VR players UI button.
    public void EraseLine()
    {
        if (MyEraseDropDown.options[MyEraseDropDown.value].text == "EraseAll")
        {
            EraseAllLine();
        }
        if (MyEraseDropDown.options[MyEraseDropDown.value].text == "EraseLastTen")
        {
            EraseLastTenLine();
        }
    }

    private static void EraseAllLine()
    {
        foreach (GameObject SingleBrush in MyBrushList)
        {
            SingleBrush.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
            {
                SingleBrush.GetComponent<ASL.ASLObject>().DeleteObject();
            });
        }

        MyBrushList.Clear();
    }

    private static void EraseLastTenLine()
    {
        for (int i = 0; i < 10; i++)
        {
            if (MyBrushList.Count != 0)
            {
                GameObject LastBrush = MyBrushList[MyBrushList.Count - 1];

                LastBrush.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
                {
                    LastBrush.GetComponent<ASL.ASLObject>().DeleteObject();
                });

                MyBrushList.RemoveAt(MyBrushList.Count - 1);
            }
        }
    }

    //==================
    //PCHandler/PC_Player_Canvas/EraseMapButton will call EraseLineOnMap.
    //You may need to user VR players UI button.
    public void EraseLineOnMap()
    {
        if (MyEraseDropDown.options[MyEraseDropDown.value].text == "EraseAll")
        {
            EraseAllLineOnMap();
        }
        if (MyEraseDropDown.options[MyEraseDropDown.value].text == "EraseLastTen")
        {
            EraseLastTenLineOnMap();
        }
    }

    private static void EraseAllLineOnMap()
    {
        foreach (GameObject SingleBrush in MySmallBrushList)
        {
            SingleBrush.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
            {
                SingleBrush.GetComponent<ASL.ASLObject>().DeleteObject();
            });
        }

        foreach (GameObject SingleBrush in MyLargerBrushList)
        {
            SingleBrush.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
            {
                SingleBrush.GetComponent<ASL.ASLObject>().DeleteObject();
            });
        }

        MySmallBrushList.Clear();
        MyLargerBrushList.Clear();
    }

    private static void EraseLastTenLineOnMap()
    {
        for (int i = 0; i < 10; i++)
        {
            if (MySmallBrushList.Count != 0)
            {
                GameObject LastBrushOnSmallMap = MySmallBrushList[MyBrushList.Count - 1];
                GameObject LastBrushOnLargeMap = MyLargerBrushList[MyBrushList.Count - 1];

                LastBrushOnSmallMap.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
                {
                    LastBrushOnSmallMap.GetComponent<ASL.ASLObject>().DeleteObject();
                });

                LastBrushOnLargeMap.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
                {
                    LastBrushOnLargeMap.GetComponent<ASL.ASLObject>().DeleteObject();
                });

                MySmallBrushList.RemoveAt(MySmallBrushList.Count - 1);
                MyLargerBrushList.RemoveAt(MyLargerBrushList.Count - 1);
            }
        }
    }
}
