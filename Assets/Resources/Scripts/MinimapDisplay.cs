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

    private List<GameObject> playerMarkerPool = new List<GameObject>();
    private List<GameObject> routeMarkerPool = new List<GameObject>();

    private static bool PlayerInitFinished = false;

    private void Awake()
    {
        current = this;
    }


    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(playerMarker != null, "Please set " + playerMarker + " in the inspector.");
        Debug.Assert(routeNodeMarker != null, "Please set " + routeNodeMarker + " in the inspector.");

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

    public void AddRouteMarker()
    {
        GameObject newMarker = Instantiate(routeNodeMarker, transform);
        routeMarkerPool.Add(newMarker);
    }

    public void UpdatePlayerMinimapMarkers(List<Transform> playerTransforms)
    {
        int numPlayers = playerTransforms.Count;
        for(int i = 0; i < playerMarkerPool.Count; i++)
        {
            if(i < numPlayers)
            {
                playerMarkerPool[i].SetActive(true);
                Vector3 position = playerTransforms[i].position;
                position.y = 10f;
                playerMarkerPool[i].transform.position = position;
            } else
            {
                playerMarkerPool[i].SetActive(false);
            }
        }
    }
}
