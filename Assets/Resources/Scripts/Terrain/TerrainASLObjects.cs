using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class TerrainASLObjectsGeneration
{
    public static List<GameObject> TerrainASLObjects = new List<GameObject>();
    static int objectsToGenerate;
    static bool isComplete = false;
    public bool IsComplete { get => isComplete; set => isComplete = value; }

    public void GenerateTerrainAslObjects(int _count) {
        objectsToGenerate = _count;
        for (int i = 0; i < _count; i++) {
            ASLHelper.InstantiateASLObject("TerrainChunk", Vector3.zero, Quaternion.identity, null, null, OnObjectSpawn);
        }
    }
    public GameObject GetASLObject(int _index) {
        return TerrainASLObjects[_index];
    }

    public List<GameObject> GetASLObjects() {
        return TerrainASLObjects;
    }

    public static void OnObjectSpawn(GameObject _gameObject) {
        _gameObject.transform.name = "TerrainASLObject" + TerrainASLObjects.Count;
        TerrainASLObjects.Add(_gameObject);
        if (TerrainASLObjects.Count == objectsToGenerate) {
            isComplete = true;
        }
    }
}
