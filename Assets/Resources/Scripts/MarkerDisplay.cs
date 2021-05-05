using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class MarkerDisplay : MonoBehaviour
{
    public static MarkerDisplay current;
    public GenerateMapFromHeightMap bigMap = null;
    public GenerateMapFromHeightMap smallMap = null;

    private int mapScaleFactor = 1;

    public float updatesPerSecond = 2f;

    public GameObject playerMaker = null;
    public Transform mapDisplay = null;


    private static List<GameObject> playerMarkerPool = new List<GameObject>();
    private static Dictionary<GameObject, ASLObject> markerToObjectDictionary = new Dictionary<GameObject, ASLObject>();

    private bool expanding = false;

    private void Awake() {
        current = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(bigMap != null, "Please set " + bigMap + " in the inspector.");
        Debug.Assert(smallMap != null, "Please set " + smallMap + " in the inspector.");
        Debug.Assert(playerMaker != null, "Please set " + playerMaker + " in the inspector.");

        mapScaleFactor = bigMap.mapSize / smallMap.mapSize;

        GeneratePlayerPool(20);
        StartCoroutine(UpdatePlayerPositions());
    }

    public bool GetMarketGameObject(GameObject marker, out ASLObject obj) {
        obj = null;
        if (markerToObjectDictionary.ContainsKey(marker)) {
            obj = markerToObjectDictionary[marker];
            return true;
        }
        return false;
    }

    IEnumerator UpdatePlayerPositions() {
        while (true) {
            if (playerMarkerPool.Count == 0) {
                yield return new WaitForSeconds(1f);
            }
            yield return new WaitForSeconds(1 / updatesPerSecond);
            UpdateMapMarkers();
        }
    }

    public void UpdateMapMarkers() {
        List<Transform> playerTransforms = ASLObjectTrackingSystem.GetPlayers();
        List<Transform> objectTransforms = ASLObjectTrackingSystem.GetObjects();

        int numPlayers = playerTransforms.Count;
        int numObjects = objectTransforms.Count;

        // check and update pool of objects
        if (numPlayers + numObjects >= (playerMarkerPool.Count * 0.8f) && !expanding) {
            expanding = true;
            GeneratePlayerPool(20);
        }


        for (int i = 0, o = 0; i < playerMarkerPool.Count; i++) {
            if (i < numPlayers) {
                // for each player create a blue marker
                playerMarkerPool[i].SetActive(true);
                playerMarkerPool[i].transform.GetComponentInChildren<MeshRenderer>().material.color = Color.blue;
                // translate position to small world scale
                Vector3 position = mapDisplay.position + (playerTransforms[i].position / mapScaleFactor);
                playerMarkerPool[i].transform.position = position;
                Quaternion rotation = Quaternion.identity;
                rotation.eulerAngles = new Vector3(0f, playerTransforms[i].rotation.eulerAngles.y, 0f);
                playerMarkerPool[i].transform.rotation = rotation;

                // send to ASL
                ASLObject marker = playerMarkerPool[i].GetComponent<ASLObject>();
                marker.SendAndSetClaim(() => {
                    marker.SendAndSetWorldPosition(marker.transform.position);
                    //marker.SendAndIncrementWorldRotation(rotation);
                });

                // add to dictionary
                markerToObjectDictionary.Add(playerMarkerPool[i], playerTransforms[i].GetComponent<ASLObject>());

            } else if (o < numObjects) {
                playerMarkerPool[i].SetActive(true);
                playerMarkerPool[i].transform.GetComponentInChildren<MeshRenderer>().material.color = Color.green;
                Vector3 position = mapDisplay.position + (objectTransforms[o].position / mapScaleFactor);
                //position.y = mapDisplay.position.y;
                playerMarkerPool[i].transform.position = position;

                // send to ASL
                ASLObject marker = playerMarkerPool[i].GetComponent<ASLObject>();
                marker.SendAndSetClaim(() => {
                    marker.SendAndSetWorldPosition(playerMarkerPool[i].transform.position);
                });

                // add to dictionary
                markerToObjectDictionary.Add(playerMarkerPool[i], objectTransforms[o].GetComponent<ASLObject>());

                o++;
            } else { 
               playerMarkerPool[i].SetActive(false);
            
            }
            
        }
    }

    private void GeneratePlayerPool(int count) {
        for (int i = 0; i < count; i++) {
            ASLHelper.InstantiateASLObject(playerMaker.name, Vector3.zero, Quaternion.identity, null, null, OnMarkerCreate);
        }
        expanding = false;
    }

    private static void OnMarkerCreate(GameObject _gameObject) {
        playerMarkerPool.Add(_gameObject);
        _gameObject.SetActive(false);
        _gameObject.transform.parent = current.transform;
    }

    public static int GetScaleFactor()
    {
        return current.mapScaleFactor;
    }
}
