using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class MinimapDisplay : MonoBehaviour
{
    public static MinimapDisplay current;

    public float updatesPerSecond = 10f;
    public GameObject playerMarker = null;
    public GameObject routeNodeMarker = null;
    public GameObject routePathMarker = null;

    private List<GameObject> playerMarkerPool = new List<GameObject>();
    private List<GameObject> routeMarkerPool = new List<GameObject>();
    private List<GameObject> routeConnectPool = new List<GameObject>();

    private static bool PlayerInitFinished = false;
     bool RouteChanged = false;

    private void Awake()
    {
        current = this;
    }


    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(playerMarker != null, "Please set " + playerMarker + " in the inspector.");
        Debug.Assert(routeNodeMarker != null, "Please set " + routeNodeMarker + " in the inspector.");
        Debug.Assert(routePathMarker != null, "Please set " + routePathMarker + " in the inspector.");

        GeneratePlayerPool();
        StartCoroutine(DelayedPlayerInit());
        StartCoroutine(UpdatePlayerPositions());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }

    IEnumerator UpdatePlayerPositions()
    {
        while (PlayerInitFinished)
        {
            yield return new WaitForSeconds(1 / updatesPerSecond);

            UpdatePlayerMinimapMarkers(ASLObjectTrackingSystem.GetPlayers());
            UpdateRouteMinimapMarkers();
        }
    }

    IEnumerator DelayedPlayerInit()
    {
        Color theColor;
        for(int i = 0; i < playerMarkerPool.Count; i++)
        {
            while(playerMarkerPool[i] == null)
            {
                yield return new WaitForSeconds(0.1f);
            }
            Debug.Log("Marker " + i + " initialized");
            theColor = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f), 1f);
            playerMarkerPool[i].GetComponent<Renderer>().material.color = theColor;

            playerMarkerPool[i].GetComponent<ASLObject>().SendAndSetClaim(() =>
            {
                playerMarkerPool[i].GetComponent<ASLObject>().SendAndSetObjectColor(theColor, theColor);
            });

            playerMarkerPool[i].SetActive(false);
            ASLObjectTrackingSystem.AddObjectToTrack(playerMarkerPool[i].GetComponent<ASLObject>(), playerMarkerPool[i].transform);
        }

        Debug.Log("Init finished");
        PlayerInitFinished = true;
    }

    void GeneratePlayerPool()
    {
        for(int i = 0; i < 20; i++)
        {
            GameObject newPlayer = Instantiate(playerMarker, transform);
            newPlayer.SetActive(false);
            playerMarkerPool.Add(newPlayer);
        }
    }

    public static void AddRouteMarker(Vector3 position)
    {
        GameObject newMarker = Instantiate(current.routeNodeMarker, current.transform);
        position.y = 9.5f;
        newMarker.transform.position = position;
        current.routeMarkerPool.Add(newMarker);
        ASLObjectTrackingSystem.AddObjectToTrack(newMarker.GetComponent<ASLObject>(), newMarker.transform);
        current.RouteChanged = true;
        Debug.Log("Added marker");
    }

    public void RemoveRouteMarker(GameObject routeMarker)
    {

    }

    void UpdatePlayerMinimapMarkers(List<Transform> playerTransforms)
    {
        int numPlayers = playerTransforms.Count;
        for(int i = 0; i < playerMarkerPool.Count; i++)
        {
            if(i < numPlayers)
            {
                playerMarkerPool[i].SetActive(true);
                Vector3 position = playerTransforms[i].position;
                position.y = 55f;
                playerMarkerPool[i].transform.position = position;
            } else
            {
                playerMarkerPool[i].SetActive(false);
            }
        }
    }

    void UpdateRouteMinimapMarkers()
    {
        GameObject newPath;
        Vector3 dir;
        Vector3 scale;
        Vector3 pos;
        float length;
        int ndx = 0;
        if (RouteChanged)
        {
            Debug.Log("Route changed");
            for (ndx = 0; ndx < routeMarkerPool.Count - 1; ndx++)
            {
                dir = routeMarkerPool[ndx + 1].transform.position - routeMarkerPool[ndx].transform.position;
                length = (routeMarkerPool[ndx + 1].transform.position - routeMarkerPool[ndx].transform.position).magnitude / 2f;

                if (routeConnectPool.Count < routeMarkerPool.Count - 1)
                {
                    Debug.Log("Creating new path");
                    newPath = Instantiate(routePathMarker, transform);
                    routeConnectPool.Add(newPath);
                }

                Debug.Log(length);
                routeConnectPool[ndx].transform.up = dir;
                scale = routeConnectPool[ndx].transform.localScale;
                scale.y = length;
                pos = routeMarkerPool[ndx].transform.position + (length * routeConnectPool[ndx].transform.up);
                routeConnectPool[ndx].transform.position = pos;
                routeConnectPool[ndx].transform.localScale = scale;
            }

            RouteChanged = false;
        }
    }
}


