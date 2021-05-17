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
    
    public List<GameObject> routeMarkerPool = new List<GameObject>();
    private List<GameObject> routeConnectPool = new List<GameObject>();
    private List<GameObject> smallConnectPool = new List<GameObject>();

    private List<Transform> linkedObj = new List<Transform>();
    private int nodeCount = 0;

    //Map references
    public Transform MapDisplay = null;
    private GameObject SmallMap;
    private GameObject LargeMap;

    private bool DonePooling = false;

    //Data used in MapPath functions
    private AstarData data;
    public ABPath myPath;
    public float scanFactor = 2f;
    [Range(1,5)]
    public int scaleFactor = 2;

    private bool DataCollected = false,
        GraphSet = false,
        DrawPath = false;

    public BezierSpline mySpline = null, oldSpline = null;

    private const int gridRes = 16;    //Node resolution of the graph
    [Range(0.1f,10f)]
    public float traceSpeed = 0.3f;    //inverse Approximate time to trace 3 path nodes

    //Would prefer to fetch this from the player once instantiated
    private Color myColor;

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
        SplineDecorator.SetForwardLook(true);
        StartCoroutine(SetHolomap());
        StartCoroutine(GenerateGraph());
    }


    /// <summary>
    /// Adds a set number of route objects to the pool
    /// </summary>
    /// <param name="toAdd">The number of route segments to add to the pool</param>
    private void GenerateRoutePool(int toAdd)
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
    private void UpdateRouteV2(int actNdx, bool Recheck)
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
    /// <param name="route">The route connnector on the large map</param>
    /// <param name="small">The route connector on the small map</param>
    private void RouteDrawV2(Vector3 start, Vector3 end, GameObject route, GameObject small)
    {
        //Debug.Log("Start: " + start + "; End: " + end);
        Vector3 dir, scale, pos;
        float length = 0f;

        route.SetActive(true);
        small.SetActive(true);

        dir = end - start;
        length = dir.magnitude / 2f;
        scale = new Vector3(.25f, length, .25f);
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
                scale = new Vector3(.05f, length / scaleFactor, .05f);
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
                scale = new Vector3(0.01f, length / scaleFactor, 0.01f);
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
    /// Sets the position, scale, and rotation of route objects in ASL space
    /// </summary>
    /// <param name="_g">The game object to be modified</param>
    /// <param name="pos">The position of the game object</param>
    /// <param name="scale">The local scale of the game object</param>
    private void DrawRouteObject(GameObject _g, Vector3 pos, Vector3 scale)
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

    /// <summary>
    /// Generates a graph. A graph shall not be generated if there is no large map, or if data has already been collected.
    /// Once heightmap data has been collected, graph paremeters shall be derieved.
    /// The layer of the holomap chunks (the small map) shall also be set, in addition to the graph
    /// The graph shall be scanned once set, and data shall be considered collected after that point
    /// </summary>
    /// <returns></returns>
    public static IEnumerator GenerateGraph()
    {
        if (current.LargeMap != null && !current.DataCollected)
        {
            Debug.Log("Generating new graph");
            current.data = AstarPath.active.data;
            
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
                PathDisplayV2.SetMapHeight(scanHeight);

                //current.StartCoroutine(current.SetHolomap());
                current.StartCoroutine(current.SetGridGraph(graphSize/2, nodeSize*2, scanHeight));

                while(!current.GraphSet || !heightMapData.IsGenerated)
                {
                    yield return new WaitForSeconds(1 / current.updatesPerSecond);
                }

                AstarPath.active.Scan(current.data.graphs);
                //consider culling nodes with no neighbors
                current.DataCollected = !current.DataCollected;
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
    IEnumerator SetGridGraph(int size, float nodeSize, float scanHeight)
    {
        GridGraph graph = data.AddGraph(typeof(GridGraph)) as GridGraph;

        graph.center = LargeMap.transform.position;
        graph.neighbours = NumNeighbours.Six;
        graph.maxClimb = 0f;

        graph.SetDimensions(size, size, nodeSize);
        graph.collision.fromHeight = scanHeight * scanFactor;
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
    IEnumerator SetHolomap()
    {
        MeshFilter[] meshes = SmallMap.GetComponentsInChildren<MeshFilter>();
        while(meshes.Length < 1)
        {
            yield return new WaitForSeconds(1 / updatesPerSecond);
            meshes = SmallMap.GetComponentsInChildren<MeshFilter>();
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
    IEnumerator DrawMapCurveV2()
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

        myPath = ABPath.FakePath(posList, nodeList);
        BezierTrace();
        
        
    }

    /// <summary>
    /// Resets and then traces over the BezierSpline mySpline based on the ABPath created from DrawMapCurve
    /// </summary>
    private void BezierTrace()
    {
        BezierSpline _bs = mySpline;
        if (_bs != null) _bs.Reset();
        if (myPath != null && _bs != null)
        {
            List<Vector3> posNodes = myPath.vectorPath;
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
            mySpline = _bs;
        }
        PathDisplayV2.DisplayCheck(mySpline.Length);
        DrawPath = false;
    }

    #endregion

    #region STATIC_FUNCTIONS

    /// <summary>
    /// Adds a marker at the end of the linkedObj list. 
    /// If the linkedObj count is greater than the current route segment length, additional route segments shall be pooled
    /// </summary>
    /// <param name="_t">The transform of the marker game object to be added</param>
    public static void AddRouteMarker(Transform _t)
    {
        current.linkedObj.Add(_t);
        current.nodeCount++;
        current.DonePooling = false;
        if (current.nodeCount >= current.routeMarkerPool.Count)
        {
            Debug.Log("Instantiating new batch");
            
            current.GenerateRoutePool(current.batchSize);
        } else
        {
            current.DonePooling = true;
        }
        current.UpdateRouteV2(current.linkedObj.Count - 1, false);
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
        int ndx = (_target != null) ? current.linkedObj.IndexOf(_target) : -1;
        if(ndx < 0)
        {
            AddRouteMarker(_t);
        } else
        {
            current.linkedObj.Insert(ndx + 1, _t);
            current.nodeCount++;
            current.DrawPath = true;
            if (current.linkedObj.Count >= current.routeMarkerPool.Count)
            {
                Debug.Log("Instantiating new batch");
                current.DonePooling = false;
                current.GenerateRoutePool(current.batchSize);
            }
            while (!current.DonePooling) ;
            current.Reinsertion(current.routeConnectPool.Count - 1, ndx + 1);
            current.UpdateRouteV2(ndx + 1, false);
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
        if (!current.linkedObj.Contains(_t)) return false;
        int actionNdx = current.linkedObj.IndexOf(_t);
        if (actionNdx > -1)
        {
            current.Reinsertion(actionNdx, current.linkedObj.Count);
            current.linkedObj.Remove(_t);
            current.nodeCount--;
            if (current.nodeCount < 2) PathDisplayV2.ClearPath();
            current.DrawPath = true;
            current.UpdateRouteV2(actionNdx - 1, false);
            return true;
        }
        else
        {
            return false;
        }
    }

    private void Reinsertion(int removeFrom, int insertAt)
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
    public static int NodeCount { get { return current.nodeCount; } }

    /// <summary>
    /// Clears the current route and purges the linkedObj list
    /// </summary>
    public static void ClearRoute()
    {
        current.linkedObj.Clear();
        current.nodeCount = 0;
        PathDisplayV2.ClearPath();
        current.DrawPath = true;
    }

    /// <summary>
    /// Clears the graph data currently used by the route display
    /// </summary>
    public static void ClearMeshData()
    {
        foreach (NavGraph graph in current.data.graphs)
        {
            current.data.RemoveGraph(graph);
        }
        current.DataCollected = false;
        //current.GroundSet = false;
        current.GraphSet = false;
        current.DrawPath = false;
    }

    /// <summary>
    /// Copies the current spline to the old spline, and displays the path using the old spline
    /// </summary>
    public static void ShowPath()
    {
        if(current.linkedObj.Count > 1)
        {
            current.StartCoroutine(HidePath());
            current.DrawPath = true;
            current.StartCoroutine(current.DrawMapCurveV2());
            while (current.DrawPath) { Debug.Log("Waiting on path draw"); }
            current.oldSpline.Copy(current.mySpline);
            current.StartCoroutine(PathDisplayV2.DrawPath(current.oldSpline));
        }
    }

    /// <summary>
    /// Hides the straight route for path drawing purposes
    /// </summary>
    /// <returns></returns>
    static IEnumerator HidePath()
    {
        foreach(GameObject _g in current.routeConnectPool)
        {
            _g.SetActive(false);
        }
        foreach(GameObject _g in current.smallConnectPool)
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
        current.routeMarkerPool.Add(_myGameObject);
        _myGameObject.SetActive(false);
    }

    /// <summary>
    /// Instantiates a large route segment within ASL space
    /// </summary>
    /// <param name="_myGameObject">The game object that initiated this callback</param>
    private static void RouteInstantiation(GameObject _myGameObject)
    {
        current.routeConnectPool.Add(_myGameObject);
        _myGameObject.SetActive(false);
    }

    /// <summary>
    /// Instantiates a small route segment within ASL space
    /// </summary>
    /// <param name="_myGameObject">The game object that inititated this callback</param>
    private static void SmallRouteInstantiation(GameObject _myGameObject)
    {
        current.smallConnectPool.Add(_myGameObject);
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
