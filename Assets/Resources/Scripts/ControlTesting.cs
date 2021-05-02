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

    void Update()   //currently only intended for use with GetKeyDown and GetKeyUp
    {
        if (MyController.GetControlType)    //vr controller
        {
            //whatever the vr controls are
        }
        else
        {   //keyboard + mouse
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                RouteDisplayV2.ShowPath();
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                PathDisplay.DetatchPathCam();
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                PathDisplay.ToggleNotRender();
            }
        }
    }
}
