using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class MinimapDisplay : MonoBehaviour
{
    public static MinimapDisplay current;

    public float updatesPerSecond = 10f;
    public float heightAboveMarker = 5f;
    public GameObject routeNodeMarker = null;
    public GameObject routePathMarker = null;

    private List<GameObject> routeMarkerPool = new List<GameObject>();    
    public List<GameObject> routeConnectPool = new List<GameObject>();
    private List<GameObject> linkedTransform = new List<GameObject>();

    private GameObject nextMarker = null;
    private GameObject nextRoute = null;
    private Vector3 nextPos, nextRoutePos, nextScale, nextDir;
    int removedNdx = -1;

    Color myColor;

    private void Awake()
    {
        current = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        myColor = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f), .25f);
        Debug.Assert(routeNodeMarker != null, "Please set " + routeNodeMarker + " in the inspector.");
        Debug.Assert(routePathMarker != null, "Please set " + routePathMarker + " in the inspector.");
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

    //Update route coroutine split for readability
    void UpdateRoute()
    {
        GameObject curNode, nextNode;
        float length;

        for(int i = 0; i < routeConnectPool.Count; i++)
        {
            if(i+1 == routeMarkerPool.Count && routeConnectPool.Count != 0)
            {
                //Debug.Log("End of path");
                routeConnectPool[i].GetComponent<ASLObject>().SendAndSetClaim(() =>
                {
                    routeConnectPool[i].GetComponent<ASLObject>().SendAndSetWorldPosition(Vector3.zero);
                    routeConnectPool[i].GetComponent<ASLObject>().SendAndSetLocalScale(0.1f * Vector3.one);
                });
                //ASLObjectTrackingSystem.UpdateObjectTransform(routeConnectPool[i].GetComponent<ASLObject>(), routeConnectPool[i].transform);
            } else
            {
                curNode = routeMarkerPool[i]; nextNode = routeMarkerPool[i + 1];
                //Debug.Log("From:" + curNode.transform.position + " to " + nextNode.transform.position);
                nextDir = nextNode.transform.position - curNode.transform.position;
                length = (nextNode.transform.position - curNode.transform.position).magnitude / 2f;
                nextScale = new Vector3(.25f, length, .25f);
                nextRoutePos = curNode.transform.position + (length * nextDir.normalized);
                routeConnectPool[i].transform.up = nextDir;

                routeConnectPool[i].GetComponent<ASLObject>().SendAndSetClaim(() =>
                {
                    routeConnectPool[i].GetComponent<ASLObject>().SendAndSetWorldPosition(nextRoutePos);
                    routeConnectPool[i].GetComponent<ASLObject>().SendAndSetLocalScale(nextScale);
                    routeConnectPool[i].GetComponent<ASLObject>().SendAndSetLocalRotation(routeConnectPool[i].transform.localRotation);
                });
                //ASLObjectTrackingSystem.UpdateObjectTransform(routeConnectPool[i].GetComponent<ASLObject>(), routeConnectPool[i].transform);
            }
        }
    }

    #region STATIC_MUTATORS

    public static void AddRouteMarker(Transform _t)
    {
        current.linkedTransform.Add(_t.gameObject);
        current.nextPos = _t.position + current.heightAboveMarker * Vector3.up;
        ASLHelper.InstantiateASLObject("MinimapMarker_RouteNode", new Vector3(0, 0, 0), Quaternion.identity, "", "", MarkerInstantiation);
        ASLHelper.InstantiateASLObject("MinimapMarker_RoutePath", new Vector3(0, 0, 0), Quaternion.identity, "", "", RouteInstantiation);
    }

    public static void RemoveRouteMarker(Transform _t)
    {
        current.removedNdx = current.linkedTransform.IndexOf(_t.gameObject);
        if(current.removedNdx > -1)
        {
            GameObject nodeToRemove = current.routeMarkerPool[current.removedNdx];
            GameObject pathToRemove = current.routeConnectPool[current.removedNdx];
            current.linkedTransform.Remove(_t.gameObject);
            current.routeMarkerPool.RemoveAt(current.removedNdx);
            current.routeConnectPool.RemoveAt(current.removedNdx);
            //ASLObjectTrackingSystem.RemoveObjectToTrack(nodeToRemove.GetComponent<ASLObject>());
            //ASLObjectTrackingSystem.RemoveObjectToTrack(pathToRemove.GetComponent<ASLObject>());
            nodeToRemove.GetComponent<ASLObject>().SendAndSetClaim(() =>
            {
                nodeToRemove.GetComponent<ASLObject>().DeleteObject();
            });
            pathToRemove.GetComponent<ASLObject>().SendAndSetClaim(() =>
            {
                pathToRemove.GetComponent<ASLObject>().DeleteObject();
            });
        }
    }

    public static void ClearRoute()
    {
        foreach (GameObject g in current.routeConnectPool)
        {
            //ASLObjectTrackingSystem.RemoveObjectToTrack(g.GetComponent<ASLObject>());
            g.GetComponent<ASLObject>().SendAndSetClaim(() =>
            {
                g.GetComponent<ASLObject>().DeleteObject();
            });
        }
        foreach (GameObject g in current.routeMarkerPool)
        {
            //ASLObjectTrackingSystem.RemoveObjectToTrack(g.GetComponent<ASLObject>());
            g.GetComponent<ASLObject>().SendAndSetClaim(() =>
            {
                g.GetComponent<ASLObject>().DeleteObject();
            });
        }
        current.routeConnectPool.Clear();
        current.routeMarkerPool.Clear();
        current.linkedTransform.Clear();
    }

    #endregion

    #region CALLBACK_FUNCTIONS

    private static void MarkerInstantiation(GameObject _myGameObject)
    {
        current.nextMarker = _myGameObject;
        current.routeMarkerPool.Add(_myGameObject);
        current.nextMarker.GetComponent<ASLObject>().SendAndSetClaim(() =>
        {
            current.nextMarker.GetComponent<ASLObject>().SendAndSetLocalPosition(current.nextPos);
            current.nextMarker.GetComponent<ASLObject>().SendAndSetObjectColor(current.myColor, current.myColor);
        });
        //ASLObjectTrackingSystem.AddObjectToTrack(_myGameObject.GetComponent<ASLObject>(), _myGameObject.transform);
        Debug.Log("Added marker");
    }

    private static void RouteInstantiation(GameObject _myGameObject)
    {
        current.nextRoute = _myGameObject;
        current.routeConnectPool.Add(_myGameObject);
        current.nextRoute.GetComponent<ASLObject>().SendAndSetClaim(() =>
        {
            current.nextRoute.GetComponent<ASLObject>().SendAndSetWorldPosition(Vector3.zero);
            current.nextRoute.GetComponent<ASLObject>().SendAndSetLocalScale(0.1f * Vector3.one);
            current.nextRoute.GetComponent<ASLObject>().SendAndSetObjectColor(current.myColor, current.myColor);
        });
        //ASLObjectTrackingSystem.AddObjectToTrack(_myGameObject.GetComponent<ASLObject>(), _myGameObject.transform);
    }

    //For reference in the event we need to pass ASLObject ids (which are strings) to the other players
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
    });

    //Locally set float callback method
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
                    }
                }
                break;
            case 1:
                //Remove from list
                int removeNdx = -1;
                break;
                
            
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
    }*/

    #endregion


}
