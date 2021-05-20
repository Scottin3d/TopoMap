using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDrawRoute : MonoBehaviour
{
    public GameObject LargerMapGenerator;
    public GameObject SmallMapGenerator;
    private Camera PlayerCamera;
    private Camera PlayerTableViewCamera;
    private static List<GameObject> MyBrushList = new List<GameObject>();
    private static List<GameObject> MySmallBrushList = new List<GameObject>();
    private static List<GameObject> MyLargerBrushList = new List<GameObject>();

    private static List<GameObject> MyLineRenenderBetweenBrush = new List<GameObject>();
    private static Vector3 FirstBrushPosition = new Vector3();
    private static Vector3 SecondBrushPosition = new Vector3();
    private int MyLargeBruchListIndex = 0;

    private static List<GameObject> MyLineRenenderBetweenBrushSmallMap = new List<GameObject>();
    private static Vector3 FirstBrushPositionSmallMap = new Vector3();
    private static Vector3 SecondBrushPositionSmallMap = new Vector3();
    private int MySmallBrushListIndex = 0;

    public Dropdown MyEraseDropDown;
    private static GameObject ThisGameObject;

    private Dictionary<string, Vector3> OtherPlayerList = new Dictionary<string, Vector3>();

    // Start is called before the first frame update
    void Start()
    {
        ThisGameObject = this.gameObject;
        PlayerCamera = GameObject.Find("PCHandler/Player").GetComponentInChildren<Camera>();
        PlayerTableViewCamera = GameObject.Find("PCHandler/PlayerTopViewCamera").GetComponentInChildren<Camera>();
        StartCoroutine(DrawTheLineIE());
        StartCoroutine(LinkLastTwoRouteOnLargeMap());
        StartCoroutine(LinkLastTwoRouteOnSmallMap());
    }

    IEnumerator DrawTheLineIE()
    {
        while (true)
        {
            if (Input.GetMouseButton(1))
            {
                if (PlayerCamera.isActiveAndEnabled == true)
                {
                    Ray MouseRay = PlayerCamera.ScreenPointToRay(Input.mousePosition);
                    RaycastHit Hit;
                    if (Physics.Raycast(MouseRay, out Hit))
                    {
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
                            NewPositionOneLargeMap.y += 3f;
                            ASL.ASLHelper.InstantiateASLObject("LargeBrush", NewPositionOneLargeMap, Quaternion.identity, "", "", GetEachBrushOnLargerMap);
                        }
                    }
                }

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
                            NewPositionOneLargeMap.y += 3f;
                            ASL.ASLHelper.InstantiateASLObject("LargeBrush", NewPositionOneLargeMap, Quaternion.identity, "", "", GetEachBrushOnLargerMap);
                        }
                    }
                }
            }
            yield return new WaitForSeconds(0.05f);
        }
    }

    IEnumerator LinkLastTwoRouteOnLargeMap()
    {
        while (true)
        {
            if (MyLargerBrushList.Count <= 1)
            {
                yield return null;
            }
            else if (MyLargeBruchListIndex == MyLargerBrushList.Count - 1)
            {
                yield return null;
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
                //01234567
                int LargeBrushListCount = MyLargerBrushList.Count;
                for (int i = MyLargeBruchListIndex; i < LargeBrushListCount - 1; i++)
                {
                    FirstBrushPosition = new Vector3(MyLargerBrushList[i].transform.position.x, MyLargerBrushList[i].transform.position.y, MyLargerBrushList[i].transform.position.z);
                    SecondBrushPosition = new Vector3(MyLargerBrushList[i + 1].transform.position.x, MyLargerBrushList[i + 1].transform.position.y, MyLargerBrushList[i + 1].transform.position.z);
                    //FirstBrushPosition.y += 2.5f;
                    //SecondBrushPosition.y += 2.5f;

                    Vector3 CenterOfTwoBrush = (FirstBrushPosition + SecondBrushPosition) / 2;
                    float DistanceBetweenTwoBrush = Vector3.Distance(FirstBrushPosition, SecondBrushPosition);
                    int NumberOfExtraBrushNeeded = Mathf.RoundToInt(DistanceBetweenTwoBrush / 1f);

                    //Add several brush between two brush
                    for (int o = 1; o < NumberOfExtraBrushNeeded; o++)
                    {
                        float X = GetNewXYZ(FirstBrushPosition.x, SecondBrushPosition.x, DistanceBetweenTwoBrush, 1f, o);
                        float Y = GetNewXYZ(FirstBrushPosition.y, SecondBrushPosition.y, DistanceBetweenTwoBrush, 1f, o);
                        float Z = GetNewXYZ(FirstBrushPosition.z, SecondBrushPosition.z, DistanceBetweenTwoBrush, 1f, o);
                        Vector3 NewPosition = new Vector3(X, Y, Z);

                        ASL.ASLHelper.InstantiateASLObject("LargeBrush", NewPosition, Quaternion.identity, "", "", SetLineRenender);

                        yield return new WaitForSeconds(0.02f);
                    }

                    //ASL.ASLHelper.InstantiateASLObject("LineRendererBetweenTwoBrush", new Vector3(0, 0, 0), Quaternion.identity, "", "", SetLineRenender);
                    //yield return new WaitForSeconds(0.2f);
                }

                MyLargeBruchListIndex += LargeBrushListCount - 1 - MyLargeBruchListIndex;
            }
        }
    }

    IEnumerator LinkLastTwoRouteOnSmallMap()
    {
        while (true)
        {
            if (MySmallBrushList.Count <= 1)
            {
                yield return null;
            }
            else if (MySmallBrushListIndex == MySmallBrushList.Count - 1)
            {
                yield return null;
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
                int SmallBrushListCount = MySmallBrushList.Count;
                for (int i = MySmallBrushListIndex; i < SmallBrushListCount - 1; i++)
                {
                    FirstBrushPositionSmallMap = new Vector3(MySmallBrushList[i].transform.position.x, MySmallBrushList[i].transform.position.y, MySmallBrushList[i].transform.position.z);
                    SecondBrushPositionSmallMap = new Vector3(MySmallBrushList[i + 1].transform.position.x, MySmallBrushList[i + 1].transform.position.y, MySmallBrushList[i + 1].transform.position.z);

                    Vector3 CenterOfTwoBrush = (FirstBrushPositionSmallMap + SecondBrushPositionSmallMap) / 2;
                    float DistanceBetweenTwoBrush = Vector3.Distance(FirstBrushPositionSmallMap, SecondBrushPositionSmallMap);
                    int NumberOfExtraBrushNeeded = Mathf.RoundToInt(DistanceBetweenTwoBrush / 0.015f);

                    //Add several brush between two brush
                    for (int o = 1; o < NumberOfExtraBrushNeeded; o++)
                    {
                        float X = GetNewXYZ(FirstBrushPositionSmallMap.x, SecondBrushPositionSmallMap.x, DistanceBetweenTwoBrush, 0.015f, o);
                        float Y = GetNewXYZ(FirstBrushPositionSmallMap.y, SecondBrushPositionSmallMap.y, DistanceBetweenTwoBrush, 0.015f, o);
                        float Z = GetNewXYZ(FirstBrushPositionSmallMap.z, SecondBrushPositionSmallMap.z, DistanceBetweenTwoBrush, 0.015f, o);
                        Vector3 NewPosition = new Vector3 (X, Y, Z);

                        ASL.ASLHelper.InstantiateASLObject("Brush", NewPosition, Quaternion.identity, "", "", SetLineRenenderSmallMap);

                        yield return new WaitForSeconds(0.01f);
                    }

                    yield return new WaitForSeconds(0.015f);
                    //ASL.ASLHelper.InstantiateASLObject("LineRenderBetweenToBrushSmallMap", new Vector3(0, 0, 0), Quaternion.identity, "", "", SetLineRenenderSmallMap);
                    //yield return new WaitForSeconds(0.2f);
                }

                MySmallBrushListIndex += SmallBrushListCount - 1 - MySmallBrushListIndex;
            }
        }
    }

    private float GetNewXYZ(float xyz1, float xyz2, float L, float M, int o)
    {
        float NewXYZ = xyz1 + (xyz2 - xyz1) / L * M * o;
        return NewXYZ;
    }

    //private static void SetLineRenender(GameObject _myGameObject)
    //{
    //    _myGameObject.transform.parent = ThisGameObject.transform;
    //    _myGameObject.GetComponent<LineRenderer>().SetPosition(0, FirstBrushPosition);
    //    _myGameObject.GetComponent<LineRenderer>().SetPosition(1, SecondBrushPosition);
    //    MyLineRenenderBetweenBrush.Add(_myGameObject);
    //}

    //New Version
    private static void SetLineRenender(GameObject _myGameObject)
    {
        _myGameObject.transform.parent = ThisGameObject.transform;
        MyLineRenenderBetweenBrush.Add(_myGameObject);
    }

    //private static void SetLineRenenderSmallMap(GameObject _myGameObject)
    //{
    //    _myGameObject.transform.parent = ThisGameObject.transform;
    //    _myGameObject.GetComponent<LineRenderer>().SetPosition(0, FirstBrushPositionSmallMap);
    //    _myGameObject.GetComponent<LineRenderer>().SetPosition(1, SecondBrushPositionSmallMap);
    //    MyLineRenenderBetweenBrushSmallMap.Add(_myGameObject);
    //}

    //New Version
    private static void SetLineRenenderSmallMap(GameObject _myGameObject)
    {
        _myGameObject.transform.parent = ThisGameObject.transform;
        MyLineRenenderBetweenBrushSmallMap.Add(_myGameObject);
    }

    private void DrawLineRendererBetweenTwoBrush()
    {
        LineRenderer MyLine = new GameObject("Line").AddComponent<LineRenderer>();
        MyLine.startColor = Color.yellow;
        MyLine.endColor = Color.yellow;
        MyLine.startWidth = 0.01f;
        MyLine.endWidth = 0.01f;
        MyLine.positionCount = 2;
        MyLine.useWorldSpace = true;
    }

    private static void GetEachBrushOnWhiteBoard(GameObject _myGameObject)
    {
        _myGameObject.transform.parent = ThisGameObject.transform;
        MyBrushList.Add(_myGameObject);
    }

    private static void GetEachBrushOnSmallMap(GameObject _myGameObject)
    {
        Debug.Log("Add Brush");
        _myGameObject.transform.parent = ThisGameObject.transform;
        MySmallBrushList.Add(_myGameObject);
    }

    private static void GetEachBrushOnLargerMap(GameObject _myGameObject)
    {
        _myGameObject.transform.parent = ThisGameObject.transform;
        MyLargerBrushList.Add(_myGameObject);
        //GenerateExtraLineOnMap(_myGameObject);
    }

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

        foreach (GameObject LineRenender in MyLineRenenderBetweenBrush)
        {
            LineRenender.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
            {
                LineRenender.GetComponent<ASL.ASLObject>().DeleteObject();
            });
        }

        foreach (GameObject LineRenender in MyLineRenenderBetweenBrushSmallMap)
        {
            LineRenender.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
            {
                LineRenender.GetComponent<ASL.ASLObject>().DeleteObject();
            });
        }

        MySmallBrushList.Clear();
        MyLargerBrushList.Clear();
        MyLineRenenderBetweenBrush.Clear();
        MyLineRenenderBetweenBrushSmallMap.Clear();
    }

    private static void EraseLastTenLineOnMap()
    {
        for (int i = 0; i < 10; i++)
        {
            if (MySmallBrushList.Count != 0)
            {
                GameObject LastBrushOnSmallMap = MySmallBrushList[MySmallBrushList.Count - 1];
                GameObject LastBrushOnLargeMap = MyLargerBrushList[MyLargerBrushList.Count - 1];

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

        for (int i = 0; i < 9; i++)
        {
            if (MyLineRenenderBetweenBrush.Count != 0)
            {
                GameObject LastLineRenender = MyLineRenenderBetweenBrush[MyLineRenenderBetweenBrush.Count - 1];

                LastLineRenender.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
                {
                    LastLineRenender.GetComponent<ASL.ASLObject>().DeleteObject();
                });

                MyLineRenenderBetweenBrush.RemoveAt(MyLineRenenderBetweenBrush.Count - 1);
            }

            if (MyLineRenenderBetweenBrushSmallMap.Count != 0)
            {
                GameObject LastLineRenender = MyLineRenenderBetweenBrushSmallMap[MyLineRenenderBetweenBrushSmallMap.Count - 1];

                LastLineRenender.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
                {
                    LastLineRenender.GetComponent<ASL.ASLObject>().DeleteObject();
                });

                MyLineRenenderBetweenBrushSmallMap.RemoveAt(MyLineRenenderBetweenBrushSmallMap.Count - 1);
            }
        }
    }

    IEnumerator SendMyBrushListToOther()
    {
        yield return new WaitForSeconds(10f);
    }

    public List<GameObject> GetMyLargerBrushList()
    {
        return MyLargerBrushList;
    }

    public Dictionary<string, Vector3> GetOtherPlayersList()
    {
        return OtherPlayerList;
    }
}
