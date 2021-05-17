using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using ASL;

public static class PathDisplayV2
{
    private static List<GameObject> drawnPath = new List<GameObject>();
    private static List<GameObject> smallPath = new List<GameObject>();

    private static int batchSize;
    private static float mapHeight;
    private static float UPS;
    private static int outstandingCallbacks;
    private static bool isDrawing;

    private static Color myColor;


    public static void SetBatchSize(int size)
    {
        batchSize = (int)Mathf.Abs(size);
    }
    public static void SetColor(Color _c) { myColor = _c; }
    public static void SetMapHeight(float _f) { mapHeight = _f; }
    public static void SetUPS(float _f)
    {
        UPS = (Mathf.Abs(_f) > 10) ? Mathf.Abs(_f) : 10f;
    }

    
    public static int PathCount {  get { return drawnPath.Count; } }
    public static int SmallCount { get { return smallPath.Count; } }

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
        _myGameObject.SetActive(false);
    }

    /// <summary>
    /// Given a Bezier spline, displays the route on the large and small maps
    /// </summary>
    /// <param name="_bs">The bezier curve to trace</param>
    /// <returns></returns>
    public static IEnumerator DrawPath(BezierSpline _bs)
    {
        if (!isDrawing)
        {
            isDrawing = true;
            //Find the small map for drawing the small route
            GameObject smallMap = GameObject.FindWithTag("SpawnSmallMap");

            float delay = (Mathf.Abs(UPS) > 0) ? UPS : 10f;
            //Pool additional markers if necessary
            if (_bs.Length > PathCount)
            {
                DisplayHelperV2.Instance.StartCoroutine(PathPooling((int)(_bs.Length - PathCount) - outstandingCallbacks));
            }
            SplineDecorator.SetSpline(_bs);


            bool NoCallBacks;
            bool NoSmallbacks;
            //Begin display
            for (int ndx = 0; ndx < PathCount + outstandingCallbacks; ndx++)
            {
                //Ensure that callbacks are finished
                NoCallBacks = true;
                NoSmallbacks = true;
                while (ndx > drawnPath.Count - 1) { yield return new WaitForSeconds(0.1f / delay); NoCallBacks = false; }
                Debug.Log("Passed route callbacks");
                while (ndx > smallPath.Count - 1) { yield return new WaitForSeconds(0.1f / delay); NoSmallbacks = false; }
                Debug.Log("Passed small callbacks");

                //Delay if no callbacks
                if (NoCallBacks) yield return new WaitForSeconds(0.1f / delay);
                if (NoSmallbacks) yield return new WaitForSeconds(0.1f / delay);

                //Give large route segments to spline decorator for drawing
                SplineDecorator.Decorate(drawnPath);

                //ASL display logic
                if (ndx < _bs.Length)
                {
                    //Position and scale of large route segments
                    Vector3 pos = drawnPath[ndx].transform.position;
                    Vector3 scale = drawnPath[ndx].transform.localScale;
                    DrawRouteObject(drawnPath[ndx], pos, scale);

                    //Set position of small route segments
                    //TODO: fix small route display
                    if (smallMap != null)
                    {
                        //Set active and get position and scale for small route segments
                        smallPath[ndx].SetActive(true);
                        smallPath[ndx].transform.up = drawnPath[ndx].transform.up;
                        pos = smallMap.transform.position + pos / MarkerDisplay.GetScaleFactor();
                        scale = new Vector3(0.01f, scale.y / MarkerDisplay.GetScaleFactor(), 0.01f);
                        DrawRouteObject(smallPath[ndx], pos, scale);
                    }
                    else
                    {
                        smallPath[ndx].SetActive(false);
                    }


                }
                else { drawnPath[ndx].SetActive(false); smallPath[ndx].SetActive(false); }

            }
            isDrawing = false;
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
        Debug.Log("Callbacks: " + outstandingCallbacks);
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
