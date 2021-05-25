using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using ASL;

public static class RouteDisplayV2 
{
    #region VARIABLES
    
    private static float updatesPerSecond = 10f;
    private static float heightAboveMarker = 5f;
    private static int batchSize = 10;
    
    private static List<GameObject> routeMarkerPool = new List<GameObject>();
    private static List<GameObject> routeConnectPool = new List<GameObject>();
    private static List<GameObject> smallConnectPool = new List<GameObject>();

    private static List<Transform> linkedObj = new List<Transform>();
    private static int nodeCount = 0;

    //Map references
    private static Transform MapDisplay = null;
    private static GameObject SmallMap;
    private static GameObject LargeMap;

    private static bool DonePooling = false;
    private static bool RouteRendering = true;

    //Data used in MapPath functions
    private static AstarData data;
    public static ABPath myPath;   //This is public because of RtDisplayEditor

    private static bool DataCollected = false;
    private static bool DrawPath = false;

    private static BezierSpline mySpline = null; 
    private static Coroutine drawCoroutine;
    private static Coroutine renderCoroutine;

    //private const int gridRes = 16;    //Node resolution of the graph
    //[Range(0.1f,10f)]
    //public float traceSpeed = 0.3f;    //inverse Approximate time to trace 3 path nodes

    //Would prefer to fetch this from the player once instantiated
    private static Color myColor;

    #endregion

    public static void Init(BezierSpline _ms, GameObject _lm, GameObject _sm, Color _c, float _UPS, float _dispHeight, int _batchSize)
    {
        LargeMap = _lm;
        SmallMap = _sm;
        myColor = _c;
        mySpline = _ms;
        batchSize = _batchSize;
        GenerateRoutePool(_batchSize);

        updatesPerSecond = _UPS;
        heightAboveMarker = _dispHeight;
    }


    /// <summary>
    /// Adds a set number of route objects to the pool
    /// </summary>
    /// <param name="toAdd">The number of route segments to add to the pool</param>
    private static void GenerateRoutePool(int toAdd)
    {
        for(int i = 0; i < toAdd; i++)
        {
            ASLHelper.InstantiateASLObject("MinimapMarker_RouteNode", new Vector3(0, 0, 0), Quaternion.identity, "", "", MarkerInstantiation);
            ASLHelper.InstantiateASLObject("MinimapMarker_RoutePath", new Vector3(0, 0, 0), Quaternion.identity, "", "", RouteInstantiation);
            ASLHelper.InstantiateASLObject("MinimapMarker_RoutePath", new Vector3(0, 0, 0), Quaternion.identity, "", "", SmallRouteInstantiation);
        }
        DonePooling = true;
    }

    #region DRAW_DIRECT_ROUTE

    /// <summary>
    /// Draws a route node, then updates the current and previous connector paths on the large and small maps
    /// </summary>
    /// <param name="actNdx">The index of the node to be acted upon</param>
    private static void UpdateRouteV2(int actNdx, bool Recheck)
    {
        while (!DonePooling) Debug.Log("Route checks not cleared");
        //Debug.Log("ndx of action: " + actNdx);
        Vector3 pos, scale, nextPos, prevPos;

        if (actNdx >= 0)
        {
            scale = new Vector3(1.5f, 0.5f, 1.5f);
            pos = linkedObj[actNdx].position + heightAboveMarker * Vector3.up;

            GameObject curNode = routeMarkerPool[actNdx];
            GameObject curRoute = routeConnectPool[actNdx];
            GameObject smRoute = smallConnectPool[actNdx];

            curNode.SetActive(true);
            DrawRouteObject(curNode, pos, scale);
                
            if (actNdx > 0)
            {
                GameObject prevNode = routeMarkerPool[actNdx - 1];
                GameObject prevRoute = routeConnectPool[actNdx - 1];
                GameObject smPrev = smallConnectPool[actNdx - 1];
                prevPos = linkedObj[actNdx - 1].position + heightAboveMarker * Vector3.up;
                if (!Recheck) UpdateRouteV2(actNdx - 1, true);
                RouteDrawV2(prevPos, pos, prevRoute, smPrev);
            }
            if (actNdx + 1 < routeConnectPool.Count)
            {
                if(actNdx+1 < nodeCount)
                {
                    nextPos = linkedObj[actNdx + 1].position + heightAboveMarker * Vector3.up;
                    RouteDrawV2(pos, nextPos, curRoute, smRoute);
                } else
                {
                    curRoute.SetActive(false);
                    smRoute.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// Draws the connectors on the large and small maps
    /// </summary>
    /// <param name="start">The start point of the route draw on the large map</param>
    /// <param name="end">The end point of the route draw on the large map</param>
    /// <param name="route">The route connector on the large map</param>
    /// <param name="small">The route connector on the small map</param>
    private static void RouteDrawV2(Vector3 start, Vector3 end, GameObject route, GameObject small)
    {
        //Debug.Log("Start: " + start + "; End: " + end);
        Vector3 dir, scale, pos;
        float length = 0f;

        route.SetActive(true);
        small.SetActive(true);

        dir = end - start;
        length = dir.magnitude / 2f;
        scale = (RouteRendering) ? new Vector3(.25f, length, .25f) : new Vector3(0, length, 0);
        pos = start + (length * dir.normalized);
        route.transform.up = dir;
        DrawRouteObject(route, pos, scale);

        float scaleFactor = MarkerDisplay.GetScaleFactor();
        small.transform.up = dir;
        if (MapDisplay != null)
        {
            if (scaleFactor > 0)
            {
                pos = MapDisplay.position + ((pos - 0.5f * heightAboveMarker * Vector3.up) / scaleFactor);
                scale = (RouteRendering) ? new Vector3(.05f, length / scaleFactor, .05f) : new Vector3(0, length / scaleFactor, 0);
            }
            else
            {
                pos = Vector3.zero;
                scale = Vector3.zero;
            }
        }
        else
        {
            if (scaleFactor > 0)
            {
                pos = SmallMap.transform.position + ((pos - 0.5f * heightAboveMarker * Vector3.up) / scaleFactor);
                scale = (RouteRendering) ? new Vector3(0.01f, length / scaleFactor, 0.01f) : new Vector3(0, length / scaleFactor, 0);
            }
            else
            {
                pos = Vector3.zero;
                scale = Vector3.zero;
            }
        }
        DrawRouteObject(small, pos, scale);
    }

    /// <summary>
    /// Sets the scale of connectors on the large and small maps
    /// </summary>
    /// <param name="route">The route connector on the large map</param>
    /// <param name="small">The route connector on the small map</param>
    private static void RouteDrawV2(GameObject route, GameObject small)
    {
        route.SetActive(true);
        small.SetActive(true);

        float length = route.transform.localScale.y;
        Vector3 scale = (RouteRendering) ? new Vector3(0.25f, length, 0.25f) : new Vector3(0, length, 0);
        DrawRouteObject(route, route.transform.position,scale);

        float scaleFactor = MarkerDisplay.GetScaleFactor();
        if(MapDisplay != null)
        {
            if (scaleFactor > 0)
            {
                scale = (RouteRendering) ? new Vector3(.05f, length / scaleFactor, .05f) : new Vector3(0, length / scaleFactor, 0);
            }
            else
            {
                scale = Vector3.zero;
            }
        } else
        {
            if (scaleFactor > 0)
            {
                scale = (RouteRendering) ? new Vector3(0.01f, length / scaleFactor, 0.01f) : new Vector3(0, length / scaleFactor, 0);
            }
            else
            {
                scale = Vector3.zero;
            }
        }
        DrawRouteObject(small, small.transform.position, scale);
    }

    /// <summary>
    /// Sets the position, scale, and rotation of route objects in ASL space
    /// </summary>
    /// <param name="_g">The game object to be modified</param>
    /// <param name="pos">The position of the game object</param>
    /// <param name="scale">The local scale of the game object</param>
    private static void DrawRouteObject(GameObject _g, Vector3 pos, Vector3 scale)
    {
        _g.GetComponent<ASLObject>().SendAndSetClaim(() =>
        {
            _g.GetComponent<ASLObject>().SendAndSetWorldPosition(pos);
            _g.GetComponent<ASLObject>().SendAndSetLocalScale(scale);
            _g.GetComponent<ASLObject>().SendAndSetLocalRotation(_g.transform.localRotation);
            
            
            float[] toSend = { myColor.r, myColor.g, myColor.b, myColor.a, scale.y, 1f};
            _g.GetComponent<ASLObject>().SendFloatArray(toSend);
            
        });
    }

    #endregion

    #region SCAN_GRAPH



    #endregion

    #region TRACE_PATH

    /// <summary>
    /// Get graph data from the display manager
    /// </summary>
    /// <param name="_ad">The AstarData from the display manager</param>
    public static void ReceiveGraphData(AstarData _ad) { data = _ad; DataCollected = true; }

    /// <summary>
    /// Traces a path between each pair of adjacent markers placed on the map. 
    /// Markers are considered adjacent if they are indexed next to each other in the linkedObj list
    /// These paths are combined to create a single path, which is used in BezierTrace
    /// </summary>
    /// <returns></returns>
    public static IEnumerator DrawMapCurveV2()
    {
        while (!DataCollected) yield return new WaitForSeconds(1f / updatesPerSecond);
        DrawPath = true;
        List<Vector3> posList = new List<Vector3>();
        List<GraphNode> nodeList = new List<GraphNode>();
        ABPath tempPath;

        for (int ndx = 0; ndx < linkedObj.Count - 1; ndx++)
        {
            tempPath = ABPath.Construct(linkedObj[ndx].position, linkedObj[ndx + 1].position);
            AstarPath.StartPath(tempPath);
            tempPath.BlockUntilCalculated();

            if (ndx == 0)
            {
                posList.AddRange(tempPath.vectorPath);
                nodeList.AddRange(tempPath.path);
            }
            else
            {
                posList.AddRange(tempPath.vectorPath.GetRange(1, tempPath.vectorPath.Count - 1));
                nodeList.AddRange(tempPath.path.GetRange(1, tempPath.path.Count - 1));
            }
        }

        myPath = ABPath.FakePath(posList, nodeList);
        BezierTrace();
        
    }

    /// <summary>
    /// Resets and then traces over the BezierSpline mySpline based on the ABPath created from DrawMapCurve
    /// </summary>
    private static void BezierTrace()
    {
        if (mySpline != null) mySpline.Reset();
        if (myPath != null && mySpline != null)
        {
            List<Vector3> posNodes = myPath.vectorPath;
            if (posNodes.Count > 1)
            {
                mySpline.SetCurvePoint(0, posNodes[0]);
                int ndx;
                for (ndx = 3; ndx < posNodes.Count; ndx += 3)
                {
                    mySpline.SetCurvePoint(ndx, posNodes[ndx]);
                }
                if(((mySpline.GetLastControlPoint() - posNodes[posNodes.Count - 1]).magnitude > 0.5f))
                {
                    mySpline.SetCurvePoint(ndx, posNodes[posNodes.Count - 1]);
                }
            }
        }
        DrawPath = false;
    }

    /// <summary>
    /// Gets whether DrawPath is true or not
    /// </summary>
    public static bool IsDrawing { get { return DrawPath; } }
    
    #endregion

    #region INSERTION/REMOVAL

    /// <summary>
    /// Adds a marker at the end of the linkedObj list. 
    /// If the linkedObj count is greater than the current route segment length, additional route segments shall be pooled
    /// </summary>
    /// <param name="_t">The transform of the marker game object to be added</param>
    public static void AddRouteMarker(Transform _t)
    {
        linkedObj.Add(_t);
        nodeCount++;
        DonePooling = false;
        if (nodeCount >= routeMarkerPool.Count)
        {
            Debug.Log("Instantiating new batch");
            
            GenerateRoutePool(batchSize);
        } else
        {
            DonePooling = true;
        }
        UpdateRouteV2(linkedObj.Count - 1, false);
    }

    /// <summary>
    /// Attempts to insert a marker into the linkedObj list, based on a marker transform.
    /// If the target cannot be found, the marker will instead be added to the end of the list.
    /// If the linkedObj count is greater than or equal to the current route segment length after insertion, additional route segments shall be pooled
    /// </summary>
    /// <param name="_target">The target transform to search for in the linkedObj list</param>
    /// <param name="_t">The transform of the marker object to be added</param>
    /// <returns>The index of the </returns>
    public static int InsertMarkerAt(Transform _target, Transform _t)
    {
        int ndx = (_target != null) ? linkedObj.IndexOf(_target) : -1;
        if(ndx < 0)
        {
            AddRouteMarker(_t);
        } else
        {
            linkedObj.Insert(ndx + 1, _t);
            nodeCount++;
            if (linkedObj.Count >= routeMarkerPool.Count)
            {
                Debug.Log("Instantiating new batch");
                DonePooling = false;
                GenerateRoutePool(batchSize);
            }
            while (!DonePooling) ;
            Reinsertion(routeConnectPool.Count - 1, ndx + 1);
            UpdateRouteV2(ndx + 1, false);
        }
        return ndx;
    }

    /// <summary>
    /// Attempts to remove a marker from the linkedObj list.
    /// If the marker is not in the linkedObj list, an ASL callback shall be sent to all other clients
    /// </summary>
    /// <param name="_t">The transform to be removed</param>
    /// <param name="fromFloatCallback">Specifies whether this is being called via ASL callback</param>
    /// <returns>Returns true if removal is successful</returns>
    public static bool RemoveRouteMarker(Transform _t, bool fromFloatCallback)
    {
        if (!linkedObj.Contains(_t)) return false;
        int actionNdx = linkedObj.IndexOf(_t);
        if (actionNdx > -1)
        {
            Reinsertion(actionNdx, linkedObj.Count);
            linkedObj.Remove(_t);
            nodeCount--;
            if (nodeCount < 1) DisplayManager.ClearPath();
            UpdateRouteV2(actionNdx - 1, false);
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Reinserts route segments from one point in their respective lists to a different point
    /// </summary>
    /// <param name="removeFrom">The index of the segments to be moved</param>
    /// <param name="insertAt">The index to reinsert the segments</param>
    private static void Reinsertion(int removeFrom, int insertAt)
    {
        if (removeFrom > routeMarkerPool.Count - 1 || removeFrom < 0) return;
        if (insertAt > routeMarkerPool.Count - 1 || insertAt < 0) return;
        if (insertAt > removeFrom) insertAt--;

        GameObject nodeToRemove = routeMarkerPool[removeFrom];
        GameObject pathToRemove = routeConnectPool[removeFrom];
        GameObject smallToRemove = smallConnectPool[removeFrom];

        routeMarkerPool.RemoveAt(removeFrom);
        routeConnectPool.RemoveAt(removeFrom);
        smallConnectPool.RemoveAt(removeFrom);

        routeMarkerPool.Insert(insertAt, nodeToRemove);
        routeConnectPool.Insert(insertAt, pathToRemove);
        smallConnectPool.Insert(insertAt, smallToRemove);

        nodeToRemove.SetActive(false);
        pathToRemove.SetActive(false);
        smallToRemove.SetActive(false);
    }

    /// <summary>
    /// Gets the count of the linkedObj list
    /// </summary>
    public static int NodeCount { get { return nodeCount; } }

    #endregion

    #region DISPLAY

    /// <summary>
    /// Toggles whether the route (drawn with straight lines) or the path (drawn on a curve) is being displayed
    /// </summary>
    public static void ToggleRoute()
    {
        RouteRendering = !RouteRendering;
        if (RouteRendering)
        {
            for(int ndx = 0; ndx < NodeCount - 1; ndx++)
            {
                RouteDrawV2(routeConnectPool[ndx], smallConnectPool[ndx]);
            }
        } else
        {
            RouteHelper.Instance.StartCoroutine(HideRoute());
        }
    }

    /// <summary>
    /// Hides the straight route for path drawing purposes
    /// </summary>
    /// <returns></returns>
    public static IEnumerator HideRoute()
    {
        RouteRendering = false;
        foreach(GameObject _g in routeConnectPool)
        {
            _g.SetActive(false);
        }
        foreach(GameObject _g in smallConnectPool)
        {
            _g.SetActive(false);
        }
        yield return new WaitForSeconds(1f);
    }

    /// <summary>
    /// Clears the route display and deletes all the markers "owned" by the player
    /// </summary>
    public static void ClearRoute()
    {
        nodeCount = 0;
        mySpline.Reset();
        myPath = null;
        foreach (Transform _t in linkedObj)
        {
            PlayerMarkerGenerator.RemoveMarker(_t.gameObject);
        }
        foreach (GameObject _g in routeMarkerPool)
        {
            _g.SetActive(false);
        }
        linkedObj.Clear();
        RouteRendering = true;
    }

    #endregion

    #region CALLBACK_FUNCTIONS

    /// <summary>
    /// Instantiates a marker within ASL space
    /// </summary>
    /// <param name="_myGameObject">The game object that initiated this callback</param>
    private static void MarkerInstantiation(GameObject _myGameObject)
    {
        routeMarkerPool.Add(_myGameObject);
        _myGameObject.transform.parent = RouteHelper.Instance.gameObject.transform;
        _myGameObject.SetActive(false);
    }

    /// <summary>
    /// Instantiates a large route segment within ASL space
    /// </summary>
    /// <param name="_myGameObject">The game object that initiated this callback</param>
    private static void RouteInstantiation(GameObject _myGameObject)
    {
        routeConnectPool.Add(_myGameObject);
        _myGameObject.transform.parent = RouteHelper.Instance.gameObject.transform;
        _myGameObject.SetActive(false);
    }

    /// <summary>
    /// Instantiates a small route segment within ASL space
    /// </summary>
    /// <param name="_myGameObject">The game object that inititated this callback</param>
    private static void SmallRouteInstantiation(GameObject _myGameObject)
    {
        smallConnectPool.Add(_myGameObject);
        _myGameObject.transform.parent = RouteHelper.Instance.gameObject.transform;
        _myGameObject.SetActive(false);
    }

    #endregion

    //Functions not currently in use
    #region UNUSED FUNCTIONS
    /*
/// <summary>
/// Alternative version of BezierTrace that directly uses the linkedObj list
/// </summary>
private void BezierTraceV2()
{
    BezierSpline _bs = mySpline;
    if (_bs != null) _bs.Reset();
    if (linkedObj.Count > 1 && _bs != null)
    {
        for (int ndx = 0; ndx < linkedObj.Count; ndx++)
        {
            _bs.SetCurvePoint(ndx * 3, linkedObj[ndx].position);
        }
        mySpline = _bs;
    }
}

/// <summary>
/// Sends the string ID of an ASLObject as a float array to all clients
/// </summary>
/// <param name="id">The ID of the ASLObject to be sent</param>
private void PrepSearchCallback(string id)
{
    current.gameObject.GetComponent<ASLObject>().SendAndSetClaim(() =>
    {
        //Based on an answer to
        //https://stackoverflow.com/questions/5322056/how-to-convert-an-ascii-character-into-an-int-in-c/37736710#:~:text=A%20char%20value%20in%20C,it%20with%20(int)c%20.
        char[] splitId = id.ToCharArray();
        float[] Ids = new float[splitId.Length];
        for (int i = 0; i < splitId.Length; i++)
        {
            Ids[i] = (float)splitId[i];
        }
        current.gameObject.GetComponent<ASLObject>().SendFloatArray(Ids);
    });
}


/// <summary>
/// Callback function used in conjunction with RemoveRouteMarker to remove markers not in a player's local linkedObj list
/// </summary>
/// <param name="_id">The string ID of the ASLObject that iniated this callback</param>
/// <param name="_f">The float array sent over ASL</param>
public void SyncLists(string _id, float[] _f)
{
    string theID = current.AssembleID(_f);
    bool foundObject = false;
    Transform obj = null;

    List<Transform> transforms = ASLObjectTrackingSystem.GetObjects();
    foreach (Transform _t in transforms)
    {
        if (_t.gameObject.GetComponent<ASLObject>().m_Id.Equals(theID))
        {
            foundObject = true;
            obj = _t;
        }
    }

    if (foundObject)
    {
        RemoveRouteMarker(obj, true);
    }
}

/// <summary>
/// Reassembes an ASLObject ID from a float array, to check if the player has that ASLObject
/// </summary>
/// <param name="f_id">The float array containing the ID</param>
/// <returns>The string ID of the ASLObject to search for</returns>
private string AssembleID(float[] f_id)
{
    char[] c_id = new char[f_id.Length];
    for (int i = 0; i < c_id.Length; i++)
    {
        c_id[i] = (char)f_id[i];
    }
    return string.Concat(c_id);
}
*/
    #endregion
}

//From https://www.reddit.com/r/Unity3D/comments/3y2scl/how_to_call_a_coroutine_from_a/
//Used in the event we need to call a coroutine from inside RouteDisplayV2
public class RouteHelper : MonoBehaviour
{
    private static RouteHelper _Instance;

    public static RouteHelper Instance
    {
        get
        {
            if (_Instance == null) _Instance = new GameObject("RouteDisplayHelper").AddComponent<RouteHelper>();
            return _Instance;
        }
    }
}
