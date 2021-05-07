using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ASL;

public static class PathDisplay //: MonoBehaviour
{
    
    private static List<SplineWalker> walkers = new List<SplineWalker>();
    private static int walkerCount = 0;

    private static GameObject cam;
    private static Vector3 camOffset = new Vector3(0f, 2f, 0f);
    private static Quaternion camRot = new Quaternion(-0.2f, 0f, 0f, 1f);


    private static bool InitFinished = false;
    private static bool PoolFinished = false;
    private static bool RunningTrace = false;
    private static bool DoNotRender = true;

    //Variables from RouteDisplay
    private static Color myColor;
    private static BezierSpline mySpline;
    private static float updatesPerSecond = 10f;
    private static float speed = 0.5f;

    //should also get scale

    #region INIT

    //Start up
    public static IEnumerator Initialize(Color _c, float tSpeed, float ups)
    {
        cam = GameObject.Find("PathCamera");
        Debug.Assert(cam != null, "Missing path camera");

        myColor = _c;
        updatesPerSecond = ups;
        speed = tSpeed;
        yield return new WaitForSeconds(1f);
        InitFinished = true;
    }

    //Create splinewalkers
    public static IEnumerator GeneratePathPool(int amount)
    {
        while (!InitFinished) yield return new WaitForSeconds(0.1f);
        int ndx = 0;
        while(ndx < amount)
        {
            yield return new WaitForSeconds(1f / updatesPerSecond);
            ASLHelper.InstantiateASLObject("PathWalker", new Vector3(0, 0, 0), Quaternion.identity, "", "", PathTraceInstantiation);
            ndx++;
            walkerCount++;
        }
        yield return new WaitForSeconds(1f);
        PoolFinished = true;
    }

    #endregion

    #region DISPLAY

    //Set spline and begin trace
    public static void SetSpline(BezierSpline _b)   //Need some way to clone a spline
    {
        mySpline = _b;
        PathDisplayHelper.Instance.StartCoroutine(StartTrace());
    }

    //Set inverse of walker speed at default
    public static void SetSpeed(float _f)
    {
        _f = (_f > 10f) ? 10f : _f;
        _f = (_f < 0f) ? 0f : _f;
        foreach(SplineWalker _s in walkers)
        {
            _s.duration = (_f > 0) ? mySpline.CurveCount / _f : 0;
        }
    }

    //Set walker positions
    public static IEnumerator StartTrace()
    {
        while (RunningTrace) yield return new WaitForSeconds(1f / updatesPerSecond);

        RunningTrace = true;
        if (cam.transform.parent == null) cam.SetActive(false);
        while (!PoolFinished) yield return new WaitForSeconds(0.1f);
        float maxDuration = (mySpline != null) ? mySpline.CurveCount : 0;
        float spacing = (maxDuration > 0) ? maxDuration / walkerCount : 0;
        float curDuration = 0f;

        for (int ndx = 0; ndx < walkerCount; ndx++)
        {
            walkers[ndx].Reset();
            walkers[ndx].spline = mySpline;
            walkers[ndx].duration = (speed > 0) ? maxDuration / speed : maxDuration;
            curDuration += spacing * speed;
            walkers[ndx].Begin((float)ndx / (float)walkerCount);
        }

        DoNotRender = false;
        yield return new WaitForSeconds(0.1f);
        RunningTrace = false;
    }

    //Show/hide spline walkers
    public static IEnumerator Render()
    {
        while (true)
        {
            cam.transform.localRotation = camRot;
            cam.transform.localPosition = camOffset;
            cam.transform.localScale = Vector3.one;
            foreach(SplineWalker _s in walkers)
            {
                _s.ToggleRender(!DoNotRender);
                if(_s.GetComponentInChildren<PathCam>() != null)
                {
                    _s.GetComponentInChildren<PathCam>().SetRender(!DoNotRender);
                }

                _s.gameObject.GetComponent<ASLObject>().SendAndSetClaim(() =>
                {
                    _s.gameObject.GetComponent<ASLObject>().SendAndSetLocalPosition(_s.gameObject.transform.position);
                    _s.gameObject.GetComponent<ASLObject>().SendAndSetLocalRotation(_s.gameObject.transform.localRotation);
                });
            }
            yield return new WaitForSeconds(1f / updatesPerSecond);
        }
    }

    //Toggle render
    public static void ToggleNotRender()
    {
        DoNotRender = !DoNotRender;
    }

    //Stop rendering
    public static void ClearNotRender()
    {
        DoNotRender = true;
        DetatchPathCam();
    }

    #endregion

    #region PATH_CAMERA

    //Select walker and attach camera
    public static void Select(Transform _t)
    {
        if(cam.transform.parent != null && _t != cam.transform.parent)
        {
            if(_t.gameObject.layer != 9) cam.transform.parent.DetachChildren();
        }
        if (_t.gameObject.layer == 10)
        {
            cam.SetActive(true);
            if (cam.transform.parent != null) cam.transform.parent.DetachChildren();
            cam.transform.SetParent(_t, false);
        }
    }

    //Automatically remove path camera
    public static void DetatchPathCam()
    {
        if(cam.transform.parent != null) cam.transform.parent.DetachChildren();
    }

    #endregion

    //Get velocity of spline walker
    public static float GetWalkerVelocity(Transform _t)
    {
        if(_t == null)
        {
            float maxDuration = (mySpline != null) ? mySpline.CurveCount : 0;
            return (speed > 0) ? maxDuration / speed : 0;
        }
        else if(_t.gameObject.GetComponent<SplineWalker>() == null)
        {
            float maxDuration = (mySpline != null) ? mySpline.CurveCount : 0;
            return (speed > 0) ? maxDuration / speed : 0;
        } else
        {
            SplineWalker _sw = _t.gameObject.GetComponent<SplineWalker>();
            return _sw.spline.GetVelocity(_sw.GetProgress()).magnitude;
        }
        
    }

    //Set up spline walkers
    private static void PathTraceInstantiation(GameObject _myGameObject)
    {
        if (_myGameObject.GetComponent<SplineWalker>() != null)
        {
            walkers.Add(_myGameObject.GetComponent<SplineWalker>());
            _myGameObject.GetComponent<ASLObject>().SendAndSetClaim(() =>
            {
                _myGameObject.GetComponent<ASLObject>().SendAndSetObjectColor(myColor, myColor);
            });
            _myGameObject.GetComponent<MeshRenderer>().enabled = false;
            _myGameObject.GetComponent<MeshCollider>().enabled = false;
        }
        else
        {
            _myGameObject.GetComponent<ASLObject>().SendAndSetClaim(() =>
            {
                _myGameObject.GetComponent<ASLObject>().DeleteObject();
            });
        }
    }
}

//From https://www.reddit.com/r/Unity3D/comments/3y2scl/how_to_call_a_coroutine_from_a/
//Used in the event we need to call a coroutine from inside PathDisplay
public class PathDisplayHelper : MonoBehaviour
{
    private static PathDisplayHelper _Instance;

    public static PathDisplayHelper Instance
    {
        get
        {
            if (_Instance == null) _Instance = new GameObject("PathDisplayHelper").AddComponent<PathDisplayHelper>();
            return _Instance;
        }
    }
}
