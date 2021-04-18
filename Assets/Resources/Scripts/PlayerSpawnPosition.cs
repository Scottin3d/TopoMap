using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnPosition : MonoBehaviour
{
    public static PlayerSpawnPosition current;
    public GameObject[] playerSpawnPositions;

    private void Awake() {
        current = this;
    }

    public Transform GetSpawnPosition() {
        int arraySize = playerSpawnPositions.Length;
        int spawnIndex = Mathf.FloorToInt(Random.Range(0, arraySize - 1));
        return playerSpawnPositions[spawnIndex].transform;
    }

}
