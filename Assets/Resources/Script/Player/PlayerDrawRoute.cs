using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDrawRoute : MonoBehaviour
{
    private Camera PlayerCamera;
    public static List<GameObject> MyBrushList = new List<GameObject>();
    public Dropdown MyEraseDropDown;

    void Awake()
    {
        PlayerCamera = GameObject.Find("Player").GetComponentInChildren<Camera>();
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
        if (Input.GetMouseButton(1))
        {
            Ray MouseRay = PlayerCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit Hit;
            if (Physics.Raycast(MouseRay, out Hit))
            {
                if (Hit.collider.tag == "WhiteBoard")
                {
                    ASL.ASLHelper.InstantiateASLObject("Brush", Hit.point, Quaternion.identity, "", "", GetEachBrush);
                }
            }
        }
    }

    private static void GetEachBrush(GameObject _myGameObject)
    {
        MyBrushList.Add(_myGameObject);
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
        for(int i = 0; i < 100; i++)
        {
            MyBrushList[MyBrushList.Count - 1].GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
            {
                MyBrushList[MyBrushList.Count - 1].GetComponent<ASL.ASLObject>().DeleteObject();
            });
            MyBrushList.RemoveAt(MyBrushList.Count - 1);
        }
    }
}
