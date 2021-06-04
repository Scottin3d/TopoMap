using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeSpawner : MonoBehaviour {

    public List<GameObject> treePrefabs;
    private List<GameObject> spawnedTrees;
    public GenerateMapFromHeightMap mapGen = null;


    // Start is called before the first frame update
    void Start() {
        spawnedTrees = new List<GameObject>();
        StartCoroutine(GenerateTrees());
    }

    IEnumerator GenerateTrees() {
        while (!mapGen.IsGenerated) {
            yield return new WaitForSeconds(0.1f);
        }

        MapChunk[,] mapChunks = mapGen.mapChunks;
        int numberOfChunks = mapGen.NumberOfChunks;


        int row = 0;
        int col = 0;
        for (int c = 0; c < numberOfChunks * numberOfChunks; c++) {
            if (row == numberOfChunks) {
                row = 0;
                col++;
            }

            Vector3 center = new Vector3(mapChunks[row, col].center.x, 0f, mapChunks[row, col].center.y);

            Vector3[] vertices = mapChunks[row, col].meshData.vertices;
            Vector3[] normals = mapChunks[row, col].meshData.normals;
            for (int i = 0; i < normals.Length; i++) {
                float angle = Mathf.Max(0, Vector3.Dot(Vector3.up.normalized, normals[i].normalized));


                if (angle < 0.95f) {
                    float chance = Random.Range(0,1f);
                    if (chance < 0.75f) {
                        GameObject tree = Instantiate(treePrefabs[(int)Random.Range(0, 2f)], (vertices[i] + center), Quaternion.identity, transform);
                        tree.transform.localScale = new Vector3(Random.Range(0.75f, 1f), Random.Range(0.75f, 1f), Random.Range(0.75f, 1f));
                        spawnedTrees.Add(tree);
                    }
                }

            }

            row++;
        }
    
    }
}
