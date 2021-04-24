using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class MinimapDisplay : MonoBehaviour
{
    public static MinimapDisplay current;

    public float updatesPerSecond = 10f;
    public GameObject routeNodeMarker = null;
    public GameObject routePathMarker = null;

    private List<GameObject> routeMarkerPool = new List<GameObject>();
    private List<GameObject> linkedTransform = new List<GameObject>();
    public List<GameObject> routeConnectPool = new List<GameObject>();
    private GameObject nextMarker = null;
    private GameObject nextRoute = null;
    private Vector3 nextPos, nextRoutePos, nextScale, nextDir;
    private string nextId;

    bool RouteChanged = false;
    bool NodeRemoved = false;

    private void Awake()
    {
        current = this;
    }


    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(routeNodeMarker != null, "Please set " + routeNodeMarker + " in the inspector.");
        Debug.Assert(routePathMarker != null, "Please set " + routePathMarker + " in the inspector.");

        gameObject.GetComponent<ASLObject>()._LocallySetFloatCallback(SyncLists);
        StartCoroutine(UpdateRoutePositions());
    }

    public void SyncLists(string _id, float[] _f)
    {
        float[] copy = new float[_f.Length - 1];
        System.Array.ConstrainedCopy(_f, 1, copy, 0, copy.Length);
        string theID = current.AssembleID(copy);
        List<Transform> transforms = ASLObjectTrackingSystem.GetObjects();

        switch (_f[1])
        {
            case 0:
                Debug.Log("Adding object: " + theID);
                //Add to list
                bool isPresent = false;
                foreach (GameObject obj in routeMarkerPool)
                {
                    if(obj.GetComponent<ASLObject>().m_Id.Equals(theID))
                    {
                        isPresent = true;
                    }
                }
                if (!isPresent)
                {
                    Debug.Log("Object not yet tracked");
                    GameObject toAdd = null;
                    foreach (Transform t in transforms)
                    {
                        Debug.Log(t.gameObject.GetComponent<ASLObject>().m_Id);
                        if (t.gameObject.GetComponent<ASLObject>().m_Id.Equals(theID))
                        {
                            toAdd = t.gameObject;
                        }
                    }
                    /*if (toAdd != null)
                    {
                        routeMarkerPool.Add(toAdd);
                        Debug.Log("Now tracking object");
                    }
                    else
                    {
                        Debug.Log("Already tracking object");
                    }*/
                }
                break;
            case 1:
                //Remove from list
                int removeNdx = -1;
                break;
                
            
        }
    }

    private string AssembleID(float[] f_id)
    {
        char[] c_id = new char[f_id.Length];
        for (int i = 0; i < c_id.Length; i++)
        {
            c_id[i] = (char)f_id[i];
        }
        return string.Concat(c_id);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }

    IEnumerator UpdateRoutePositions()
    {
        while (true)
        {
            yield return new WaitForSeconds(1 / updatesPerSecond);
            UpdateRouteMinimapMarkers();
        }
    }

    
    public static void AddRouteMarker(Transform _t)
    {
        current.linkedTransform.Add(_t.gameObject);
        current.nextPos = _t.position;
        current.nextPos.y = 10f;
        ASLHelper.InstantiateASLObject("MinimapMarker_RouteNode", new Vector3(0, 0, 0), Quaternion.identity, "", "", MarkerInstantiation);
              
        
        Debug.Log("Added marker");
    }

    public static void RemoveRouteMarker(Transform _t)
    {
        int ndx = current.linkedTransform.IndexOf(_t.gameObject);
        if(ndx > -1)
        {
            GameObject toRemove = current.routeMarkerPool[ndx];
            current.linkedTransform.Remove(_t.gameObject);
            current.routeMarkerPool.RemoveAt(ndx);
            ASLObjectTrackingSystem.RemoveObjectToTrack(toRemove.GetComponent<ASLObject>());
            toRemove.GetComponent<ASLObject>().SendAndSetClaim(() =>
            {
                toRemove.GetComponent<ASLObject>().DeleteObject();
            });

            current.RouteChanged = true;
            current.NodeRemoved = true;
        }        
    }

    private static void MarkerInstantiation(GameObject _myGameObject)
    {
        current.nextMarker = _myGameObject;
        current.routeMarkerPool.Add(_myGameObject);
        current.nextMarker.GetComponent<ASLObject>().SendAndSetClaim(() =>
        {
            current.nextMarker.GetComponent<ASLObject>().SendAndSetLocalPosition(current.nextPos);
            current.nextId = current.nextMarker.GetComponent<ASLObject>().m_Id;
        });
        ASLObjectTrackingSystem.AddObjectToTrack(_myGameObject.GetComponent<ASLObject>(), _myGameObject.transform);
        current.RouteChanged = true;

        /*current.gameObject.GetComponent<ASLObject>().SendAndSetClaim(() =>
        {
            //float[] Ids = new float[2];

            //Based on an answer to
            //https://stackoverflow.com/questions/5322056/how-to-convert-an-ascii-character-into-an-int-in-c/37736710#:~:text=A%20char%20value%20in%20C,it%20with%20(int)c%20.
            char[] splitId = current.nextId.ToCharArray();
            float[] Ids = new float[splitId.Length + 1];
            Ids[0] = 0;
            for(int i = 0; i < splitId.Length; i++)
            {
                Ids[i + 1] = (float)splitId[i];
            }
            current.gameObject.GetComponent<ASLObject>().SendFloatArray(Ids);
        });*/
    }

    private static void RouteInstantiation(GameObject _myGameObject)
    {
        current.nextRoute = _myGameObject;
        current.routeConnectPool.Add(_myGameObject);
        _myGameObject.transform.up = current.nextDir;
        current.nextRoute.GetComponent<ASLObject>().SendAndSetClaim(() =>
        {
            current.nextRoute.GetComponent<ASLObject>().SendAndSetWorldPosition(current.nextRoutePos);
            current.nextRoute.GetComponent<ASLObject>().SendAndSetLocalRotation(_myGameObject.transform.localRotation);
            current.nextRoute.GetComponent<ASLObject>().SendAndSetLocalScale(current.nextScale);
        });
        ASLObjectTrackingSystem.AddObjectToTrack(_myGameObject.GetComponent<ASLObject>(), _myGameObject.transform);
    }

    
    
    void UpdateRouteMinimapMarkers()
    {
        GameObject newPath;
        GameObject curNode, nextNode;

        float length;
        int ndx = 0;
        if (RouteChanged)
        {
            Debug.Log("Route changed");
            for (ndx = 0; ndx < routeMarkerPool.Count - 1; ndx++)
            {
                curNode = routeMarkerPool[ndx]; nextNode = routeMarkerPool[ndx + 1];

                nextDir = nextNode.transform.position - curNode.transform.position;
                length = (nextNode.transform.position - curNode.transform.position).magnitude / 2f;
                nextScale = new Vector3(.25f, length, .25f);
                nextRoutePos = curNode.transform.position + (length * nextDir.normalized);

                Debug.Log(routeConnectPool.Count < routeMarkerPool.Count - 1);
                if (routeConnectPool.Count < routeMarkerPool.Count - 1)
                {
                    Debug.Log("Creating new path");
                    ASLHelper.InstantiateASLObject("MinimapMarker_RoutePath", new Vector3(0, 0, 0), Quaternion.identity, "", "", RouteInstantiation);
                } else if (NodeRemoved)
                {
                Debug.Log("Changing existing path");
                GameObject theRoute = routeConnectPool[ndx];
                theRoute.transform.up = nextDir;
                theRoute.GetComponent<ASLObject>().SendAndSetClaim(() =>
                    {
                        theRoute.GetComponent<ASLObject>().SendAndSetWorldPosition(current.nextRoutePos);
                        theRoute.GetComponent<ASLObject>().SendAndSetLocalRotation(theRoute.transform.localRotation);
                        theRoute.GetComponent<ASLObject>().SendAndSetLocalScale(current.nextScale);
                    });
                    ASLObjectTrackingSystem.UpdateObjectTransform(theRoute.GetComponent<ASLObject>(), theRoute.transform);
                }
                Debug.Log(length);
            }

            /*GameObject toRemove;
            while (routeConnectPool.Count >= ndx && ndx != 0)
            {
                toRemove = routeConnectPool[ndx];
                int removeNdx = routeConnectPool.IndexOf(toRemove);
                routeConnectPool.RemoveAt(removeNdx);
                ASLObjectTrackingSystem.RemoveObjectToTrack(toRemove.GetComponent<ASLObject>());
                toRemove.GetComponent<ASLObject>().SendAndSetClaim(() =>
                {
                    toRemove.GetComponent<ASLObject>().DeleteObject();
                });
            }*/
            StartCoroutine(TrimRoute(ndx));

            RouteChanged = false;
            NodeRemoved = false;
        }
    }

    IEnumerator TrimRoute(int ndx)
    {
        int count = 0;
        foreach(GameObject g in routeConnectPool)
        {
            if (g == null) yield return new WaitForSeconds(0.1f);
            count++;
        }
        Debug.Log(count);
        Debug.Log(routeConnectPool.Count);
        StopCoroutine(TrimRoute(ndx));
    }

    private void DrawDirectRoute(Transform start, Transform end)
    {

    }
}


