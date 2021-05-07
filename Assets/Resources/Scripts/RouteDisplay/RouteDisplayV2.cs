using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using ASL;

public class RouteDisplayV2 : MonoBehaviour
{
    public static RouteDisplayV2 current;

    public float updatesPerSecond = 10f;
    public float heightAboveMarker = 5f;
    public int batchSize = 10;
    
    private List<GameObject> routeMarkerPool = new List<GameObject>();
    private List<GameObject> routeConnectPool = new List<GameObject>();
    private List<GameObject> smallConnectPool = new List<GameObject>();
    //private List<SplineWalker> pathDisplayPool = new List<SplineWalker>();

    private List<Transform> linkedObj = new List<Transform>();
    int removedNdx = -1;

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
        GroundSet = false, 
        GraphSet = false,
        DrawPath = false;

    public BezierSpline mySpline = null, oldSpline = null;

    private const int gridRes = 16;    //Node resolution of the graph
    [Range(0.1f,10f)]
    public float traceSpeed = 0.3f;    //inverse Approximate time to trace 3 path nodes

    //Would prefer to fetch this from the player once instantiated
    private Color myColor;

    private void Awake()
    {
        current = this;
        MyController.Initialize();
    }

    // Start is called before the first frame update
    void Start()
    {
        LargeMap = GameObject.FindWithTag("SpawnLargerMap");
        SmallMap = GameObject.FindWithTag("SpawnSmallMap");

        if(current.gameObject.GetComponent<ASLObject>() != null)
        {
            current.gameObject.GetComponent<ASLObject>()._LocallySetFloatCallback(SyncLists);
        }
        myColor = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f), .25f);
        GenerateRoutePool(batchSize);
        StartCoroutine(PathDisplay.Initialize(myColor, traceSpeed, updatesPerSecond));
        StartCoroutine(PathDisplay.GeneratePathPool(batchSize));
        StartCoroutine(PathDisplay.Render());

        StartCoroutine(GenerateGraph());
        StartCoroutine(UpdateRoute());
        StartCoroutine(DrawMapCurve());
    }

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

    public Color GetColor()
    {
        return myColor;
    }

    #region DRAW_DIRECT_ROUTE

    //High-level route update
    IEnumerator UpdateRoute()
    {
        GameObject curNode, curPath, smPath;
        Vector3 scale, dir, pos, nextPos;
        int ndx;
        float length; 
        while (true)
        {
            while (!DonePooling) yield return new WaitForSeconds(0.1f);
            yield return new WaitForSeconds(1f / updatesPerSecond);

            for (ndx = 0; ndx < linkedObj.Count; ndx++)
            {
                curNode = routeMarkerPool[ndx];
                curPath = routeConnectPool[ndx];
                smPath = smallConnectPool[ndx];

                curNode.SetActive(true);
                curPath.SetActive(true);
                smPath.SetActive(true);

                scale = new Vector3(1.5f, 0.5f, 1.5f);
                pos = linkedObj[ndx].position;// + PrevHeight;// * Vector3.up;
                pos.y += heightAboveMarker;
                DrawRoute(curNode, pos, scale);

                if (ndx < linkedObj.Count - 1)
                {
                    nextPos = linkedObj[ndx + 1].position;
                    nextPos.y += heightAboveMarker;
                    dir = nextPos - pos;
                    length = (dir).magnitude / 2f;
                    scale = new Vector3(.25f, length, .25f);
                    pos = pos + (length * dir.normalized);
                    curPath.transform.up = dir;
                    DrawRoute(curPath, pos, scale);

                    float scaleFactor = CalcSmallScale();
                    smPath.transform.up = dir;
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
                            //- smMap.transform.right / scaleFactor - smMap.transform.right / smMap.GetComponent<GenerateMapFromHeightMap>().mapSize;
                            scale = new Vector3(0.01f, length / scaleFactor, 0.01f);
                        }
                        else
                        {
                            pos = Vector3.zero;
                            scale = Vector3.zero;
                        }
                    }
                    DrawRoute(smPath, pos, scale);

                }
                else
                {
                    curPath.SetActive(false);
                    smPath.SetActive(false);
                }
            }
            for (ndx = linkedObj.Count; ndx < routeMarkerPool.Count; ndx++)
            {
                routeMarkerPool[ndx].SetActive(false);
                routeConnectPool[ndx].SetActive(false);
                smallConnectPool[ndx].SetActive(false);
            }
        }
        
    }

    //Get map small scale
    private float CalcSmallScale()
    {
        if (TryGetComponent(out MarkerDisplay _md))
        {
            return MarkerDisplay.GetScaleFactor();
        }
        else
        {
            if (SmallMap == null || LargeMap == null) return -1;

            GenerateMapFromHeightMap _sm = SmallMap.GetComponent<GenerateMapFromHeightMap>();
            GenerateMapFromHeightMap _lg = LargeMap.GetComponent<GenerateMapFromHeightMap>();
            if (_sm == null || _lg == null) return -1;

            return _lg.mapSize / _sm.mapSize;
        }
    }

    //Set route object transform
    private void DrawRoute(GameObject _g, Vector3 pos, Vector3 scale)
    {
        _g.GetComponent<ASLObject>().SendAndSetClaim(() =>
        {
            _g.GetComponent<ASLObject>().SendAndSetWorldPosition(pos);
            _g.GetComponent<ASLObject>().SendAndSetLocalScale(scale);
            _g.GetComponent<ASLObject>().SendAndSetLocalRotation(_g.transform.localRotation);
        });
    }

    #endregion

    #region SCAN_GRAPH

    //High level graph generation
    IEnumerator GenerateGraph()
    {
        if (LargeMap != null && !DataCollected)
        {
            Debug.Log("Generating new graph");
            data = AstarPath.active.data;
            
            GenerateMapFromHeightMap heightMapData = LargeMap.GetComponent<GenerateMapFromHeightMap>();

            while (heightMapData == null)
            {
                yield return new WaitForSeconds(1 / updatesPerSecond);
                heightMapData = LargeMap.GetComponent<GenerateMapFromHeightMap>();
            }

            if(heightMapData.heightmap!= null && heightMapData.heightmap.width >= 32 && heightMapData.heightmap.width % 2 == 0)
            {
                int graphSize = heightMapData.heightmap.width / 2;
                float nodeSize = (float)heightMapData.mapSize / graphSize;
                nodeSize = (nodeSize > 0) ? nodeSize : 1;
                float scanHeight = heightMapData.meshHeight;

                StartCoroutine(SetGround());
                StartCoroutine(SetHolomap());
                StartCoroutine(SetGridGraph(2 * graphSize, nodeSize / 2, scanHeight));

                while(!GraphSet || !GroundSet)
                {
                    yield return new WaitForSeconds(1 / updatesPerSecond);
                }

                AstarPath.active.Scan(data.graphs);
                //consider culling nodes with no neighbors
                DataCollected = !DataCollected;
            } else
            {
                Debug.LogError("Large map does not have valid heightmap set.", LargeMap);
            }            
        }
        yield return new WaitForSeconds(0.1f);
    }

    //Set graph data
    IEnumerator SetGridGraph(int size, float nodeSize, float scanHeight)
    {
        GridGraph graph = data.AddGraph(typeof(GridGraph)) as GridGraph;

        graph.center = LargeMap.transform.position;
        graph.neighbours = NumNeighbours.Six;
        graph.maxClimb = 0f;

        graph.SetDimensions(size, size, nodeSize);
        graph.collision.fromHeight = scanHeight * scanFactor;
        graph.collision.heightMask = LayerMask.GetMask("Ground");

        //Set penalties
        graph.penaltyAngle = true;
        graph.penaltyPosition = true;

        GraphSet = true;
        yield return new WaitForSeconds(0.1f);
    }

    //Set large map to ground layer
    IEnumerator SetGround()
    {
        MeshFilter[] meshes = LargeMap.GetComponentsInChildren<MeshFilter>();
        while (meshes.Length < 1)
        {
            yield return new WaitForSeconds(1 / updatesPerSecond);
            meshes = LargeMap.GetComponentsInChildren<MeshFilter>();
        }
        Debug.Log("Setting " + meshes.Length + " chunks to Ground layer");
        int ndx = 0;
        while (ndx < meshes.Length)
        {
            meshes[ndx].gameObject.layer = LayerMask.NameToLayer("Ground");
            ndx++;
        }
        GroundSet = true;
        yield return new WaitForSeconds(0.1f);
    }

    //Set small map to holomap layer
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

    //Get path from grid
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

    //Trace spline in preparation for display
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
                for (ndx = 3; ndx * 2 < posNodes.Count; ndx += 3)
                {
                    _bs.SetCurvePoint(ndx, posNodes[ndx * 2]);
                }
                if(posNodes.Count % 3 != 0 && ((_bs.GetLastControlPoint() - posNodes[posNodes.Count - 1]).magnitude > 0.5f))
                {
                    _bs.SetCurvePoint(ndx, posNodes[posNodes.Count - 1]);
                }
            }
            mySpline = _bs;
        }
    }

    #endregion

    #region STATIC_FUNCTIONS

    //Add node at end
    public static void AddRouteMarker(Transform _t)
    {
        current.linkedObj.Add(_t);
        current.DrawPath = true;
        current.DonePooling = false;
        if (current.linkedObj.Count > current.routeMarkerPool.Count)
        {
            Debug.Log("Instantiating new batch");
            
            current.GenerateRoutePool(current.batchSize);
        } else
        {
            current.DonePooling = true;
        }
    }

    //Route insertion
    public static int InsertMarkerAt(Transform _target, Transform _t)
    {
        int ndx = (_target != null) ? current.linkedObj.IndexOf(_target) : -1;
        if(ndx < 0)
        {
            AddRouteMarker(_t);
        } else
        {
            current.linkedObj.Insert(ndx, _t);
            current.DrawPath = true;
            if (current.linkedObj.Count > current.routeMarkerPool.Count)
            {
                Debug.Log("Instantiating new batch");
                current.DonePooling = false;
                current.GenerateRoutePool(current.batchSize);
            }
        }
        return ndx;
    }

    //Remove route node
    public static bool RemoveRouteMarker(Transform _t, bool fromFloatCallback)
    {
        current.removedNdx = current.linkedObj.IndexOf(_t);
        if (current.removedNdx > -1)
        {
            GameObject nodeToRemove = current.routeMarkerPool[current.removedNdx];
            GameObject pathToRemove = current.routeConnectPool[current.removedNdx];
            GameObject smallToRemove = current.smallConnectPool[current.removedNdx];

            current.linkedObj.Remove(_t);
            if (current.linkedObj.Count < 2) PathDisplay.ClearNotRender();

            current.routeMarkerPool.RemoveAt(current.removedNdx);
            current.routeConnectPool.RemoveAt(current.removedNdx);
            current.smallConnectPool.RemoveAt(current.removedNdx);

            current.routeMarkerPool.Add(nodeToRemove);
            current.routeConnectPool.Add(pathToRemove);
            current.smallConnectPool.Add(smallToRemove);
            nodeToRemove.SetActive(false);
            pathToRemove.SetActive(false);
            smallToRemove.SetActive(false);
            current.DrawPath = true;

            if (fromFloatCallback)
            {
                PlayerMarkerGenerator.RemoveMarker(_t.gameObject);
            }

            return true;
        }
        else
        {
            if (!fromFloatCallback && current.gameObject.GetComponent<ASLObject>() != null)
            {
                current.PrepSearchCallback(_t.gameObject.GetComponent<ASLObject>().m_Id);
            }

            return false;
        }
    }

    //Delete route
    public static void ClearRoute()
    {
        current.linkedObj.Clear();
        PathDisplay.ClearNotRender();
        current.DrawPath = true;
    }

    //Clear map data
    public static void ClearMeshData()
    {
        foreach (NavGraph graph in current.data.graphs)
        {
            current.data.RemoveGraph(graph);
        }
        current.DataCollected = false;
        current.GroundSet = false;
        current.GraphSet = false;
        current.DrawPath = false;
    }

    //Change path being tracked
    public static void ShowPath()
    {
        if(current.linkedObj.Count > 1)
        {
            current.oldSpline.Copy(current.mySpline);
            PathDisplay.SetSpline(current.oldSpline);
        }
    }

    #endregion

    #region CALLBACK_FUNCTIONS

    //Create route note
    private static void MarkerInstantiation(GameObject _myGameObject)
    {
        current.routeMarkerPool.Add(_myGameObject);
        _myGameObject.GetComponent<ASLObject>().SendAndSetClaim(() =>
        {
            _myGameObject.GetComponent<ASLObject>().SendAndSetObjectColor(current.myColor, current.myColor);
        });
        _myGameObject.SetActive(false);
        Debug.Log("Added marker");
    }

    //Create route on large map
    private static void RouteInstantiation(GameObject _myGameObject)
    {
        current.routeConnectPool.Add(_myGameObject);
        _myGameObject.GetComponent<ASLObject>().SendAndSetClaim(() =>
        {
            _myGameObject.GetComponent<ASLObject>().SendAndSetObjectColor(current.myColor, current.myColor);
        });
        _myGameObject.SetActive(false);
    }

    //Create route on small map
    private static void SmallRouteInstantiation(GameObject _myGameObject)
    {
        current.smallConnectPool.Add(_myGameObject);
        _myGameObject.GetComponent<ASLObject>().SendAndSetClaim(() =>
        {
            _myGameObject.GetComponent<ASLObject>().SendAndSetObjectColor(current.myColor, current.myColor);
        });
        _myGameObject.SetActive(false);
    }


    //For reference in the event we need to pass ASLObject ids (which are strings) to the other players
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


    //Locally set float callback method
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

    //Reassemble ASLObject id from float array
    private string AssembleID(float[] f_id)
    {
        char[] c_id = new char[f_id.Length];
        for (int i = 0; i < c_id.Length; i++)
        {
            c_id[i] = (char)f_id[i];
        }
        return string.Concat(c_id);
    }

    #endregion
}
