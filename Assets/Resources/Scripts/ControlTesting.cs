using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MyController {
    private static bool IsVR = false;

    public static void Initialize()
    {
        ControlTesting.Instance.StartCoroutine(Init());
    }

    public static IEnumerator Init()
    {
        yield return new WaitForSeconds(0.1f);
    }

    public static void ToggleControlStyle()
    {
        IsVR = !IsVR;
    }

    public static bool GetControlType {
        get {
            return IsVR;
        }
    }
}


//From https://www.reddit.com/r/Unity3D/comments/3y2scl/how_to_call_a_coroutine_from_a/
//Used in the event we need to call a coroutine from inside MyController
public class ControlTesting : MonoBehaviour
{
    private static ControlTesting _Instance;
    

    public static ControlTesting Instance
    {
        get
        {
            if (_Instance == null) _Instance = new GameObject("ControlTesting").AddComponent<ControlTesting>();
            return _Instance;
        }
    }

    public PC_Interface _pc;
    public GameObject Player;
    private GameObject FlashLight;
    private GameObject projectionMarker;

    void Awake()
    {
        if (_Instance == null) _Instance = this;
    }

    void Start()
    {
        //Debug.Assert(Player != null, "Please set player object in inspector");
        //Debug.Assert(_pc != null, "Please set PC interface in inspector");
        //ASLHelper.InstantiateASLObject("PlayerFlashLight", new Vector3(0, 60, 0), Quaternion.identity, "", "", InstantiateFlashlight);


        //projectionMarker = Instantiate(Resources.Load("MyPrefabs/PlayerMarker") as GameObject);
        //Destroy(projectionMarker.GetComponent<BoxCollider>());
        //projectionMarker.SetActive(false);
    }

    void Update()   //currently only intended for use with GetKeyDown and GetKeyUp
    {
        if (MyController.GetControlType)    //vr controller
        {
            //whatever the vr controls are
        }
        else
        {   //keyboard + mouse
            if(_pc != null) //Inputs specific to non-vr
            {
                if (Input.GetKeyDown(KeyCode.V))    //Toggle between table camera and player camera
                {
                    PC_Interface.ToggleCameras();
                }
                if (Input.GetKeyDown(KeyCode.P))    //Toggle cursor lock
                {
                    PC_Interface.ToggleLocked();
                }
                if (Input.GetKeyDown(KeyCode.Y))    //Toggle flashlight
                {

                }
                PC_Interface.ProjectMarker(projectionMarker);
                PC_Interface.Paint(Input.GetMouseButton(1));

                if (Input.GetMouseButtonDown(0))    //Raycast to either place marker or select/deselect a marker
                {
                    PC_Interface.OnClickLMB(Input.GetKeyDown(KeyCode.LeftShift));
                }
                if (Input.GetMouseButton(0))        //Drag + draw cast
                {
                    PC_Interface.OnHoldLMB();
                }
                if (Input.GetMouseButtonUp(0))      //Finish drag + draw
                {
                    PC_Interface.OnHoldLMB();
                }
            }
            
            if (Input.GetKeyDown(KeyCode.Alpha2))   //Switch to new path
            {
                DisplayManager.ShowPath();
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))   //Disable camera
            {
                //PathDisplay.DetatchPathCam();
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))   //Toggle ability to see path
            {
                DisplayManager.DisplayToggle();
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))   //Reset route and path
            {
                DisplayManager.ResetDisplay();
            }
            if (Input.GetKeyDown(KeyCode.Backspace))//Delete last placed marker
            {
                MarkerGeneratorV2.DeleteLastPlaced();
            }
            if (Input.GetKeyDown(KeyCode.Minus))    //Delete selected marker
            {
                MarkerGeneratorV2.DeleteSelected();
            }
            if (Input.GetKeyDown(KeyCode.T))        //Teleport between the large map and small map
            {

            }
        }
    }

    private static void InstantiateFlashlight(GameObject _myGameObject)
    {
        _Instance.FlashLight = _myGameObject;
        _myGameObject.SetActive(false);
    }
}
