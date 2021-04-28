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
    public int batchSize = 20;
    
    private List<GameObject> routeMarkerPool = new List<GameObject>();
    private List<GameObject> routeConnectPool = new List<GameObject>();
    private List<GameObject> smallConnectPool = new List<GameObject>();

    private List<Transform> linkedObj = new List<Transform>();
    int removedNdx = -1;

    //Map references
    public Transform MapDisplay = null;
    private GameObject SmallMap;
    private GameObject LargeMap;

    //Data used in MapPath functions
    private AstarData data;
    private ABPath myPath;
    //private float

    private bool DataCollected = false, DrawNewCurve = false;

    //Would prefer to fetch this from the player once instantiated
    protected Color myColor;

    private void Awake()
    {
        current = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        LargeMap = GameObject.FindWithTag("LargeMap");
        SmallMap = GameObject.FindWithTag("SmallMap");

        if(current.gameObject.GetComponent<ASLObject>() != null)
        {
            current.gameObject.GetComponent<ASLObject>()._LocallySetFloatCallback(SyncLists);
        }
        myColor = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f), .25f);
        GenerateRoutePool(batchSize);
        StartCoroutine(ReceiveMeshData());
        StartCoroutine(UpdateRoutePositions());
    }

    IEnumerator UpdateRoutePositions()
    {
        while (true)
        {
            yield return new WaitForSeconds(1 / updatesPerSecond);
            UpdateRoute();
        }
    }

    private void GenerateRoutePool(int toAdd)
    {
        for(int i = 0; i < toAdd; i++)
        {
            ASLHelper.InstantiateASLObject("MinimapMarker_RouteNode", new Vector3(0, 0, 0), Quaternion.identity, "", "", MarkerInstantiation);
            ASLHelper.InstantiateASLObject("MinimapMarker_RoutePath", new Vector3(0, 0, 0), Quaternion.identity, "", "", RouteInstantiation);
            ASLHelper.InstantiateASLObject("MinimapMarker_RoutePath", new Vector3(0, 0, 0), Quaternion.identity, "", "", SmallRouteInstantiation);
        }
    }

    private void GeneratePathPool()
    {

    }

    #region DRAW_STRAIGHT_ROUTE

    //Update route coroutine split for readability
    void UpdateRoute()
    {
        GameObject curNode, curPath, smPath;
        Vector3 scale, dir, pos;
        int ndx;
        float length;

        for(ndx = 0; ndx < linkedObj.Count; ndx++)
        {
            curNode = routeMarkerPool[ndx];
            curPath = routeConnectPool[ndx];
            smPath = smallConnectPool[ndx];

            scale = new Vector3(1.5f, 0.5f, 1.5f);
            pos = linkedObj[ndx].position + heightAboveMarker * Vector3.up;
            DrawRoute(curNode, pos, scale);

            if (ndx < linkedObj.Count - 1)
            {
                dir = linkedObj[ndx + 1].position - linkedObj[ndx].position;
                length = (dir).magnitude / 2f;
                scale = new Vector3(.25f, length, .25f);
                pos = pos + (length * dir.normalized);
                curPath.transform.up = dir;
                

                DrawRoute(curPath, pos, scale);

                int scaleFactor = CalcSmallScale();
                smPath.transform.up = dir;
                if (MapDisplay != null)
                {
                    if (scaleFactor > 0)
                    {
                        pos = MapDisplay.position + ((pos - heightAboveMarker * Vector3.up) / scaleFactor);
                        scale = new Vector3(.05f, length / scaleFactor, .05f);
                    }
                    else
                    {
                        pos = Vector3.zero;
                        scale= Vector3.zero;
                    }
                }
                else
                {
                    if (scaleFactor > 0)
                    {
                        pos = SmallMap.transform.position + ((pos - heightAboveMarker * 0.5f * Vector3.up) / scaleFactor);
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

            } else
            {
                DrawRoute(curPath, Vector3.zero, Vector3.zero);
                DrawRoute(smPath, Vector3.zero, Vector3.zero);
            }
        }
        for(ndx = linkedObj.Count; ndx < routeMarkerPool.Count; ndx++)
        {
            DrawRoute(routeMarkerPool[ndx], Vector3.zero, Vector3.zero);
            DrawRoute(routeConnectPool[ndx], Vector3.zero, Vector3.zero);
            DrawRoute(smallConnectPool[ndx], Vector3.zero, Vector3.zero);
        }
    }

    private int CalcSmallScale()
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

    //Intended to replace the SendAndSetClaim blocks used to draw the routes, but causes route segments to pile on each other sometimes
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

    #region PATHFINDING

    IEnumerator ReceiveMeshData()
    {
        if (LargeMap != null && !DataCollected)
        {
            Debug.Log("Generating new graph");
            data = AstarPath.active.data;
            NavMeshGraph graph;
            
            MeshFilter[] meshes = LargeMap.GetComponentsInChildren<MeshFilter>();
            while (meshes.Length < 1)
            {
                yield return new WaitForSeconds(0.1f);
                meshes = LargeMap.GetComponentsInChildren<MeshFilter>();
            }
            Debug.Log("Chunk count: " + meshes.Length);
            int ndx = 0;
            while (ndx < meshes.Length)
            {
                Debug.Log("Adding chunk graph");
                graph = data.AddGraph(typeof(NavMeshGraph)) as NavMeshGraph;
                graph.sourceMesh = meshes[ndx].sharedMesh;
                graph.offset = meshes[ndx].transform.position;
                ndx++;
            }
            
            AstarPath.active.Scan(current.data.graphs);
            DataCollected = !DataCollected;
        }
        yield return new WaitForSeconds(0.1f);
    }

    public static void ClearMeshData()
    {
        foreach(NavGraph graph in current.data.graphs)
        {
            current.data.RemoveGraph(graph);
        }
        current.DataCollected = false;
    }

    private void DrawMapCurve()
    {
        if (DrawNewCurve && DataCollected && data.graphs[0] != null)
        {
            List<Vector3> posList = new List<Vector3>();
            List<GraphNode> nodeList = new List<GraphNode>();
            List<GraphNode> connections = new List<GraphNode>();

            Vector3 targetPos, curPos, dir;
            NavGraph curGraph = data.graphs[0];
            GraphNode curNode, prevNode;

            for (int ndx = 0; ndx < linkedObj.Count - 1; ndx++)
            {                
                targetPos = linkedObj[ndx + 1].position;
                float distCheck = float.PositiveInfinity, curDist;

                if (ndx == 0)
                {
                    //Find graph nearest position
                    foreach (NavGraph graphCheck in data.graphs)
                    {
                        curDist = ((Vector3)graphCheck.GetNearest(linkedObj[ndx].position).node.position - linkedObj[ndx].position).magnitude;
                        if (curDist < distCheck)
                        {
                            curDist = distCheck;
                            curGraph = graphCheck;
                        }
                    }
                    //Find node on graph nearest position
                    curNode = curGraph.GetNearest(linkedObj[ndx].position).node;
                    prevNode = null;
                    curPos = (Vector3)curNode.position;
                    dir = (targetPos - curPos);

                    posList.Add(curPos);
                    nodeList.Add(curNode);
                } else
                {
                    //Else curNode and curPos are the last items added to their respective lists
                    curNode = nodeList[nodeList.Count - 1];
                    curPos = posList[posList.Count - 1];
                    curGraph = curNode.Graph;

                    //if previous node in list and curNode are in the same graph, prevNode = previous node, otherwise is null
                    if (nodeList.Count > 1)
                    {
                        if (curNode.GraphIndex.Equals(nodeList[nodeList.Count - 2].GraphIndex))
                        {
                            prevNode = nodeList[nodeList.Count - 2];
                        } else
                        {
                            prevNode = null;
                        }
                    } else
                    {
                        prevNode = null;
                    }

                    if(prevNode != null)
                    {
                        foreach (NavGraph graphCheck in data.graphs)
                        {
                            //If dist from nearest node to cur pos
                            //Add each graph that is not curGraph to tempGraphList
                        }
                    }                    
                }
                

                    //while we haven't reached the target position
                        //get the list of connected nodes
                        //remove nodes with too large a height diff (abs value), starting with the largest unless i have 1 node
                        //remove the previous node (if applicable) 

                        //add low-energy node with largest dot product with dir
                        //prevNode = curNode, curNode = node added
                        //recalculate dir 

                        //is there a similar position in an adjacent graph? (distance < some value from current pos)
                            //if there is, get those nodes and their corresponding graphs (not cur graph)
                            //calculate dir from each node
                            //replace curNode with node that has smallest dot product, and replace curGraph with the corresponding graph; prevNode = null
                                //additional check if distance to target from curNode is shorter than the node on the different graph

                
                    
                    //repeat while block
            }
            DrawNewCurve = !DrawNewCurve;
        }
    }

    bool GetNextGraph(ref NavGraph curGraph, ref GraphNode curNode, ref Vector3 curPos, Vector3 target)
    {
        List<NavGraph> tempGraph = new List<NavGraph>();
        List<GraphNode> tempNode = new List<GraphNode>();

        foreach (NavGraph graphCheck in data.graphs)
        {
            if (!graphCheck.graphIndex.Equals(curGraph.graphIndex))
            {
                //If dist from nearest node to cur pos
                    //Add each graph that is not curGraph to tempGraphList
            }
        }

        return false;
    }

    #endregion

    #region STATIC_MUTATORS

    public static void AddRouteMarker(Transform _t)
    {
        current.linkedObj.Add(_t);
        current.DrawNewCurve = true;
        if(current.linkedObj.Count > current.routeMarkerPool.Count)
        {
            Debug.Log("Instantiating new batch");
            current.GenerateRoutePool(current.batchSize);
        }
    }

    public static void RemoveRouteMarker(Transform _t, bool fromFloatCallback)
    {
        current.removedNdx = current.linkedObj.IndexOf(_t);
        if (current.removedNdx > -1)
        {
            GameObject nodeToRemove = current.routeMarkerPool[current.removedNdx];
            GameObject pathToRemove = current.routeConnectPool[current.removedNdx];
            GameObject smallToRemove = current.smallConnectPool[current.removedNdx];
            current.linkedObj.Remove(_t);
            current.routeMarkerPool.RemoveAt(current.removedNdx);
            current.routeConnectPool.RemoveAt(current.removedNdx);
            current.smallConnectPool.RemoveAt(current.removedNdx);
            current.routeMarkerPool.Add(nodeToRemove);
            current.routeConnectPool.Add(pathToRemove);
            current.smallConnectPool.Add(smallToRemove);
        }
        else
        {
            if (!fromFloatCallback && current.gameObject.GetComponent<ASLObject>() != null)
            {
                current.PrepSearchCallback(_t.gameObject.GetComponent<ASLObject>().m_Id);
            }
        }
    }

    public static void ClearRoute()
    {
        foreach (GameObject g in current.routeConnectPool)
        {
            g.GetComponent<ASLObject>().SendAndSetClaim(() =>
            {
                g.GetComponent<ASLObject>().SendAndSetWorldPosition(Vector3.zero);
                g.GetComponent<ASLObject>().SendAndSetLocalScale(Vector3.zero);
            });
        }
        foreach (GameObject g in current.routeMarkerPool)
        {
            g.GetComponent<ASLObject>().SendAndSetClaim(() =>
            {
                g.GetComponent<ASLObject>().SendAndSetWorldPosition(Vector3.zero);
                g.GetComponent<ASLObject>().SendAndSetLocalScale(Vector3.zero);
            });
        }
        foreach (GameObject g in current.smallConnectPool)
        {
            g.GetComponent<ASLObject>().SendAndSetClaim(() =>
            {
                g.GetComponent<ASLObject>().SendAndSetWorldPosition(Vector3.zero);
                g.GetComponent<ASLObject>().SendAndSetLocalScale(Vector3.zero);
            });
        }
        current.linkedObj.Clear();
    }

    #endregion

    #region CALLBACK_FUNCTIONS

    private static void MarkerInstantiation(GameObject _myGameObject)
    {
        current.routeMarkerPool.Add(_myGameObject);
        _myGameObject.GetComponent<ASLObject>().SendAndSetClaim(() =>
        {
            _myGameObject.GetComponent<ASLObject>().SendAndSetWorldPosition(Vector3.zero);
            _myGameObject.GetComponent<ASLObject>().SendAndSetLocalScale(Vector3.zero);
            _myGameObject.GetComponent<ASLObject>().SendAndSetObjectColor(current.myColor, current.myColor);
        });
        Debug.Log("Added marker");
    }

    private static void RouteInstantiation(GameObject _myGameObject)
    {
        current.routeConnectPool.Add(_myGameObject);
        _myGameObject.GetComponent<ASLObject>().SendAndSetClaim(() =>
        {
            _myGameObject.GetComponent<ASLObject>().SendAndSetWorldPosition(Vector3.zero);
            _myGameObject.GetComponent<ASLObject>().SendAndSetLocalScale(Vector3.zero);
            _myGameObject.GetComponent<ASLObject>().SendAndSetObjectColor(current.myColor, current.myColor);
        });
    }

    private static void SmallRouteInstantiation(GameObject _myGameObject)
    {
        current.smallConnectPool.Add(_myGameObject);
        _myGameObject.GetComponent<ASLObject>().SendAndSetClaim(() =>
        {
            _myGameObject.GetComponent<ASLObject>().SendAndSetWorldPosition(Vector3.zero);
            _myGameObject.GetComponent<ASLObject>().SendAndSetLocalScale(Vector3.zero);
            _myGameObject.GetComponent<ASLObject>().SendAndSetObjectColor(current.myColor, current.myColor);
        });
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
