using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMarkerGenerator : MonoBehaviour
{
    private GameObject SelectedMap;
    private Camera PlayerCamera;
    private static GameObject MarkerObject;
    public static List<GameObject> PlayerSetMarker = new List<GameObject>();
    public Dropdown MyDropdownList;

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
        SelectObjectByClick();
    }

    private void SelectObjectByClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray MouseRay = PlayerCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit Hit;
            if (Physics.Raycast(MouseRay, out Hit))
            {
                if (Hit.collider.tag == "Plane")
                {
                    string DropdownOpionValue = MyDropdownList.options[MyDropdownList.value].text;
                    ASL.ASLHelper.InstantiateASLObject(DropdownOpionValue, Hit.point, Quaternion.identity, "", "", GetHoldObject);
                }
                else
                {
                    return;
                }
            }
        }
    }

    private static void GetHoldObject(GameObject _myGameObject)
    {
        MarkerObject = _myGameObject;
        PlayerSetMarker.Add(_myGameObject);
    }
}
