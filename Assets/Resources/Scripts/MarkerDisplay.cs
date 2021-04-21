using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class MarkerDisplay : MonoBehaviour
{
    public static MarkerDisplay current;
    public int mapScaleFactor;

    public float updatesPerSecond = 2f;

    public GameObject playerMaker = null;
    public Transform mapDisplay = null;


    private List<GameObject> playerMarkerPool = new List<GameObject>();

    private void Awake() {
        current = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(playerMaker != null, "Please set " + playerMaker + " in the inspector.");
        GeneratePlayerPool();

        StartCoroutine(UpdatePlayerPositions());
    }

    IEnumerator UpdatePlayerPositions() {
        while (true) {
            yield return new WaitForSeconds(1 / updatesPerSecond);

            UpdateMapMarkers();
        }
    }

    public void UpdateMapMarkers() {
        List<Transform> playerTransforms = ASLObjectTrackingSystem.GetPlayers();
        List<Transform> objectTransforms = ASLObjectTrackingSystem.GetObjects();

        int numPlayers = playerTransforms.Count;
        int numObjects = objectTransforms.Count;

        for (int i = 0, o = 0; i < playerMarkerPool.Count; i++) {
            if (i < numPlayers) {
                playerMarkerPool[i].SetActive(true);
                playerMarkerPool[i].transform.GetComponentInChildren<MeshRenderer>().material.color = Color.blue;
                Vector3 position = mapDisplay.position + (playerTransforms[i].position / mapScaleFactor);
                position.y = mapDisplay.position.y;
                playerMarkerPool[i].transform.position = position;
                Quaternion rotation = Quaternion.identity;
                rotation.eulerAngles = new Vector3(0f, playerTransforms[i].rotation.eulerAngles.y, 0f);
                playerMarkerPool[i].transform.rotation = rotation;
            } else if (o < numObjects) {
                playerMarkerPool[i].SetActive(true);
                playerMarkerPool[i].transform.GetComponentInChildren<MeshRenderer>().material.color = Color.green;
                Vector3 position = mapDisplay.position + (objectTransforms[o].position / mapScaleFactor);
                position.y = mapDisplay.position.y;
                playerMarkerPool[i].transform.position = position;

                o++;
            } else { 
                        playerMarkerPool[i].SetActive(false);
            
            }
            
        }
    }

    private void GeneratePlayerPool() {
        for (int i = 0; i < 20; i++) {
            GameObject newPlayer = Instantiate(playerMaker, transform);
            newPlayer.SetActive(false);
            playerMarkerPool.Add(newPlayer);
        }
        
    }
}
