using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

/// <summary>
/// Displays visual representations of objects between maps.  
/// In this class, world space refers to the bigMap and map space refers to the smallMap.
/// </summary>
public class MarkerDisplay : MonoBehaviour {

    public static MarkerDisplay current;                    // a static reference to self
    public GenerateMapFromHeightMap bigMap = null;          // The world space reference for objects being displayed
    public GenerateMapFromHeightMap smallMap = null;        // The map markers will be displayed on
    Transform mapDisplay = null;                            // The transform of the display map
    private int mapScaleFactor = 1;                         // The scale difference between the two maps

    public float updatesPerSecond = 2f;                     // how often the markers will be updated
    public GameObject playerMaker = null;                   // the prefab of the display marker

    private bool expanding = false;                         // used when the object pool is expanding

    private static List<GameObject> markerPool = new List<GameObject>();    // the marker object pool
    private static Dictionary<GameObject, ASLObject> markerToObjectDictionary = new Dictionary<GameObject, ASLObject>();    // dictionary of markers and corrisponding ASLObjects

    bool deleteMode = false;                                // for testing

    /// <summary>
    /// MonoBehaviour Awake
    /// </summary>
    private void Awake() {
        current = this;
    }

    /// <summary>
    /// MonoBehaviour Start
    /// </summary>
    void Start() {
        // subscribe event handlers to events actions
        ASLObjectTrackingSystem.playerAddedEvent += current.HandleAddPlayer;
        ASLObjectTrackingSystem.objectAddedEvent += current.HandleAddObject;
        ASLObjectTrackingSystem.playerRemovedEvent += HandleRemove;
        ASLObjectTrackingSystem.objectRemovedEvent += HandleRemove;


        // ensure variables are assigned
        Debug.Assert(bigMap != null, "Please set " + bigMap + " in the inspector.");
        Debug.Assert(smallMap != null, "Please set " + smallMap + " in the inspector.");
        Debug.Assert(playerMaker != null, "Please set " + playerMaker + " in the inspector.");

        // calculate variables
        mapDisplay = smallMap.transform;
        mapScaleFactor = bigMap.mapSize / smallMap.mapSize;

        // generate markers
        GeneratePlayerPool(20);
        // start marker updates
        StartCoroutine(UpdateMarkers());
    }

    /// <summary>
    /// MonoBehaviour Update
    /// </summary>
    private void Update() {
        /*
        // check delete mdoe
        if (Input.GetKeyDown(KeyCode.R)) {
            deleteMode = !deleteMode;
        }

        // check mouse click
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
        */
    }

    /// <summary>
    /// Coroutine to update map markers.
    /// </summary>
    /// <returns>Timing (1 / updatesPerSecond)</returns>
    IEnumerator UpdateMarkers() {
        while (true) {
            if (markerPool.Count == 0) {
                yield return new WaitForSeconds(1f);
            }
            yield return new WaitForSeconds(1 / updatesPerSecond);
            UpdateMapMarkers();
        }
    }

    /// <summary>
    /// Updates the marker transforms on the map display.
    /// </summary>
    private void UpdateMapMarkers() {
        // cycle through dictionary of ASLObjects
        foreach (var pair in markerToObjectDictionary) {
            ASLObject mapMarker = pair.Key.GetComponent<ASLObject>();
            ASLObject worldObject = pair.Value;

            // translate position from world space to map space.
            Vector3 position = mapDisplay.position + (worldObject.transform.position / mapScaleFactor);
            mapMarker.transform.position = position;

            // send info through
            mapMarker.SendAndSetClaim(() => {
                mapMarker.SendAndSetWorldPosition(mapMarker.transform.position);
            });
        }
    }

    /// <summary>
    /// Handlesthe AddPlayer event action from the ASLObjectTrackingSystem.
    /// </summary>
    /// <param name="player">The ASLObject passed by the event.</param>
    private void HandleAddPlayer(ASLObject player) {
        // get free marker from pool
        GameObject marker = GetFreeMarker();
        if (marker == null) {
            return;
        }
        // set active
        marker.SetActive(true);
        // add to dictionary
        markerToObjectDictionary.Add(marker, player);
        // other actions
        marker.GetComponentInChildren<Renderer>().material.color = Color.blue;
    }

    /// <summary>
    /// Handles the AddObject event action from the ASLObjectTrackingSystem.
    /// </summary>
    /// <param name="obj">The ASLObject passed by the event.</param>
    private void HandleAddObject(ASLObject obj) {
        // get free marker from pool
        GameObject marker = GetFreeMarker();
        if (marker == null) {
            return;
        }
        // add to dictionary
        markerToObjectDictionary.Add(marker, obj);
        // set active
        marker.SetActive(true);
        // other actions
        marker.GetComponentInChildren<Renderer>().material.color = Color.green;
    }

    private void HandleRemove(ASLObject obj) {
        foreach (var pair in markerToObjectDictionary) {
            if (pair.Value == obj) {
                pair.Key.gameObject.SetActive(false);
                markerToObjectDictionary.Remove(pair.Key);
            }
        }
    }

    #region Marker Pool
    /// <summary>
    /// Get the next available marker in the pool.
    /// </summary>
    /// <returns>A marker GameObject</returns>
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

    /// <summary>
    /// Add display markers to the object pool.
    /// </summary>
    /// <param name="count">The number of markers to be added to the object pool.</param>
    private void GeneratePlayerPool(int count) {
        for (int i = 0; i < count; i++) {
            ASLHelper.InstantiateASLObject(playerMaker.name, Vector3.zero, Quaternion.identity, null, null, OnMarkerCreate);
        }
        // set expanding to false
        expanding = false;
    }

    /// <summary>
    /// The callback of the ASLObject instantiation.
    /// </summary>
    /// <param name="_gameObject">The GameObject of the ASLObject instantiated.</param>
    private static void OnMarkerCreate(GameObject _gameObject) {
        // add to the pool and set inactive
        markerPool.Add(_gameObject);
        _gameObject.SetActive(false);
        _gameObject.name = "Map Marker " + (markerPool.Count - 1);
        _gameObject.transform.parent = current.transform;
    }

    #endregion

    /// <summary>
    /// Get the scale factor difference between the two maps in use.  Large Map / Small Map.
    /// </summary>
    /// <returns>The scale factor.</returns>
    public static float GetScaleFactor() {
        return current.mapScaleFactor;
    }
}
