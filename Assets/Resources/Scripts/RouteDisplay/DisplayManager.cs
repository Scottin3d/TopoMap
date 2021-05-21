using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class DisplayManager : MonoBehaviour
{
    public static DisplayManager _dm;

    public float updatesPerSecond = 10f;
    public float heightAboveMarker = 5f;
    public int batchSize = 10;

    public Transform MapDisplay = null;
    private static GameObject LargeMap;
    private static GameObject SmallMap;

    private static AstarData data;
    public float scanFactor = 2f;
    [Range(1, 5)]
    public int scaleFactor = 2;

    private static bool GraphSet = false;

    public BezierSpline mySpline = null, oldSpline = null;
    private static Coroutine drawCoroutine;
    private static Coroutine renderCoroutine;

    private Color myColor;

    /// <summary>
    /// Ensures that the player controller is working, should be moved out
    /// </summary>
    private void Awake()
    {
        _dm = this;
        MyController.Initialize();
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(mySpline != null);
        Debug.Assert(oldSpline != null);

        LargeMap = GameObject.FindWithTag("SpawnLargerMap");
        SmallMap = GameObject.FindWithTag("SpawnSmallMap");
        myColor = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f), .25f);

        RouteDisplayV2.Init(mySpline, LargeMap, SmallMap, myColor, updatesPerSecond, heightAboveMarker, batchSize);
        PathDisplayV2.Init(oldSpline, SmallMap, myColor, updatesPerSecond);

        StartCoroutine(SetHolomap());
        GraphHasChanged();
    }

    public Color GetColor() { return myColor; }

    #region CREATE_GRAPH

    /// <summary>
    /// Signals that the graph has been changed and must be rescanned
    /// </summary>
    public static void GraphHasChanged()
    {
        if(data != null)
        {
            foreach (NavGraph graph in data.graphs)
            {
                data.RemoveGraph(graph);
            }
        }
        GraphSet = false;
        _dm.StartCoroutine(GenerateGraph());
    }

    /// <summary>
    /// Generates a graph. A graph shall not be generated if there is no large map, or if data has already been collected.
    /// Once heightmap data has been collected, graph paremeters shall be derieved.
    /// The layer of the holomap chunks (the small map) shall also be set, in addition to the graph
    /// The graph shall be scanned once set, and data shall be considered collected after that point
    /// </summary>
    /// <returns></returns>
    public static IEnumerator GenerateGraph()
    {
        if (LargeMap != null)
        {
            Debug.Log("Generating new graph");
            data = AstarPath.active.data;

            GenerateMapFromHeightMap heightMapData = LargeMap.GetComponent<GenerateMapFromHeightMap>();

            while (heightMapData == null)
            {
                yield return new WaitForSeconds(1 / _dm.updatesPerSecond);
                heightMapData = LargeMap.GetComponent<GenerateMapFromHeightMap>();
            }

            if (heightMapData.heightmap != null && heightMapData.heightmap.width >= 32 && heightMapData.heightmap.width % 2 == 0)
            {
                int graphSize = heightMapData.heightmap.width / 2;
                float nodeSize = (float)heightMapData.mapSize / graphSize;
                nodeSize = (nodeSize > 0) ? nodeSize : 1;
                float scanHeight = heightMapData.meshHeight;

                //current.StartCoroutine(current.SetHolomap());
                _dm.StartCoroutine(SetGridGraph(graphSize / 2, nodeSize * 2, scanHeight));

                while (!GraphSet || !heightMapData.IsGenerated)
                {
                    yield return new WaitForSeconds(1 / _dm.updatesPerSecond);
                }

                AstarPath.active.Scan(data.graphs);
                //consider culling nodes with no neighbors
                RouteDisplayV2.ReceiveGraphData(data);
            }
            else
            {
                Debug.LogError("Large map does not have valid heightmap set.", LargeMap);
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

        graph.center = LargeMap.transform.position;
        graph.neighbours = NumNeighbours.Six;
        graph.maxClimb = 0f;

        graph.SetDimensions(size, size, nodeSize);
        graph.collision.fromHeight = scanHeight * _dm.scanFactor;
        graph.collision.heightMask = LayerMask.GetMask("Ground");
        //set obstacle layer

        //Set penalties
        graph.penaltyAngle = true;
        graph.penaltyPosition = true;

        GraphSet = true;
        yield return new WaitForSeconds(0.1f);
    }

    /// <summary>
    /// Sets the holomap (small map) colliders
    /// </summary>
    /// <returns></returns>
    static IEnumerator SetHolomap()
    {
        MeshFilter[] meshes = SmallMap.GetComponentsInChildren<MeshFilter>();
        while (meshes.Length < 1)
        {
            yield return new WaitForSeconds(1 / _dm.updatesPerSecond);
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

    #region PATH_ROUTE_DISPLAY

    /// <summary>
    /// Stops active draw and render coroutines.
    /// Hides the straight drawn route.
    /// Traces a path and gives that to mySpline
    /// Copies mySpline to oldSpline, and displays the path using oldSpline
    /// </summary>
    public static void ShowPath()
    {
        if(RouteDisplayV2.NodeCount > 1)
        {
            if (drawCoroutine != null) _dm.StopCoroutine(drawCoroutine);
            if (renderCoroutine != null) _dm.StopCoroutine(renderCoroutine);
            _dm.StartCoroutine(RouteDisplayV2.HideRoute());
            _dm.StartCoroutine(RouteDisplayV2.DrawMapCurveV2());
            while (RouteDisplayV2.IsDrawing) { Debug.Log("Waiting on path draw"); }
            PathDisplayV2.DisplayCheck(_dm.mySpline.Length);
            _dm.oldSpline.Copy(_dm.mySpline);
            drawCoroutine = _dm.StartCoroutine(PathDisplayV2.DrawPath());
        }
    }

    public static void ClearPath()
    {
        if (drawCoroutine != null) _dm.StopCoroutine(drawCoroutine);
        PathDisplayV2.ClearPath();
    }

    public static void DisplayToggle()
    {
        if (renderCoroutine != null) _dm.StopCoroutine(renderCoroutine);
        renderCoroutine = _dm.StartCoroutine(PathDisplayV2.ToggleRenderer());
        RouteDisplayV2.ToggleRoute();
    }

    public static void ResetDisplay()
    {
        if (drawCoroutine != null) _dm.StopCoroutine(drawCoroutine);
        if (renderCoroutine != null) _dm.StopCoroutine(renderCoroutine);
        _dm.StartCoroutine(RouteDisplayV2.HideRoute());
        RouteDisplayV2.ClearRoute();
        PathDisplayV2.ClearPath();
    }

    #endregion

}
