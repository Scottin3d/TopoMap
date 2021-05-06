using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class MarkerDisplay : MonoBehaviour
{
    bool deleteMode = false;

    public static MarkerDisplay current;
    public GenerateMapFromHeightMap bigMap = null;
    public GenerateMapFromHeightMap smallMap = null;
    Transform mapDisplay = null;

    private int mapScaleFactor = 1;

    public float updatesPerSecond = 2f;

    public GameObject playerMaker = null;


    private static List<GameObject> markerPool = new List<GameObject>();
    private static Dictionary<GameObject, ASLObject> markerToObjectDictionary = new Dictionary<GameObject, ASLObject>();

    private bool expanding = false;

    private void Awake() {
        current = this;
        

    }

    // Start is called before the first frame update
    void Start()
    {
        ASLObjectTrackingSystem.playerAddedEvent += current.HandleAddPlayer;
        ASLObjectTrackingSystem.objectAddedEvent += current.HandleAddObject;
        Debug.Assert(bigMap != null, "Please set " + bigMap + " in the inspector.");
        Debug.Assert(smallMap != null, "Please set " + smallMap + " in the inspector.");
        Debug.Assert(playerMaker != null, "Please set " + playerMaker + " in the inspector.");

        mapDisplay = smallMap.transform;

        mapScaleFactor = bigMap.mapSize / smallMap.mapSize;

        GeneratePlayerPool(20);
        StartCoroutine(UpdatePlayerPositions());
    }


    private void Update() {
        if (Input.GetKeyDown(KeyCode.R)) {
            deleteMode = !deleteMode;
        }

        if (Input.GetMouseButton(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100f)) {
                if (hit.collider.CompareTag("Marker")) {
                    Debug.Log("Hit marker: " + hit.transform.name + ".  This represents the ASLObject " + markerToObjectDictionary[hit.collider.gameObject] + " in the world space.");
                    hit.collider.GetComponentInChildren<Renderer>().material.color = Color.white;
                }
            }
        }
    }

    private void HandleAddPlayer(ASLObject player) {
        // get free marker from pool
        GameObject marker = GetFreeMarker();
        // add to dictionary
        if (marker == null) {
            return;
        }
        marker.SetActive(true);
        marker.GetComponentInChildren<Renderer>().material.color = Color.blue;
        markerToObjectDictionary.Add(marker, player);
    }

    private void HandleAddObject(ASLObject obj) {
        // get free marker from pool
        GameObject marker = GetFreeMarker();
        // add to dictionary
        if (marker == null) {
            return;
        }
        marker.SetActive(true);
        marker.GetComponentInChildren<Renderer>().material.color = Color.green;
        markerToObjectDictionary.Add(marker, obj);
    }

    private GameObject GetFreeMarker() {
        foreach (var m in markerPool) {
            if (!m.activeSelf) {
                // check and update pool of objects
                if (markerToObjectDictionary.Count >= (markerPool.Count * 0.8f) && !expanding) {
                    expanding = true;
                    GeneratePlayerPool(20);
                }

                return m;
            }
        }
        return null;
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
            if (markerPool.Count == 0) {
                yield return new WaitForSeconds(1f);
            }
            yield return new WaitForSeconds(1 / updatesPerSecond);
            UpdateMapMarkers();
        }
    }

    private void UpdateMapMarkers() {
        foreach (var pair in markerToObjectDictionary) {
            ASLObject mapMarker = pair.Key.GetComponent<ASLObject>();
            ASLObject worldObject = pair.Value;

            Vector3 position = mapDisplay.position + (worldObject.transform.position / mapScaleFactor);
            mapMarker.transform.position = position;

            mapMarker.SendAndSetClaim(() => {
                mapMarker.SendAndSetWorldPosition(mapMarker.transform.position);
            });
        }
    }

    public void UpdateMapMarkersOld() {
        
        List<Transform> playerTransforms = ASLObjectTrackingSystem.GetPlayers();
        List<Transform> objectTransforms = ASLObjectTrackingSystem.GetObjects();
        markerToObjectDictionary.Clear();
        int numPlayers = playerTransforms.Count;
        int numObjects = objectTransforms.Count;

        // check and update pool of objects
        if (numPlayers + numObjects >= (markerPool.Count * 0.8f) && !expanding) {
            expanding = true;
            GeneratePlayerPool(20);
        }


        for (int i = 0, o = 0; i < markerPool.Count; i++) {
            if (i < numPlayers) {
                // for each player create a blue marker
                markerPool[i].SetActive(true);
                markerPool[i].transform.GetComponentInChildren<MeshRenderer>().material.color = Color.blue;
                // translate position to small world scale
                Vector3 position = mapDisplay.position + (playerTransforms[i].position / mapScaleFactor);
                markerPool[i].transform.position = position;
                Quaternion rotation = Quaternion.identity;
                rotation.eulerAngles = new Vector3(0f, playerTransforms[i].rotation.eulerAngles.y, 0f);
                markerPool[i].transform.rotation = rotation;

                // send to ASL
                ASLObject marker = markerPool[i].GetComponent<ASLObject>();
                marker.SendAndSetClaim(() => {
                    marker.SendAndSetWorldPosition(marker.transform.position);
                    //marker.SendAndIncrementWorldRotation(rotation);
                });

                

            } else if (o < numObjects) {
                markerPool[i].SetActive(true);
                markerPool[i].transform.GetComponentInChildren<MeshRenderer>().material.color = Color.green;
                Vector3 position = mapDisplay.position + (objectTransforms[o].position / mapScaleFactor);
                //position.y = mapDisplay.position.y;
                markerPool[i].transform.position = position;

                // send to ASL
                ASLObject marker = markerPool[i].GetComponent<ASLObject>();
                marker.SendAndSetClaim(() => {
                    marker.SendAndSetWorldPosition(markerPool[i].transform.position);
                });

                // add to dictionary
                markerToObjectDictionary.Add(markerPool[i], objectTransforms[o].GetComponent<ASLObject>());

                o++;
            } else { 
               markerPool[i].SetActive(false);
            
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
        markerPool.Add(_gameObject);
        _gameObject.SetActive(false);
        _gameObject.name = "Map Marker " + (markerPool.Count - 1);
        _gameObject.transform.parent = current.transform;
    }

    public static int GetScaleFactor()
    {
        return current.mapScaleFactor;
    }
}
