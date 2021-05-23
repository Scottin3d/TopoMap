using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTheDrawingRouteOnLargeMap : MonoBehaviour
{
    public GameObject Player;
    public GameObject TeleportManger;

    private float MoveSpeed = 3.5f;
    private List<GameObject> MyEntireBrushList = new List<GameObject>();
    private List<GameObject> MyLargerBrushListFromPlayerDrawRoute = new List<GameObject>();
    private List<List<GameObject>> MyExtraBrushListFromPlayerDrawRoute = new List<List<GameObject>>();

    private bool StartFollowingRoute = false;
    private int BrushListLength;
    private int CurIndex;
    private Vector3 FirstBrush;
    private Vector3 SecondBrush;

    // Update is called once per frame
    void Update()
    {
        if (!TeleportManger.GetComponent<TeleportBetweenMap>().GetAtSmallMap())
        {
            StartFollowTheRoute();
            IncreaseMoveSpeed();
        }
    }

    private void StartFollowTheRoute()
    {
        if (Input.GetKeyDown(KeyCode.Alpha3) && StartFollowingRoute == false)
        {
            GetRouteList();

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
        CurIndex = 0;
        //MyLargerBrushListFromPlayerDrawRoute = this.GetComponent<PlayerDrawRoute>().GetMyLargerBrushList();
        //MyExtraBrushListFromPlayerDrawRoute = this.GetComponent<PlayerDrawRoute>().GetExtraListBetweenOriginBrush();
        MyEntireBrushList = this.GetComponent<PlayerDrawRoute>().GetMyLineRenenderBetweenBrush();

        BrushListLength = MyEntireBrushList.Count;
        CurIndex = 0;

        if (BrushListLength >= 2)
        {
            //Get First and Second brush here
            FirstBrush = MyEntireBrushList[CurIndex].transform.position;
            SecondBrush = MyEntireBrushList[CurIndex + 1].transform.position;

            FirstBrush.y += 3.5f;
            SecondBrush.y += 3.5f;

            CurIndex++;
        }

        //int i = 0;
        //foreach (GameObject B in MyLargerBrushListFromPlayerDrawRoute)
        //{
        //    MyEntireBrushList.Add(B);

        //    if (i < MyExtraBrushListFromPlayerDrawRoute.Count)
        //    {
        //        //MyEntireBrushList.AddRange(MyExtraBrushListFromPlayerDrawRoute[i]);
        //        foreach (GameObject E in MyExtraBrushListFromPlayerDrawRoute[i])
        //        {
        //            MyEntireBrushList.Add(E);
        //        }

        //        i++;
        //    }
        //}

        //BrushListLength = MyEntireBrushList.Count;

        //if (BrushListLength >= 2)
        //{
        //    //Get First and Second brush here
        //    FirstBrush = MyEntireBrushList[CurIndex].transform.position;
        //    SecondBrush = MyEntireBrushList[CurIndex + 1].transform.position;

        //    FirstBrush.y += 3f;
        //    SecondBrush.y += 3f;

        //    CurIndex++;
        //}
    }

    private void MoveToNextTwoBrush()
    {
        FirstBrush = MyEntireBrushList[CurIndex].transform.position;
        SecondBrush = MyEntireBrushList[CurIndex + 1].transform.position;

        FirstBrush.y += 3.5f;
        SecondBrush.y += 3.5f;

        Player.transform.position = FirstBrush;
        CurIndex++;
    }

    private void IncreaseMoveSpeed()
    {
        if (StartFollowingRoute == true && Input.GetKeyDown(KeyCode.W))
        {
            MoveSpeed += 1f;
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
