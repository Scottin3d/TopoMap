using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public static class PathDisplayV2
{
    private static List<GameObject> drawnPath = new List<GameObject>();
    private static List<GameObject> smallPath = new List<GameObject>();

    private static int batchSize;
    private static float UPS;
    private static int outstandingCallbacks;

    private static bool IsRendering;

    private static GameObject smallMap;
    private static Color myColor;


    public static void SetBatchSize(int size)
    {
        batchSize = (int)Mathf.Abs(size);
    }
    public static void SetColor(Color _c) { myColor = _c; }
    public static void SetRenderer(bool toRender) { IsRendering = toRender; }
    public static void SetSmallMap(GameObject _sm) { smallMap = _sm; }
    public static void SetUPS(float _f)
    {
        UPS = (Mathf.Abs(_f) > 10) ? Mathf.Abs(_f) : 10f;
    }    
    
    public static int PathCount {  get { return drawnPath.Count; } }
    public static int SmallCount { get { return smallPath.Count; } }

    public static void Init(GameObject _sm, Color _c, float _up)
    {
        smallMap = _sm;
        myColor = _c;
        UPS = (Mathf.Abs(_up) > 10) ? Mathf.Abs(_up) : 10f;
    }

    #region PATH_POOLING

    /// <summary>
    /// Pools n new route segments for the small and large maps
    /// </summary>
    /// <param name="pathCount">The number of segments to pool</param>
    /// <returns></returns>
    public static IEnumerator PathPooling(int pathCount)
    {
        if(pathCount > 0)
        {
            float delay = (Mathf.Abs(UPS) > 0) ? UPS : 10f;
            outstandingCallbacks += pathCount;
            for (int i = 0; i < pathCount; i++)
            {
                ASLHelper.InstantiateASLObject("MinimapMarker_RoutePath", new Vector3(0, 0, 0), Quaternion.identity, "", "", PathInstantiated);
                ASLHelper.InstantiateASLObject("MinimapMarker_RoutePath", new Vector3(0, 0, 0), Quaternion.identity, "", "", SmallInstantiated);
                yield return new WaitForSeconds(1f / delay);
            }
        }
        yield return new WaitForSeconds(0.01f);
    }

    /// <summary>
    /// Callback from instantiating a large map route segment.
    /// Decrements outstanding callback count when finished.
    /// </summary>
    /// <param name="_myGameObject">The game object that initiated this callback</param>
    public static void PathInstantiated(GameObject _myGameObject)
    {
        drawnPath.Add(_myGameObject);
        _myGameObject.layer = LayerMask.NameToLayer("Markers");
        _myGameObject.transform.parent = DisplayHelperV2.Instance.gameObject.transform;
        _myGameObject.SetActive(false);
        _myGameObject.GetComponent<ASLObject>().SendAndSetClaim(() =>
        {
            float[] toSend = { myColor.r, myColor.g, myColor.b, myColor.a, _myGameObject.transform.localScale.y, 0.85f };
            _myGameObject.GetComponent<ASLObject>().SendFloatArray(toSend);
        });        
        outstandingCallbacks--;
    }

    /// <summary>
    /// Callback from instantiating a small map route object
    /// </summary>
    /// <param name="_myGameObject">The game object that initiated this callback</param>
    public static void SmallInstantiated(GameObject _myGameObject)
    {
        smallPath.Add(_myGameObject);
        _myGameObject.layer = LayerMask.NameToLayer("Markers");
        _myGameObject.transform.parent = DisplayHelperV2.Instance.gameObject.transform;
        _myGameObject.SetActive(false);
    }

    #endregion

    /// <summary>
    /// Given a Bezier spline, displays the route on the large and small maps
    /// </summary>
    /// <param name="_bs">The bezier curve to trace</param>
    /// <returns></returns>
    public static IEnumerator DrawPath(BezierSpline _bs)
    {
        ClearPath();
        IsRendering = true;

        float delay = (Mathf.Abs(UPS) > 0) ? UPS : 10f;
        //Pool additional markers if necessary
        if (_bs.Length > PathCount)
        {
            DisplayHelperV2.Instance.StartCoroutine(PathPooling((int)(_bs.Length - PathCount) - outstandingCallbacks));
        }
        //Get stepsize for tracing
        float stepSize = StepSize(_bs);

        //Begin display
        for (int ndx = 0, p = 0; ndx < PathCount + outstandingCallbacks; ndx++, p++)
        {
            //Ensure that callbacks are finished
            while (ndx > drawnPath.Count - 1) { yield return new WaitForSeconds(0.001f / delay); }
            Debug.Log("Passed route callbacks");
            while (ndx > smallPath.Count - 1) { yield return new WaitForSeconds(0.001f / delay); }
            Debug.Log("Passed small callbacks");

            //ASL display logic
            ASL_Display(ndx, p, _bs, stepSize);
        }             
        yield return new WaitForSeconds(1f);
    }

    static float StepSize(BezierSpline _bs)
    {
        float stepSize = PathCount + outstandingCallbacks;
        if (_bs.Loop || stepSize == 1)
        {
            stepSize = 1f / stepSize;
        }
        else if (stepSize > _bs.Length)
        {
            stepSize = 1f / (_bs.Length - 1);
        }
        else
        {
            stepSize = 1f / (stepSize - 1);
        }
        return stepSize;
    }

    /// <summary>
    /// ASL Display logic. 
    /// Places large path segments along the spline at set intervals
    /// Places small path segments at corresponding positions on the small map
    /// Segments will be hidden if the path is set to not render
    /// </summary>
    /// <param name="ndx">The current segment index to render</param>
    /// <param name="p">The point along the spline to render</param>
    /// <param name="_bs">The Bezier curve to render on</param>
    /// <param name="stepSize">The distance between each route segment relative to the curve</param>
    static void ASL_Display(int ndx, int p, BezierSpline _bs, float stepSize)
    {
        //Transform variables
        Vector3 pos, scale, dir;

        //Display logic
        if (ndx < _bs.Length)
        {
            //Position and scale of large route segments
            drawnPath[ndx].SetActive(true);
            smallPath[ndx].SetActive(true);

            //Position, scale, and direction of large route segments
            pos = _bs.GetPoint(p * stepSize) + 2f * Vector3.up;
            scale = (IsRendering) ? new Vector3(0.5f, 0.5f, 0.5f) : new Vector3(0, 0.5f, 0);
            dir = _bs.GetDirection(p * stepSize);
            drawnPath[ndx].transform.up = dir;
            DrawRouteObject(drawnPath[ndx], pos, scale);

            if (smallMap != null)
            {
                //Position, scale, and direction of small route segments
                pos = smallMap.transform.position + pos / MarkerDisplay.GetScaleFactor();
                scale = (IsRendering) ? new Vector3(0.02f, scale.y / MarkerDisplay.GetScaleFactor(), 0.02f) :
                    new Vector3(0, scale.y / MarkerDisplay.GetScaleFactor(), 0);
                smallPath[ndx].transform.up = dir;
                DrawRouteObject(smallPath[ndx], pos, scale);
            }
            else
            {
                smallPath[ndx].SetActive(false);
            }

        }
        else { drawnPath[ndx].SetActive(false); smallPath[ndx].SetActive(false); }
    }

    public static IEnumerator ToggleRenderer(BezierSpline _bs)
    {
        float delay = (Mathf.Abs(UPS) > 0) ? Mathf.Abs(UPS) : 10f;
        Vector3 scale;

        IsRendering = !IsRendering;
        if (IsRendering)
        {
            float stepSize = StepSize(_bs);
            //show all paths up to bezier spline
            for (int ndx = 0; ndx < PathCount; ndx++)
            {
                if (ndx > PathCount - 1 || ndx > SmallCount - 1)
                {
                    ndx--;
                }
                else
                {
                    if (ndx < _bs.Length)
                    {
                        scale = new Vector3(0.5f, 0.5f, 0.5f);
                        DrawRouteObject(drawnPath[ndx], drawnPath[ndx].transform.position, scale);
                        if (smallMap != null)
                        {
                            scale = new Vector3(0.02f, scale.y / MarkerDisplay.GetScaleFactor(), 0.02f);
                            DrawRouteObject(smallPath[ndx], smallPath[ndx].transform.position, scale);
                        }
                        else
                        {
                            scale = new Vector3(0, 0, 0);
                            DrawRouteObject(smallPath[ndx], smallPath[ndx].transform.position, scale);
                        }
                    }
                    else
                    {
                        scale = new Vector3(0, 0, 0);
                        DrawRouteObject(drawnPath[ndx], drawnPath[ndx].transform.position, scale);
                        DrawRouteObject(smallPath[ndx], smallPath[ndx].transform.position, scale);
                    }
                }
                yield return new WaitForSeconds(0.001f / delay);
            }
        }
        else
        {
            //
            for (int ndx = 0; ndx < PathCount + outstandingCallbacks; ndx++)
            {
                if (ndx > PathCount - 1 || ndx > SmallCount - 1)
                {
                    ndx--;
                }
                else
                {
                    scale = new Vector3(0, 0.5f, 0);
                    DrawRouteObject(drawnPath[ndx], drawnPath[ndx].transform.position, scale);
                    scale = new Vector3(0, 0.5f / MarkerDisplay.GetScaleFactor(), 0);
                    DrawRouteObject(smallPath[ndx], smallPath[ndx].transform.position, scale);
                }
                yield return new WaitForSeconds(0.001f / delay);
            }
        }
        yield return new WaitForSeconds(1f);
    }

    /// <summary>
    /// Clears the small and large paths
    /// </summary>
    public static void ClearPath()
    {
        foreach(GameObject _g in drawnPath)
        {
            _g.SetActive(false);
        }
        foreach (GameObject _g in smallPath)
        {
            _g.SetActive(false);
        }
    }

    /// <summary>
    /// Draws a route segment in ASL space
    /// </summary>
    /// <param name="_g">The route segment game object to draw</param>
    /// <param name="pos">The position of the route segment</param>
    /// <param name="scale">The scale of the route segment</param>
    private static void DrawRouteObject(GameObject _g, Vector3 pos, Vector3 scale)
    {
        _g.GetComponent<ASLObject>().SendAndSetClaim(() =>
        {
            _g.GetComponent<ASLObject>().SendAndSetWorldPosition(pos);
            _g.GetComponent<ASLObject>().SendAndSetLocalScale(scale);
            _g.GetComponent<ASLObject>().SendAndSetLocalRotation(_g.transform.localRotation);


            float[] toSend = { myColor.r, myColor.g, myColor.b, myColor.a, scale.y, 0.85f };
            _g.GetComponent<ASLObject>().SendFloatArray(toSend);

        });
    }

    /// <summary>
    /// Checks the route segment count against the length of a bezier spline
    /// </summary>
    /// <param name="splineLength">The length of the bezier spline</param>
    public static void DisplayCheck(float splineLength)
    {
        if (PathCount < (int)splineLength)
        {
            DisplayHelperV2.Instance.StartCoroutine(PathPooling((int)(splineLength - PathCount)));
        }
        //Debug.Log("Callbacks: " + outstandingCallbacks);
    }
}

//From https://www.reddit.com/r/Unity3D/comments/3y2scl/how_to_call_a_coroutine_from_a/
//Used in the event we need to call a coroutine from inside PathDisplayV2
public class DisplayHelperV2 : MonoBehaviour
{
    private static DisplayHelperV2 _Instance;

    public static DisplayHelperV2 Instance
    {
        get
        {
            if (_Instance == null) _Instance = new GameObject("PathDisplayHelper").AddComponent<DisplayHelperV2>();
            return _Instance;
        }
    }
}
