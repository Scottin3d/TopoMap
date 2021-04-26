using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class MinimapDisplay : MonoBehaviour
{
    public static MinimapDisplay current;

    public float updatesPerSecond = 10f;
    public float heightAboveMarker = 5f;
    public Transform MapDisplay = null;

    private List<GameObject> routeMarkerPool = new List<GameObject>();    
    private List<GameObject> routeConnectPool = new List<GameObject>();
    private List<GameObject> smallConnectPool = new List<GameObject>();
    private List<GameObject> linkedTransform = new List<GameObject>();

    private GameObject nextMarker = null;
    private GameObject nextRoute = null;
    private Vector3 nextPos, nextRoutePos, nextScale, nextDir;
    int removedNdx = -1;

    //Would prefer to fetch this from the player once instantiated
    Color myColor;

    private void Awake()
    {
        current = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        current.gameObject.GetComponent<ASLObject>()._LocallySetFloatCallback(SyncLists);
        myColor = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f), .25f);
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
                Debug.Log("End of path");
                routeConnectPool[i].GetComponent<ASLObject>().SendAndSetClaim(() =>
                {
                    routeConnectPool[i].GetComponent<ASLObject>().SendAndSetWorldPosition(Vector3.zero);
                    routeConnectPool[i].GetComponent<ASLObject>().SendAndSetLocalScale(Vector3.zero);
                });

                smallConnectPool[i].GetComponent<ASLObject>().SendAndSetClaim(() =>
                {
                    smallConnectPool[i].GetComponent<ASLObject>().SendAndSetWorldPosition(Vector3.zero);
                    smallConnectPool[i].GetComponent<ASLObject>().SendAndSetLocalScale(Vector3.zero);
                });
            } else
            {
                curNode = routeMarkerPool[i]; nextNode = routeMarkerPool[i + 1];
                //Debug.Log("From:" + curNode.transform.position + " to " + nextNode.transform.position);
                nextDir = nextNode.transform.position - curNode.transform.position;
                length = (nextDir).magnitude / 2f;
                nextScale = new Vector3(.25f, length, .25f);
                nextRoutePos = curNode.transform.position + (length * nextDir.normalized);
                routeConnectPool[i].transform.up = nextDir;

                routeConnectPool[i].GetComponent<ASLObject>().SendAndSetClaim(() =>
                {
                    routeConnectPool[i].GetComponent<ASLObject>().SendAndSetWorldPosition(nextRoutePos);
                    routeConnectPool[i].GetComponent<ASLObject>().SendAndSetLocalScale(nextScale);
                    routeConnectPool[i].GetComponent<ASLObject>().SendAndSetLocalRotation(routeConnectPool[i].transform.localRotation);
                });

                int scaleFactor = CalcSmallScale();
                smallConnectPool[i].transform.up = nextDir;
                if (MapDisplay != null)
                {
                    if (scaleFactor > 0)
                    {
                        nextRoutePos = MapDisplay.position + ((nextRoutePos - heightAboveMarker * Vector3.up) / scaleFactor);
                        nextScale = new Vector3(.05f, length / scaleFactor, .05f);
                    }
                    else
                    {
                        nextRoutePos = Vector3.zero;
                        nextScale = Vector3.zero;
                    }
                }
                else
                {
                    if (scaleFactor > 0)
                    {
                        GameObject smMap = GameObject.FindWithTag("SmallMap");
                        nextRoutePos = smMap.transform.position + ((nextRoutePos - heightAboveMarker * 0.5f * Vector3.up) / scaleFactor);
                            //- smMap.transform.right / scaleFactor - smMap.transform.right / smMap.GetComponent<GenerateMapFromHeightMap>().mapSize;
                        nextScale = new Vector3(0.01f, length / scaleFactor, 0.01f);
                    }
                    else
                    {
                        nextRoutePos = Vector3.zero;
                        nextScale = Vector3.zero;
                    }
                }
                smallConnectPool[i].GetComponent<ASLObject>().SendAndSetClaim(() =>
                {
                    smallConnectPool[i].GetComponent<ASLObject>().SendAndSetWorldPosition(nextRoutePos);
                    smallConnectPool[i].GetComponent<ASLObject>().SendAndSetLocalScale(nextScale);
                    smallConnectPool[i].GetComponent<ASLObject>().SendAndSetLocalRotation(smallConnectPool[i].transform.localRotation);
                });
            }
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
            GameObject smallMap = GameObject.FindWithTag("SmallMap");
            GameObject largeMap = GameObject.FindWithTag("LargeMap");
            if (smallMap == null || largeMap == null) return -1;

            GenerateMapFromHeightMap _sm = smallMap.GetComponent<GenerateMapFromHeightMap>();
            GenerateMapFromHeightMap _lg = largeMap.GetComponent<GenerateMapFromHeightMap>();
            if (_sm == null || _lg == null) return -1;

            return _lg.mapSize / _sm.mapSize;
        }
    }

    //Intended to replace the SendAndSetClaim blocks used to draw the routes, but causes route segments to pile on each other sometimes
    /*private void DrawRoute(GameObject _g)
    {
        _g.GetComponent<ASLObject>().SendAndSetClaim(() =>
        {
            _g.GetComponent<ASLObject>().SendAndSetWorldPosition(nextRoutePos);
            _g.GetComponent<ASLObject>().SendAndSetLocalScale(nextScale);
            _g.GetComponent<ASLObject>().SendAndSetLocalRotation(_g.transform.localRotation);
        });
    }*/

    #region STATIC_MUTATORS

    public static void AddRouteMarker(Transform _t)
    {
        current.linkedTransform.Add(_t.gameObject);
        current.nextPos = _t.position + current.heightAboveMarker * Vector3.up;
        ASLHelper.InstantiateASLObject("MinimapMarker_RouteNode", new Vector3(0, 0, 0), Quaternion.identity, "", "", MarkerInstantiation);
        ASLHelper.InstantiateASLObject("MinimapMarker_RoutePath", new Vector3(0, 0, 0), Quaternion.identity, "", "", RouteInstantiation);
        ASLHelper.InstantiateASLObject("MinimapMarker_RoutePath", new Vector3(0, 0, 0), Quaternion.identity, "", "", SmallRouteInstantiation);
    }

    public static void RemoveRouteMarker(Transform _t, bool fromFloatCallback)
    {
        current.removedNdx = current.linkedTransform.IndexOf(_t.gameObject);
        if(current.removedNdx > -1)
        {
            GameObject nodeToRemove = current.routeMarkerPool[current.removedNdx];
            GameObject pathToRemove = current.routeConnectPool[current.removedNdx];
            GameObject smallToRemove = current.smallConnectPool[current.removedNdx];
            current.linkedTransform.Remove(_t.gameObject);
            current.routeMarkerPool.RemoveAt(current.removedNdx);
            current.routeConnectPool.RemoveAt(current.removedNdx);
            current.smallConnectPool.RemoveAt(current.removedNdx);
            nodeToRemove.GetComponent<ASLObject>().SendAndSetClaim(() =>
            {
                nodeToRemove.GetComponent<ASLObject>().DeleteObject();
            });
            pathToRemove.GetComponent<ASLObject>().SendAndSetClaim(() =>
            {
                pathToRemove.GetComponent<ASLObject>().DeleteObject();
            });
            smallToRemove.GetComponent<ASLObject>().SendAndSetClaim(() =>
            {
                smallToRemove.GetComponent<ASLObject>().DeleteObject();
            });
        } else
        {
            //Search for the transform of the small marker
            //if removedNdx > -1
            //else
            if (!fromFloatCallback)
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
                g.GetComponent<ASLObject>().DeleteObject();
            });
        }
        foreach (GameObject g in current.routeMarkerPool)
        {
            g.GetComponent<ASLObject>().SendAndSetClaim(() =>
            {
                g.GetComponent<ASLObject>().DeleteObject();
            });
        }
        foreach(GameObject g in current.smallConnectPool)
        {
            g.GetComponent<ASLObject>().SendAndSetClaim(() =>
            {
                g.GetComponent<ASLObject>().DeleteObject();
            });
        }
        current.routeConnectPool.Clear();
        current.routeMarkerPool.Clear();
        current.smallConnectPool.Clear();
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
    }

    private static void SmallRouteInstantiation(GameObject _myGameObject)
    {
        current.smallConnectPool.Add(_myGameObject);
        _myGameObject.GetComponent<ASLObject>().SendAndSetClaim(() =>
        {
            _myGameObject.GetComponent<ASLObject>().SendAndSetWorldPosition(Vector3.zero);
            _myGameObject.GetComponent<ASLObject>().SendAndSetLocalScale(0.1f * Vector3.one);
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
        foreach(Transform _t in transforms)
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
