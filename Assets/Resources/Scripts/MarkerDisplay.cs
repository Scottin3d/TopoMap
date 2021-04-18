using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class MarkerDisplay : MonoBehaviour
{
    public static MarkerDisplay current;

    public float updatesPerSecond = 10f;
    public GameObject playerMaker = null;
    public Transform mapDisplay = null;
    public int mapScaleFactor;

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

            UpdatePlayerMapMarkers(ASLObjectTrackingSystem.GetPlayers());
        }
    }

    public void UpdatePlayerMapMarkers(List<Transform> playerTransforms) {
        int numPlayers = playerTransforms.Count;
        for (int i = 0; i < playerMarkerPool.Count; i++) {
            if (i < numPlayers) {
                playerMarkerPool[i].SetActive(true);
                Vector3 position = mapDisplay.position - (playerTransforms[i].position / mapScaleFactor);
                position.y = mapDisplay.position.y;
                playerMarkerPool[i].transform.position = position;

                playerMarkerPool[i].transform.rotation = playerTransforms[i].rotation;
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
