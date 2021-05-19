using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using ASL;

public class RouteDisplayV2 : MonoBehaviour
{
    #region VARIABLES

    public static RouteDisplayV2 current;

    public float updatesPerSecond = 10f;
    public float heightAboveMarker = 5f;
    public int batchSize = 10;
    
    private static List<GameObject> routeMarkerPool = new List<GameObject>();
    private static List<GameObject> routeConnectPool = new List<GameObject>();
    private static List<GameObject> smallConnectPool = new List<GameObject>();

    private static List<Transform> linkedObj = new List<Transform>();
    private static int nodeCount = 0;

    //Map references
    public Transform MapDisplay = null;
    private GameObject SmallMap;
    private GameObject LargeMap;

    private static bool DonePooling = false;
    private static bool RouteRendering = true;

    //Data used in MapPath functions
    private static AstarData data;
    public ABPath myPath;   //This is public because of RtDisplayEditor
    public float scanFactor = 2f;
    [Range(1,5)]
    public int scaleFactor = 2;

    private static bool DataCollected = false;
    private static bool GraphSet = false;
    private static bool DrawPath = false;

    public BezierSpline mySpline = null, oldSpline = null;
    private static Coroutine drawCoroutine;
    private static Coroutine renderCoroutine;

    //private const int gridRes = 16;    //Node resolution of the graph
    //[Range(0.1f,10f)]
    //public float traceSpeed = 0.3f;    //inverse Approximate time to trace 3 path nodes

    //Would prefer to fetch this from the player once instantiated
    private static Color myColor;

    #endregion

    /// <summary>
    /// Ensures that the player controller is working, should be moved out
    /// </summary>
    private void Awake()
    {
        current = this;
        MyController.Initialize();
    }

    /// <summary>
    /// -Instantiate route object pool
    /// -Initialize and instantiate path display
    /// -Generate and scan graph
    /// </summary>
    void Start()
    {
        LargeMap = GameObject.FindWithTag("SpawnLargerMap");
        SmallMap = GameObject.FindWithTag("SpawnSmallMap");
        myColor = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f), .25f);
        GenerateRoutePool(batchSize);

        PathDisplayV2.SetColor(myColor);
        PathDisplayV2.SetUPS(updatesPerSecond);
        StartCoroutine(SetHolomap());
        StartCoroutine(GenerateGraph());
    }

    public static void Init(BezierSpline _ms, BezierSpline _os, GameObject _lm, GameObject _sm, Color _c, float _UPS, float _dispHeight, int _batchSize)
    {
        current.LargeMap = _lm;
        current.SmallMap = _sm;
        myColor = _c;
        current.batchSize = _batchSize;
        GenerateRoutePool(_batchSize);

        current.updatesPerSecond = _UPS;
        current.heightAboveMarker = _dispHeight;

        current.StartCoroutine(SetHolomap());
        current.StartCoroutine(GenerateGraph());
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

    /// <summary>
    /// Gets the color of the route
    /// </summary>
    /// <returns>The color assigned to all route segments</returns>
    public Color GetColor()
    {
        return myColor;
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
            pos = linkedObj[actNdx].position + current.heightAboveMarker * Vector3.up;

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
                prevPos = linkedObj[actNdx - 1].position + current.heightAboveMarker * Vector3.up;
                if (!Recheck) UpdateRouteV2(actNdx - 1, true);
                RouteDrawV2(prevPos, pos, prevRoute, smPrev);
            }
            if (actNdx + 1 < routeConnectPool.Count)
            {
                if(actNdx+1 < nodeCount)
                {
                    nextPos = linkedObj[actNdx + 1].position + current.heightAboveMarker * Vector3.up;
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
        if (current.MapDisplay != null)
        {
            if (scaleFactor > 0)
            {
                pos = current.MapDisplay.position + ((pos - 0.5f * current.heightAboveMarker * Vector3.up) / scaleFactor);
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
                pos = current.SmallMap.transform.position + ((pos - 0.5f * current.heightAboveMarker * Vector3.up) / scaleFactor);
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
        if(current.MapDisplay != null)
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

    public static void ReceiveGraphData(AstarData _ad) { data = _ad; }

    /// <summary>
    /// Generates a graph. A graph shall not be generated if there is no large map, or if data has already been collected.
    /// Once heightmap data has been collected, graph paremeters shall be derieved.
    /// The layer of the holomap chunks (the small map) shall also be set, in addition to the graph
    /// The graph shall be scanned once set, and data shall be considered collected after that point
    /// </summary>
    /// <returns></returns>
    public static IEnumerator GenerateGraph()
    {
        if (current.LargeMap != null && !DataCollected)
        {
            Debug.Log("Generating new graph");
            data = AstarPath.active.data;
            
            GenerateMapFromHeightMap heightMapData = current.LargeMap.GetComponent<GenerateMapFromHeightMap>();

            while (heightMapData == null)
            {
                yield return new WaitForSeconds(1 / current.updatesPerSecond);
                heightMapData = current.LargeMap.GetComponent<GenerateMapFromHeightMap>();
            }

            if(heightMapData.heightmap!= null && heightMapData.heightmap.width >= 32 && heightMapData.heightmap.width % 2 == 0)
            {
                int graphSize = heightMapData.heightmap.width / 2;
                float nodeSize = (float)heightMapData.mapSize / graphSize;
                nodeSize = (nodeSize > 0) ? nodeSize : 1;
                float scanHeight = heightMapData.meshHeight;
                //PathDisplayV2.SetMapHeight(scanHeight);

                //current.StartCoroutine(current.SetHolomap());
                current.StartCoroutine(SetGridGraph(graphSize/2, nodeSize*2, scanHeight));

                while(!GraphSet || !heightMapData.IsGenerated)
                {
                    yield return new WaitForSeconds(1 / current.updatesPerSecond);
                }

                AstarPath.active.Scan(data.graphs);
                //consider culling nodes with no neighbors
                DataCollected = !DataCollected;
            } else
            {
                Debug.LogError("Large map does not have valid heightmap set.", current.LargeMap);
            }            
        }
        yield return new WaitForSeconds(0.1f);
    }

    /// <summary>
    /// Sets various parameters of the graph used for pathfinding
    /// </summary>
    /// <param name="size">The size of the graph. This is half the size calculated in GenerateGraph.</param>
    /// <param name="nodeSize">The size of nodes in the graph. This is twice the size calculated in GenerateGraph.</param>
    /// <param name="scanHeight">The height of the scanning raycasts</param>
    /// <returns></returns>
    static IEnumerator SetGridGraph(int size, float nodeSize, float scanHeight)
    {
        GridGraph graph = data.AddGraph(typeof(GridGraph)) as GridGraph;

        graph.center = current.LargeMap.transform.position;
        graph.neighbours = NumNeighbours.Six;
        graph.maxClimb = 0f;

        graph.SetDimensions(size, size, nodeSize);
        graph.collision.fromHeight = scanHeight * current.scanFactor;
        graph.collision.heightMask = LayerMask.GetMask("Ground");
        //set obstacle layer

        //Set penalties
        graph.penaltyAngle = true;
        graph.penaltyPosition = true;

        GraphSet = true;
        yield return new WaitForSeconds(0.1f);
    }

    /// <summary>
    /// Sets the chunks in the small map to the Holomap layer
    /// </summary>
    /// <returns></returns>
    static IEnumerator SetHolomap()
    {
        MeshFilter[] meshes = current.SmallMap.GetComponentsInChildren<MeshFilter>();
        while(meshes.Length < 1)
        {
            yield return new WaitForSeconds(1 / current.updatesPerSecond);
            meshes = current.SmallMap.GetComponentsInChildren<MeshFilter>();
        }
        Debug.Log("Setting " + meshes.Length + " chunks to Holomap layer");
        int ndx = 0;
        while (ndx < meshes.Length)
        {
            meshes[ndx].gameObject.layer = LayerMask.NameToLayer("Holomap");
            ndx++;
        }
        yield return new WaitForSeconds(0.1f);
    }

    #endregion

    #region TRACE_PATH

    /// <summary>
    /// Traces a path between each pair of adjacent markers placed on the map. 
    /// Markers are considered adjacent if they are indexed next to each other in the linkedObj list
    /// These paths are combined to create a single path, which is used in BezierTrace
    /// </summary>
    /// <returns></returns>
    public static IEnumerator DrawMapCurveV2()
    {
        while (!DataCollected) yield return new WaitForSeconds(1f / current.updatesPerSecond);

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

        current.myPath = ABPath.FakePath(posList, nodeList);
        BezierTrace();
        
        
    }

    /// <summary>
    /// Resets and then traces over the BezierSpline mySpline based on the ABPath created from DrawMapCurve
    /// </summary>
    private static void BezierTrace()
    {
        BezierSpline _bs = current.mySpline;
        if (_bs != null) _bs.Reset();
        if (current.myPath != null && _bs != null)
        {
            List<Vector3> posNodes = current.myPath.vectorPath;
            if (posNodes.Count > 1)
            {
                _bs.SetCurvePoint(0, posNodes[0]);
                int ndx;
                for (ndx = 3; ndx < posNodes.Count; ndx += 3)
                {
                    _bs.SetCurvePoint(ndx, posNodes[ndx]);
                }
                if(((_bs.GetLastControlPoint() - posNodes[posNodes.Count - 1]).magnitude > 0.5f))
                {
                    _bs.SetCurvePoint(ndx, posNodes[posNodes.Count - 1]);
                }
            }
            current.mySpline = _bs;
        }
        PathDisplayV2.DisplayCheck(current.mySpline.Length);
        DrawPath = false;
    }

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
            
            GenerateRoutePool(current.batchSize);
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
            DrawPath = true;
            if (linkedObj.Count >= routeMarkerPool.Count)
            {
                Debug.Log("Instantiating new batch");
                DonePooling = false;
                GenerateRoutePool(current.batchSize);
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
            if (nodeCount < 2) PathDisplayV2.ClearPath();
            DrawPath = true;
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
    /// Clears the current route and purges the linkedObj list
    /// </summary>
    public static void ClearRoute()
    {
        linkedObj.Clear();
        nodeCount = 0;
        PathDisplayV2.ClearPath();
        DrawPath = true;
    }

    /// <summary>
    /// Clears the graph data currently used by the route display
    /// </summary>
    public static void ClearMeshData()
    {
        foreach (NavGraph graph in data.graphs)
        {
            data.RemoveGraph(graph);
        }
        DataCollected = false;
        //current.GroundSet = false;
        GraphSet = false;
        DrawPath = false;
    }

    /// <summary>
    /// Stops active draw and render coroutines
    /// Hides the straight drawn route
    /// Gets the path to be traced and gives that to the spline
    /// Copies the current spline to the old spline, and displays the path using the old spline
    /// </summary>
    public static void ShowPath()
    {
        if(linkedObj.Count > 1)
        {
            //https://answers.unity.com/questions/300864/how-to-stop-a-co-routine-in-c-instantly.html
            if (drawCoroutine != null) current.StopCoroutine(drawCoroutine);
            if (renderCoroutine != null) current.StopCoroutine(renderCoroutine);
            current.StartCoroutine(HidePath());
            DrawPath = true; RouteRendering = false;
            current.StartCoroutine(DrawMapCurveV2());
            while (DrawPath) { Debug.Log("Waiting on path draw"); }
            current.oldSpline.Copy(current.mySpline);
            drawCoroutine = current.StartCoroutine(PathDisplayV2.DrawPath(current.oldSpline));
        }
    }

    /// <summary>
    /// Toggles whether the route (drawn with straight lines) or the path (drawn on a curve) is being displayed
    /// </summary>
    public static void ToggleRoute()
    {
        if (renderCoroutine != null) current.StopCoroutine(renderCoroutine);
        renderCoroutine = current.StartCoroutine(PathDisplayV2.ToggleRenderer(current.oldSpline));
        RouteRendering = !RouteRendering;
        if (RouteRendering)
        {
            for(int ndx = 0; ndx < NodeCount - 1; ndx++)
            {
                RouteDrawV2(routeConnectPool[ndx], smallConnectPool[ndx]);
            }
        } else
        {
            current.StartCoroutine(HidePath());
        }
    }

    /// <summary>
    /// Hides the straight route for path drawing purposes
    /// </summary>
    /// <returns></returns>
    public static IEnumerator HidePath()
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

    #endregion

    #region CALLBACK_FUNCTIONS

    /// <summary>
    /// Instantiates a marker within ASL space
    /// </summary>
    /// <param name="_myGameObject">The game object that initiated this callback</param>
    private static void MarkerInstantiation(GameObject _myGameObject)
    {
        routeMarkerPool.Add(_myGameObject);
        _myGameObject.transform.parent = current.gameObject.transform;
        _myGameObject.SetActive(false);
    }

    /// <summary>
    /// Instantiates a large route segment within ASL space
    /// </summary>
    /// <param name="_myGameObject">The game object that initiated this callback</param>
    private static void RouteInstantiation(GameObject _myGameObject)
    {
        routeConnectPool.Add(_myGameObject);
        _myGameObject.transform.parent = current.gameObject.transform;
        _myGameObject.SetActive(false);
    }

    /// <summary>
    /// Instantiates a small route segment within ASL space
    /// </summary>
    /// <param name="_myGameObject">The game object that inititated this callback</param>
    private static void SmallRouteInstantiation(GameObject _myGameObject)
    {
        smallConnectPool.Add(_myGameObject);
        _myGameObject.transform.parent = current.gameObject.transform;
        _myGameObject.SetActive(false);
    }

    #endregion

    //Functions not currently in use
    #region UNUSED FUNCTIONS

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

    //Depreciated
    /// <summary>
    /// Traces a path between each pair of adjacent markers placed on the map. 
    /// Markers are considered adjacent if they are indexed next to each other in the linkedObj list
    /// These paths are combined to create a single path, which is used in BezierTrace
    /// </summary>
    /// <returns></returns>
    IEnumerator DrawMapCurve()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f / updatesPerSecond);

            if (DrawPath && DataCollected && data.gridGraph != null)
            {
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
                DrawPath = !DrawPath;
                BezierTrace();
            }
        }
    }

    #endregion
}
