using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTheDrawingRouteOnLargeMap : MonoBehaviour
{
    public GameObject Player;
    public GameObject TeleportManger;

    private float MoveSpeed = 3.5f;
    private List<GameObject> MyLargerBrushListFromPlayerDrawRoute;
    private bool StartFollowingRoute = false;
    private bool ResetPlayer = true;
    private int BrushListLength;
    private int CurIndex;
    private Vector3 FirstBrush;
    private Vector3 SecondBrush;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        StartFollowTheRoute();
        IncreaseMoveSpeed();
    }

    private void StartFollowTheRoute()
    {
        if (Input.GetKeyDown(KeyCode.Alpha3) && StartFollowingRoute == false)
        {
            GetRouteList();

            if (BrushListLength >= 2)
            {
                ResetPlayer = true;
                StartFollowingRoute = true;
                Player.GetComponent<CharacterController>().enabled = false;
                Player.GetComponent<CharacterController>().detectCollisions = false;
                Player.transform.position = FirstBrush;
            }
        }

        if (StartFollowingRoute == true && CurIndex < BrushListLength - 1)
        {
            Player.GetComponent<CharacterController>().enabled = false;
            Player.GetComponent<CharacterController>().detectCollisions = false;
            float Step = MoveSpeed * Time.deltaTime;
            Player.transform.position = Vector3.MoveTowards(Player.transform.position, SecondBrush, Step);

            if (Vector3.Distance(Player.transform.position, SecondBrush) < 0.005f)
            {
                MoveToNextTwoBrush();
            }
        }

        if (CurIndex >= BrushListLength - 1 && ResetPlayer == true)
        {
            ResetPlayer = false;
            StartFollowingRoute = false;
            Player.GetComponent<CharacterController>().enabled = true;
            Player.GetComponent<CharacterController>().detectCollisions = true;
        }
    }

    private void GetRouteList()
    {
        if (TeleportManger.GetComponent<TeleportBetweenMap>().GetAtSmallMap())
        {
            return;
        }
        else
        {
            MyLargerBrushListFromPlayerDrawRoute = this.GetComponent<PlayerDrawRoute>().GetMyLargerBrushList();
            BrushListLength = MyLargerBrushListFromPlayerDrawRoute.Count;
            CurIndex = 0;

            if (BrushListLength >= 2)
            {
                //Get First and Second brush here
                FirstBrush = MyLargerBrushListFromPlayerDrawRoute[CurIndex].transform.position;
                SecondBrush = MyLargerBrushListFromPlayerDrawRoute[CurIndex + 1].transform.position;

                FirstBrush.y += 3f;
                SecondBrush.y += 3f;

                CurIndex++;
            }
        }
    }

    private void MoveToNextTwoBrush()
    {
        FirstBrush = MyLargerBrushListFromPlayerDrawRoute[CurIndex].transform.position;
        SecondBrush = MyLargerBrushListFromPlayerDrawRoute[CurIndex + 1].transform.position;

        FirstBrush.y += 3f;
        SecondBrush.y += 3f;

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
