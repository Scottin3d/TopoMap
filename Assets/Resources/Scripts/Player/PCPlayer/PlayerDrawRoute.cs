using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ASL;

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
    private static List<GameObject> TempList = new List<GameObject>();
    private static Vector3 FirstBrushPosition = new Vector3();
    private static Vector3 SecondBrushPosition = new Vector3();
    private static int MyLargeBruchListIndex = 0;

    private static List<GameObject> MyLineRenenderBetweenBrushSmallMap = new List<GameObject>();
    private static Vector3 FirstBrushPositionSmallMap = new Vector3();
    private static Vector3 SecondBrushPositionSmallMap = new Vector3();
    private static int MySmallBrushListIndex = 0;

    private static Dictionary<string, List<Vector3>> OtherPlayerRoute = new Dictionary<string, List<Vector3>>();
    private static int SendMyRouteIndex = 0;

    public Dropdown MyEraseDropDown;
    private static GameObject ThisGameObject;
    public GameObject MyRouteHelper;

    public GameObject ThreeD_TextToDisplayRouteLengthOnSmallMap;
    private float TotalLength;

    // Start is called before the first frame update
    void Start()
    {
        ThisGameObject = this.gameObject;
        PlayerCamera = GameObject.Find("PCHandler/Player").GetComponentInChildren<Camera>();
        PlayerTableViewCamera = GameObject.Find("PCHandler/PlayerTopViewCamera").GetComponentInChildren<Camera>();
        StartCoroutine(DrawTheLineIE());
        StartCoroutine(LinkLastTwoRouteOnLargeMap());
        StartCoroutine(LinkLastTwoRouteOnSmallMap());
        StartCoroutine(SendMyBrushListToOther());
        MyRouteHelper.GetComponent<ASL.ASLObject>()._LocallySetFloatCallback(MyFloatFunction);

    }

    private void Update()
    {

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
                ThreeD_TextToDisplayRouteLengthOnSmallMap.GetComponent<TextMesh>().text = "";
                yield return null;
            }
            else if (MyLargeBruchListIndex == MyLargerBrushList.Count - 1)
            {
                CalculateTheNewLengthOfTheRoute();
                ThreeD_TextToDisplayRouteLengthOnSmallMap.GetComponent<TextMesh>().text = "Length: " + TotalLength + "m";
                ThreeD_TextToDisplayRouteLengthOnSmallMap.transform.position = MySmallBrushList[MySmallBrushList.Count / 2].transform.position;
                ThreeD_TextToDisplayRouteLengthOnSmallMap.transform.position += new Vector3(0, 0.5f, 0);
                yield return null;
            }
            else
            {
                yield return new WaitForSeconds(0.03f);
                int LargeBrushListCount = MyLargerBrushList.Count;
                for (int i = MyLargeBruchListIndex; i < LargeBrushListCount - 1; i++)
                {
                    FirstBrushPosition = new Vector3(MyLargerBrushList[i].transform.position.x, MyLargerBrushList[i].transform.position.y, MyLargerBrushList[i].transform.position.z);
                    SecondBrushPosition = new Vector3(MyLargerBrushList[i + 1].transform.position.x, MyLargerBrushList[i + 1].transform.position.y, MyLargerBrushList[i + 1].transform.position.z);

                    Vector3 CenterOfTwoBrush = (FirstBrushPosition + SecondBrushPosition) / 2;
                    float DistanceBetweenTwoBrush = Vector3.Distance(FirstBrushPosition, SecondBrushPosition);
                    int NumberOfExtraBrushNeeded = Mathf.RoundToInt(DistanceBetweenTwoBrush / 1f);

                    //Add several brush between two brush
                    for (int o = 1; o < NumberOfExtraBrushNeeded; o++)
                    {
                        float X = GetNewXYZ(FirstBrushPosition.x, SecondBrushPosition.x, DistanceBetweenTwoBrush, 1f, o);
                        float Y = GetNewXYZ(FirstBrushPosition.y, SecondBrushPosition.y, DistanceBetweenTwoBrush, 1f, o);
                        float Z = GetNewXYZ(FirstBrushPosition.z, SecondBrushPosition.z, DistanceBetweenTwoBrush, 1f, o);
                        //Spawm a little bit higher, so the large brush can reposition.
                        Vector3 NewPosition = new Vector3(X, Y + 50f, Z);

                        ASL.ASLHelper.InstantiateASLObject("LargeBrush", NewPosition, Quaternion.identity, "", "", SetLineRenender);

                        yield return new WaitForSeconds(0.01f);
                    }
                }
                //LargeBrushListCount is original length (mins 1 is the index, mins),
                //0 1 2
                //0 += 3 - 1 - 0 ==== 2
                //3 4 5 6
                //2 += 7 - 1 - 2 ==== 6
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
                yield return new WaitForSeconds(0.05f);
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
                        Vector3 NewPosition = new Vector3(X, Y, Z);

                        ASL.ASLHelper.InstantiateASLObject("Brush", NewPosition, Quaternion.identity, "", "", SetLineRenenderSmallMap);

                        yield return new WaitForSeconds(0.03f);
                    }

                    //yield return new WaitForSeconds(0.015f);
                    //ASL.ASLHelper.InstantiateASLObject("LineRenderBetweenToBrushSmallMap", new Vector3(0, 0, 0), Quaternion.identity, "", "", SetLineRenenderSmallMap);
                    //yield return new WaitForSeconds(0.2f);
                }

                MySmallBrushListIndex += SmallBrushListCount - 1 - MySmallBrushListIndex;
            }
        }
    }

    private void CalculateTheNewLengthOfTheRoute()
    {
        TotalLength = 0;

        for (int i = 0; i < MyLineRenenderBetweenBrush.Count - 1; i++)
        {
            float L = Vector3.Distance(MyLineRenenderBetweenBrush[i].transform.position, MyLineRenenderBetweenBrush[i + 1].transform.position);
            TotalLength += L;
        }
    }

    private float GetNewXYZ(float xyz1, float xyz2, float L, float M, int o)
    {
        float NewXYZ = xyz1 + (xyz2 - xyz1) / L * M * o;
        return NewXYZ;
    }

    //New Version
    private static void SetLineRenender(GameObject _myGameObject)
    {
        _myGameObject.transform.parent = ThisGameObject.transform;
        MyLineRenenderBetweenBrush.Add(_myGameObject);
    }

    //New Version
    private static void SetLineRenenderSmallMap(GameObject _myGameObject)
    {
        _myGameObject.transform.parent = ThisGameObject.transform;
        MyLineRenenderBetweenBrushSmallMap.Add(_myGameObject);
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
    }

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

        MyLargeBruchListIndex = 0;
        MySmallBrushListIndex = 0;

        SendMyRouteIndex = 0;
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

    public List<GameObject> GetMyLargerBrushList()
    {
        return MyLargerBrushList;
    }

    public List<GameObject> GetMyLineRenenderBetweenBrush()
    {
        return MyLineRenenderBetweenBrush;
    }

    public Dictionary<string, List<Vector3>> GetOtherPlayerRoute()
    {
        return OtherPlayerRoute;
    }

    IEnumerator SendMyBrushListToOther()
    {
        while (true)
        {
            //If nothing in the list, send a unique float array
            if (MyLineRenenderBetweenBrush.Count <= 1)
            {
                yield return new WaitForSeconds(3f);

                MyRouteHelper.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
                {
                    float[] MyFloatPathPosition = new float[4];
                    MyFloatPathPosition[0] = 0f;
                    MyFloatPathPosition[1] = 0f;
                    MyFloatPathPosition[2] = 10000f;

                    //Send ID to other player, they can use the ID to display the name.
                    MyFloatPathPosition[3] = GameLiftManager.GetInstance().m_PeerId;
                    MyRouteHelper.GetComponent<ASL.ASLObject>().SendFloatArray(MyFloatPathPosition);
                });
            }
            else
            {
                yield return new WaitForSeconds(5f);

                MyRouteHelper.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
                {
                    //Send a start message
                    Debug.Log("Send Start");
                    float[] StartMessage = new float[4];
                    StartMessage[0] = 0f;
                    StartMessage[1] = 0f;
                    StartMessage[2] = 12000f;

                    StartMessage[3] = GameLiftManager.GetInstance().m_PeerId;
                    MyRouteHelper.GetComponent<ASL.ASLObject>().SendFloatArray(StartMessage);
                });

                MyRouteHelper.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
                {
                    foreach (GameObject B in MyLineRenenderBetweenBrush)
                    {
                        Debug.Log("Send");
                        float[] MyFloatPathPosition = new float[4];
                        MyFloatPathPosition[0] = B.transform.position.x;
                        MyFloatPathPosition[1] = B.transform.position.y;
                        MyFloatPathPosition[2] = B.transform.position.z;

                        //Send ID to other player, they can use the ID to display the name.
                        MyFloatPathPosition[3] = GameLiftManager.GetInstance().m_PeerId;
                        MyRouteHelper.GetComponent<ASL.ASLObject>().SendFloatArray(MyFloatPathPosition);

                        //FloatIndex++;
                    }
                });
            }
        }
    }

    public static void MyFloatFunction(string _id, float[] _myFloats)
    {
        //If _myFloats contain a new m_PeerId (Player will always send a 0,0,10000 message first)
        if (!OtherPlayerRoute.ContainsKey(GameLiftManager.GetInstance().m_Players[(int)_myFloats[3]]))
        {
            //If other players send a empty list and it's a new user, add a empty List<Vector3> for that user.
            if (_myFloats[0] == 0 && _myFloats[1] == 0 && _myFloats[2] == 10000)
            {
                List<Vector3> RouteListFromOther = new List<Vector3>();
                OtherPlayerRoute.Add(GameLiftManager.GetInstance().m_Players[(int)_myFloats[3]], RouteListFromOther);
            }
            //else
            //{
            //    Vector3 NewPositionFromOther = new Vector3();
            //    NewPositionFromOther.x = _myFloats[0];
            //    NewPositionFromOther.y = _myFloats[1];
            //    NewPositionFromOther.z = _myFloats[2];

            //    List<Vector3> RouteListFromOther = new List<Vector3>();
            //    RouteListFromOther.Add(NewPositionFromOther);

            //    OtherPlayerRoute.Add(GameLiftManager.GetInstance().m_Players[(int)_myFloats[3]], RouteListFromOther);
            //}
        }
        else
        {
            //If player send a clear list message, replace the old list to a new empty List<Vector3> for that user.
            //0,0,10000 means player clear their route. 0,0,12000 means player gonna resend the entire list to me.
            if (_myFloats[0] == 0f && _myFloats[1] == 0f && _myFloats[2] == 10000f || _myFloats[0] == 0F && _myFloats[1] == 0F && _myFloats[2] == 12000f)
            //if (_myFloats[0] == 0f && _myFloats[1] == 0f && _myFloats[2] == 10000f)
            {
                List<Vector3> RouteListFromOther = new List<Vector3>();
                OtherPlayerRoute[GameLiftManager.GetInstance().m_Players[(int)_myFloats[3]]] = RouteListFromOther;
            }
            else
            {
                Vector3 NewPositionFromOther = new Vector3();
                NewPositionFromOther.x = _myFloats[0];
                NewPositionFromOther.y = _myFloats[1];
                NewPositionFromOther.z = _myFloats[2];

                OtherPlayerRoute[GameLiftManager.GetInstance().m_Players[(int)_myFloats[3]]].Add(NewPositionFromOther);
            }
        }
    }
}


/*
//After this for loop, temp list contain all the new brush between two point.
//Add temp list into CombineList, so we know the new brush list that between point A and B is here.
//Then clear templist
List<GameObject> TempListCopy = TempList.ConvertAll(G => new GameObject());

//foreach (GameObject A in TempList)
//{
//    TempListCopy.Add(A);
//}

ExtraListBetweenOriginBrush.Add(TempListCopy);
TempList.Clear();
yield return new WaitForSeconds(0.05f);
*/

/*
////After this for loop, MyLargerBrushList still contain the original brush. Combine contain the new list between each original brush.
////Now combine the ExtraListBetweenOriginBrush into the MyLargerBrushList
////0 1 2 3 4
//// 5 2 3 4
//int BrushListCount = MyLargerBrushList.Count - 1;
//int ExtraListIndex = 0;
//int ExtraLength = 0;
//for (int i = MyLargeBruchListIndex; i < BrushListCount; i++)
//{
//    List<GameObject> T = ExtraListBetweenOriginBrush[ExtraListIndex];
//    ExtraListIndex++;
//    ExtraLength += T.Count;
//    Debug.Log(T.Count);

//    for (int o = 0; o < T.Count - 1; o++)
//    {
//        MyLargerBrushList.Insert(o + i + 1, T[o]);
//    }

//    BrushListCount += T.Count;
//    i += T.Count;
//    //"0" 1 2 3 4 5 "1" 1 2 "2" 1 2 3 "3" 1 2 3 4 "4"
//}

////LargeBrushListCount is original length (mins 1 is the index, mins),
////0 1 2
//// 5 2
////"0" 1 2 3 4 5 "1" 1 2 "2"
////0 += 3 - 1 - 0 + 7  + 1 ==== 10
//MyLargeBruchListIndex += LargeBrushListCount - 1 - MyLargeBruchListIndex + ExtraLength + 1;
//ExtraListBetweenOriginBrush.Clear();
*/
