using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ASL;

public class FollowTheDrawingRouteOnLargeMap : MonoBehaviour
{
    public GameObject Player;
    public GameObject TeleportManger;
    public Text SpeedDistanceDisplay;
    public Dropdown MyPlayersRoute;

    private float MoveSpeed = 1f;

    private List<GameObject> MyEntireBrushList = new List<GameObject>();
    private List<Vector3> OtherPlayerBrushList = new List<Vector3>();

    private bool StartFollowingRoute = false;
    private int BrushListLength;
    private int CurIndex;
    private Vector3 FirstBrush;
    private Vector3 SecondBrush;
    private float TheTotalLengthOfTheRoute;

    private bool FollowMyRoute = false;
    private bool FollowOtherRoute = false;

    void Start()
    {
        MyPlayersRoute.options.Clear();

        foreach (var Thing in GameLiftManager.GetInstance().m_Players)
        {
            if (Thing.Value == GameLiftManager.GetInstance().m_Username)
            {
                MyPlayersRoute.options.Add(new Dropdown.OptionData("My Own Route"));
            }
            else
            {
                MyPlayersRoute.options.Add(new Dropdown.OptionData(Thing.Value));
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!TeleportManger.GetComponent<TeleportBetweenMap>().GetAtSmallMap())
        {
            StartFollowTheRoute();
            IncreaseMoveSpeed();
        }
        else
        {
            SpeedDistanceDisplay.enabled = false;
        }
    }

    private void StartFollowTheRoute()
    {
        if (Input.GetKeyDown(KeyCode.Alpha3) && StartFollowingRoute == false)
        {
            GetRouteList();
            SpeedDistanceDisplay.enabled = true;

            Debug.Log(BrushListLength);

            //If the brushlist has more than 1 brush, we can start following the route. otherwise stop following
            if (BrushListLength >= 2)
            {
                StartFollowingRoute = true;
                Player.GetComponent<CharacterController>().enabled = false;
                Player.GetComponent<CharacterController>().detectCollisions = false;
                Player.transform.position = FirstBrush;
            }
        }

        if (StartFollowingRoute == true && CurIndex < BrushListLength)
        {
            Player.GetComponent<CharacterController>().enabled = false;
            Player.GetComponent<CharacterController>().detectCollisions = false;
            float Step = MoveSpeed * Time.deltaTime;
            Player.transform.position = Vector3.MoveTowards(Player.transform.position, SecondBrush, Step);
            DisplayCurSpeedDistance();

            if (Vector3.Distance(Player.transform.position, SecondBrush) < 0.005f)
            {
                if (CurIndex + 1 != BrushListLength)
                {
                    MoveToNextTwoBrush();
                }
                else
                {
                    StartFollowingRoute = false;
                    Player.GetComponent<CharacterController>().enabled = true;
                    Player.GetComponent<CharacterController>().detectCollisions = true;
                }
            }
        }
    }

    private void GetRouteList()
    {
        if (MyPlayersRoute.options[MyPlayersRoute.value].text == "My Own Route")
        {
            CurIndex = 0;
            TheTotalLengthOfTheRoute = 0;

            //Get my route
            MyEntireBrushList = this.GetComponent<PlayerDrawRoute>().GetMyLineRenenderBetweenBrush();
            BrushListLength = MyEntireBrushList.Count;

            if (BrushListLength >= 2)
            {
                //Get First and Second brush here
                FirstBrush = MyEntireBrushList[CurIndex].transform.position;
                SecondBrush = MyEntireBrushList[CurIndex + 1].transform.position;

                FirstBrush.y += 3.5f;
                SecondBrush.y += 3.5f;

                CurIndex++;
            }

            TotalLengthOfTheRoute();

            FollowMyRoute = true;
            FollowOtherRoute = false;
        }
        else
        {
            CurIndex = 0;
            TheTotalLengthOfTheRoute = 0;

            //Get Other's Route based on the drop down value
            //MyPlayersRoute.options[MyPlayersRoute.value].text is the drop down value
            OtherPlayerBrushList = this.GetComponent<PlayerDrawRoute>().GetOtherPlayerRoute()[MyPlayersRoute.options[MyPlayersRoute.value].text];

            BrushListLength = OtherPlayerBrushList.Count;

            if (BrushListLength >= 2)
            {
                //Get First and Second brush here
                FirstBrush = OtherPlayerBrushList[CurIndex];
                SecondBrush = OtherPlayerBrushList[CurIndex + 1];

                FirstBrush.y += 3.5f;
                SecondBrush.y += 3.5f;

                CurIndex++;
            }

            TotalLengthOfTheRouteOtherPlayer();

            FollowMyRoute = false;
            FollowOtherRoute = true;
        }
    }

    private void DisplayCurSpeedDistance()
    {
        float RemainLength = PartLengthOfTheRoute(CurIndex);
        float RemainTime = RemainLength / MoveSpeed;
        string Info = "Speed: " + MoveSpeed + "m/s" + "\r\n" + "Remaining Distance: " + RemainLength + "m" + "\r\n" + "Remaining Time: " + RemainTime + "s";

        SpeedDistanceDisplay.GetComponent<Text>().text = Info;
    }

    private void TotalLengthOfTheRoute()
    {
        //0 1 2 3 4 5
        float ListL = MyEntireBrushList.Count - 1;
        for (int i = 0; i < ListL; i++)
        {
            TheTotalLengthOfTheRoute += Vector3.Distance(MyEntireBrushList[i].transform.position, MyEntireBrushList[i + 1].transform.position);
        }
    }

    private void TotalLengthOfTheRouteOtherPlayer()
    {
        //0 1 2 3 4 5
        float ListL = OtherPlayerBrushList.Count - 1;
        for (int i = 0; i < ListL; i++)
        {
            TheTotalLengthOfTheRoute += Vector3.Distance(OtherPlayerBrushList[i], OtherPlayerBrushList[i + 1]);
        }
    }

    private float PartLengthOfTheRoute(int CurIndexPara)
    {
        if (MyPlayersRoute.options[MyPlayersRoute.value].text == "My Own Route")
        {
            float ListL = MyEntireBrushList.Count - 1;
            float RemainL = 0;
            for (; CurIndexPara < ListL; CurIndexPara++)
            {
                RemainL += Vector3.Distance(MyEntireBrushList[CurIndexPara].transform.position, MyEntireBrushList[CurIndexPara + 1].transform.position);
            }

            return RemainL;
        }
        else
        {
            float ListL = OtherPlayerBrushList.Count - 1;
            float RemainL = 0;
            for (; CurIndexPara < ListL; CurIndexPara++)
            {
                RemainL += Vector3.Distance(OtherPlayerBrushList[CurIndexPara], OtherPlayerBrushList[CurIndexPara + 1]);
            }

            return RemainL;
        }
    }

    private void MoveToNextTwoBrush()
    {
        //if (MyPlayersRoute.options[MyPlayersRoute.value].text == "My Own Route")
        if (FollowMyRoute)
        {
            FirstBrush = MyEntireBrushList[CurIndex].transform.position;
            SecondBrush = MyEntireBrushList[CurIndex + 1].transform.position;

            FirstBrush.y += 3.5f;
            SecondBrush.y += 3.5f;

            Player.transform.position = FirstBrush;
            CurIndex++;
        }
        else
        {
            FirstBrush = OtherPlayerBrushList[CurIndex];
            SecondBrush = OtherPlayerBrushList[CurIndex + 1];

            FirstBrush.y += 3.5f;
            SecondBrush.y += 3.5f;

            Player.transform.position = FirstBrush;
            CurIndex++;
        }
    }

    private void IncreaseMoveSpeed()
    {
        if (StartFollowingRoute == true && Input.GetKeyDown(KeyCode.W))
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                MoveSpeed += 10f;
            }
            else
            {
                MoveSpeed += 1f;
            }   
        }

        if (StartFollowingRoute == true && Input.GetKeyDown(KeyCode.S))
        {
            if (MoveSpeed >= 1f)
            {
                MoveSpeed -= 1f;
            }
        }

        if (StartFollowingRoute == false)
        {
            MoveSpeed = 3.5f;
        }
    }
}
