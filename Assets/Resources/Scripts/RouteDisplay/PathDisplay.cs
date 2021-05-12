using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ASL;

public static class PathDisplay //: MonoBehaviour
{
    private static List<SplineWalker> walkers = new List<SplineWalker>();
    private static List<GameObject> smallWalker = new List<GameObject>();
    private static int walkerCount = 0;

    private static GameObject cam;
    private static Vector3 camOffset = new Vector3(0f, 2f, 0f);
    private static Quaternion camRot = new Quaternion(-0.2f, 0f, 0f, 1f);
    private static Text myText;

    private static GameObject smallMap;

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

    /// <summary>
    /// Initialize the path display by finding the small camera and small map
    /// Sets the color, speed, and updates per second
    /// </summary>
    /// <param name="_c">The color to use</param>
    /// <param name="tSpeed">The speed of the path walkers</param>
    /// <param name="ups">Updates per second</param>
    /// <returns></returns>
    public static IEnumerator Initialize(Color _c, float tSpeed, float ups)
    {
        cam = GameObject.Find("PathCamera");
        myText = GameObject.Find("PathText").GetComponent<Text>();
        Debug.Assert(cam != null, "Missing path camera");
        Debug.Assert(myText != null, "Missing path text");
        smallMap = GameObject.FindWithTag("SpawnSmallMap");

        myColor = _c;
        updatesPerSecond = ups;
        speed = tSpeed;
        yield return new WaitForSeconds(1f);
        InitFinished = true;
    }

    /// <summary>
    /// Creates a specified number of SplineWalkers and an equal number of sphere primitives
    /// </summary>
    /// <param name="amount">The number of SplineWalkers to create</param>
    /// <returns></returns>
    public static IEnumerator GeneratePathPool(int amount)
    {
        while (!InitFinished) yield return new WaitForSeconds(0.1f);
        int ndx = 0;
        while(ndx < amount)
        {
            yield return new WaitForSeconds(1f / updatesPerSecond);
            smallWalker.Add(GameObject.CreatePrimitive(PrimitiveType.Sphere));
            ASLHelper.InstantiateASLObject("PathWalker", new Vector3(0, 0, 0), Quaternion.identity, "", "", PathTraceInstantiation);
            ndx++;
            walkerCount++;
        }
        foreach(GameObject _g in smallWalker)
        {
            GameObject.Destroy(_g.GetComponent<SphereCollider>());
            _g.GetComponent<MeshRenderer>().material.color = myColor;
            _g.SetActive(false);
        }
        yield return new WaitForSeconds(1f);
        Debug.Log("Done path pooling");
        PoolFinished = true;
    }

    #endregion

    #region DISPLAY

    /// <summary>
    /// Sets the bezier curve of the path to be traced and begins tracing it
    /// </summary>
    /// <param name="_b">THe bezier curve to be traced</param>
    public static void SetSpline(BezierSpline _b)
    {
        mySpline = _b;
        PathDisplayHelper.Instance.StartCoroutine(StartTrace());
    }

    /// <summary>
    /// Sets the speed at which walkers trace the spline
    /// </summary>
    /// <param name="_f">The new speed of the walkers</param>
    public static void SetSpeed(float _f)
    {
        speed = (_f < 0f) ? 0f : _f;
    }

    /// <summary>
    /// Resets all walkers and traces the bezier spline
    /// Sets the duration and speed of all walkers used. Walkers which are not used are disabled
    /// </summary>
    /// <returns></returns>
    public static IEnumerator StartTrace()
    {
        while (RunningTrace) yield return new WaitForSeconds(1f / updatesPerSecond);

        RunningTrace = true;
        if (cam.transform.parent == null) cam.SetActive(false);
        float maxDuration = (mySpline != null) ? mySpline.Length : 0;
        if(maxDuration > 0)
        {
            if(walkerCount * speed * 20f < maxDuration)
            {
                PoolFinished = false;
                PathDisplayHelper.Instance.StartCoroutine(GeneratePathPool((int)(maxDuration - walkerCount * speed * 20f)));
            }
        }
        while (!PoolFinished) yield return new WaitForSeconds(0.1f);
        try
        {
            for (int ndx = 0; ndx < walkerCount; ndx++)
            {
                //Debug.Log("walker " + ndx);
                if (ndx * speed * 30f < maxDuration && speed > 0)
                {
                    walkers[ndx].gameObject.SetActive(true);
                    walkers[ndx].Reset();
                    walkers[ndx].spline = mySpline;
                    walkers[ndx].duration = /*(speed > 0) ? maxDuration / speed :*/ maxDuration;
                    walkers[ndx].SetVelocity(speed);
                    walkers[ndx].constantVelocity = true;
                    walkers[ndx].Begin((float)ndx * speed * 30f / maxDuration);
                }
                else
                {
                    walkers[ndx].gameObject.SetActive(false);
                }

            }
        } catch (Exception e)
        {
            Debug.LogException(e, PathDisplayHelper.Instance);
        } finally
        {
            DoNotRender = false;
            RunningTrace = false;
        }
        yield return new WaitForSeconds(0.1f);
    }

    /// <summary>
    /// Renders all walkers and the camera based on DoNotRender
    /// Also renders the sphere primitives and position text, if needed
    /// </summary>
    /// <returns></returns>
    public static IEnumerator Render()
    {
        while (true)
        {
            cam.transform.localRotation = camRot;
            cam.transform.localPosition = camOffset;
            cam.transform.localScale = Vector3.one;
            for (int ndx = 0; ndx < walkers.Count; ndx++)
            {
                SplineWalker _s = walkers[ndx];
                if (_s.gameObject.activeSelf)
                {
                    _s.ToggleRender(!DoNotRender);
                    if (_s.GetComponentInChildren<PathCam>() != null)
                    {
                        _s.GetComponentInChildren<PathCam>().SetRender(!DoNotRender);
                    }

                    _s.gameObject.GetComponent<ASLObject>().SendAndSetClaim(() =>
                    {
                        _s.gameObject.GetComponent<ASLObject>().SendAndSetLocalPosition(_s.gameObject.transform.position);
                        _s.gameObject.GetComponent<ASLObject>().SendAndSetLocalRotation(_s.gameObject.transform.localRotation);
                    });
                    if (_s.IsRendering)
                    {
                        smallWalker[ndx].SetActive(true);
                        smallWalker[ndx].transform.position = smallMap.transform.position + _s.gameObject.transform.position / MarkerDisplay.GetScaleFactor();
                        smallWalker[ndx].transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
                    }
                    else
                    {
                        smallWalker[ndx].SetActive(false);
                    }
                } else
                {
                    smallWalker[ndx].SetActive(false);
                }
                
                
            }
            if (!DoNotRender) {
                if (cam.transform.parent != null) { 
                    myText.text = string.Format("Position:\n({0:f4},{1:f4})", cam.transform.parent.position.x, cam.transform.parent.position.z);
                } 
                else myText.text = "No node selected";
            }            
            else myText.text = "No node selected";
            yield return new WaitForSeconds(1f / updatesPerSecond);
        }
    }

    /// <summary>
    /// Checks if additional walkers need to be added to the pool
    /// </summary>
    /// <param name="splineLength">The length of the spline to be checked against</param>
    public static void DisplayCheck(float splineLength)
    {
        if (walkerCount * speed * 30f < splineLength)
        {
            PoolFinished = false;
            PathDisplayHelper.Instance.StartCoroutine(GeneratePathPool((int)(splineLength - walkerCount * speed * 30f)));
        }
    }

    /// <summary>
    /// Toggles whether the path is rendered if and only if the route node count is greater than 1
    /// </summary>
    public static void ToggleNotRender()
    {
        if(RouteDisplayV2.NodeCount > 1) DoNotRender = !DoNotRender;
    }

    /// <summary>
    /// Stops rendering the path and detaches the path camera from its selected node
    /// </summary>
    public static void ClearNotRender()
    {
        DoNotRender = true;
        DetatchPathCam();
    }

    #endregion

    #region PATH_CAMERA

    /// <summary>
    /// Attaches the path camera to the selected walker, or detaches the camera from its current walker
    /// </summary>
    /// <param name="_t">The transform to try to attach the camera to</param>
    public static void Select(Transform _t)
    {
        if (cam == null) return;
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

    /// <summary>
    /// Detaches the path camera from its parent, if it has one
    /// </summary>
    public static void DetatchPathCam()
    {
        if (cam == null) return;
        if(cam.transform.parent != null) cam.transform.parent.DetachChildren();
    }

    #endregion

    //Depreciated
    public static float GetWalkerVelocity(Transform _t)
    {
        if(_t == null)
        {
            float maxDuration = (mySpline != null) ? GetSplineLength() : 0;
            return (speed > 0) ? maxDuration / speed : 0;
        }
        else if(_t.gameObject.GetComponent<SplineWalker>() == null)
        {
            float maxDuration = (mySpline != null) ? GetSplineLength() : 0;
            return (speed > 0) ? maxDuration / speed : 0;
        } else
        {
            SplineWalker _sw = _t.gameObject.GetComponent<SplineWalker>();
            return _sw.spline.GetVelocity(_sw.GetProgress()).magnitude;
        }
        
    }

    /// <summary>
    /// Gets the length of the display spline
    /// </summary>
    /// <returns>The length of mySpline</returns>
    public static float GetSplineLength()
    {
        return (mySpline != null) ? mySpline.Length : -1f;
    }

    /// <summary>
    /// Instantiates a spline walker within ASL space
    /// </summary>
    /// <param name="_myGameObject">The game object that initiated this callback</param>
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
