using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

/// <summary>
/// GenerateMapFromHeightMap will read in a heightmap of size greater than 32px x 32px and power of 2 and create meshes based on the input heightmap.
/// Each mesh has a chunk resolution of 64x64 triangles.
/// </summary>
public partial class GenerateMapFromHeightMap : MonoBehaviour {
    //TerrainASLObjectsGeneration taslog;

    private float worldMaxHeight = float.MinValue;
    private float worldMinHeight = float.MaxValue;


    [Header("Heightmap Properties")]
    [Tooltip("The heightmap used to generate the terrain.")]
    public Texture2D heightmap;                             // base heightmap
    [Tooltip("The size of the map in meters.")]
    public int mapSize = 10;                                // the total size of the map

    public static readonly int chunkResolution = 16;       // the texture resolution of each chunk
    private int numberOfChunks;                             // the number of chunks (width, height) the heightmap is made of. heightmap resolution / chunkResolution
    public int NumberOfChunks { get => numberOfChunks; set => numberOfChunks = value; }
    private float chunkSize;                                // the world unit size of each chunk. mapSize / numberOf Chunks 
    public float ChunkSize { get => chunkSize; set => chunkSize = value; }
    public GameObject[,] mapChunksGameObjects;              // map chunk container


    public List<GameObject> ChunksObjects { get => chunksObjects; set => chunksObjects = value; }
    List<GameObject> chunksObjects = new List<GameObject>();

    //public GameObject[,] ASLMapChunks;
    //List<GameObject> ASLTerrainObjects;
    public MapChunk[,] mapChunks;                         // map chunk container


    [Header("Mesh Properties")]
    [Tooltip("This material will be instances on each chunk.")]
    public Material material = null;
    public bool useDefaultMeshHeight = true;
    public float meshHeight = 1f;
    public AnimationCurve meshHieghtCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
    public const int mapChunkSize = 241;
    [Range(0, 6)]
    private int editorPreviewLOD = 0;

    

    private NoiseProperties noiseProperties;
    [Header("Noise Properties")]
    [Range(0f, 1f)]
    public float noiseInfluence = 0.25f;
    [Range(0.5f, 10f)]
    public float noiseScale = 1f;
    [Range(1, 8)]
    public int octaves = 4;
    [Range(0f, 1f)]
    public float persistence = 0.5f;
    [Range(0.01f, 5f)]
    public float lacunarity = 1f;
    public int seed = 69;


    private bool DoneGeneration = false;
    public bool IsGenerated { get { return DoneGeneration; } }


    /// <summary>
    /// Assigns class variables.
    /// </summary>
    private void Awake() {
        Debug.Assert(heightmap != null && heightmap.width >= 32 && heightmap.width % 2 == 0, "Missing or invalid heightmap.");

        numberOfChunks = heightmap.width / chunkResolution;
        chunkSize = (float)mapSize / numberOfChunks;
        chunkSize = (chunkSize > 0) ? chunkSize : 1;            // the minimum chunk size is 1

        if (useDefaultMeshHeight) {
            meshHeight = mapSize / 10;
            meshHeight = (meshHeight > 0) ? meshHeight : 0.1f;
        }

        noiseProperties = new NoiseProperties(noiseInfluence, noiseScale, octaves, persistence, lacunarity, seed);

        //taslog = new TerrainASLObjectsGeneration();
    }

    /// <summary>
    /// Generate the mehses at run time.
    /// </summary>
    private void Start() {
        if (demoMode) {
            GenDemo();
        } else {
            /*
            if (GameLiftManager.GetInstance().m_PeerId == 1) { 
                StartCoroutine(GenerateMap());
            }
            */
            GenerateMap();
        }
    }

    /// <summary>
    /// The core logic to generating the terrain meshes by doing the following:
    /// 1. generate the chunk data of the terrain by dividing the heightmap into 32px X 32px textures
    /// 2. finds the neighboring chunks for fixing the seams later on
    /// 3. smooth the edges of the terrain chunks
    /// 4. generate the chunk meshes
    /// </summary>
    void GenerateMap() {
        worldMaxHeight = float.MinValue;
        worldMinHeight = float.MaxValue;

        mapChunks = new MapChunk[numberOfChunks, numberOfChunks];   // set map chunk container
        mapChunksGameObjects = new GameObject[numberOfChunks, numberOfChunks];

        int mapWidth = heightmap.width;                             // full heightmap resolution, min 32
        int mapHeight = heightmap.height;                           // full heightmap resolution, min 32

        float mapLowerLeftX = mapSize / -2f;                        // constructing the map from left -> right, bottom -> top
        float mapLowerLeftZ = mapSize / -2f;

        int row = 0;
        int col = 0;
        // 1. generate map chunks
        for (int c = 0; c < numberOfChunks * numberOfChunks; c++) {
            if (row == numberOfChunks) {
                row = 0;
                col++;
            }
            // find the center of the chunk
            float halfChunk = chunkSize / 2f;
            Vector2 _chunkCenter = new Vector2(transform.position.x + mapLowerLeftX + (row * chunkSize) + halfChunk,
                                                transform.position.z + mapLowerLeftZ + (col * chunkSize) + halfChunk);

            // generate heightmap chunk
            Texture2D _heightmap = TextureGenerator.GetPixelMap(heightmap, (mapWidth / numberOfChunks) * row,
                                                (mapHeight / numberOfChunks) * col,
                                                mapWidth / numberOfChunks);
            // generate map data
            MapData _mapData = GenerateMapData(_heightmap);

            // create chunk
            MapChunk mapChunk = new MapChunk(_heightmap, _chunkCenter, _mapData, c);
            mapChunks[row, col] = mapChunk;
            GameObject chunk = new GameObject();
           
            // set chunk parent
            chunk.transform.parent = transform;

            // set chunk scale
            chunk.transform.localScale = Vector3.one;

            // set chunk position
            chunk.transform.position = new Vector3(mapChunk.center.x, transform.position.y, mapChunk.center.y);

            // set chunk tag
            chunk.tag = "Chunk";
                
            // set chunk name
            chunk.name = "Chunk" + row + ", " + col;

            // set chunk layerMask
            chunk.layer = LayerMask.NameToLayer("Ground");

            // set mapchunk
            chunk.AddComponent<ChunkData>().MapChunk = mapChunk;

            StartCoroutine(chunk.GetComponent<ChunkData>().AskIfVisible());
            
            chunksObjects.Add(chunk);
            mapChunksGameObjects[row, col] = chunk;
            row++;
        }

        row = 0;
        col = 0;

        // 2. find neighbors
        for (int c = 0; c < numberOfChunks * numberOfChunks; c++) {
            if (row == numberOfChunks) {
                row = 0;
                col++;
            }
            //==sides==
            // Top : z + 1 <= resolution - 1
            if (col + 1 <= numberOfChunks - 1) {
                mapChunks[row, col].chunkNeighbors[(int)MapChunkNeighbor.Top] = mapChunks[row, col + 1];
                mapChunks[row, col].chunkNeighborObjects[(int)MapChunkNeighbor.Top] = mapChunksGameObjects[row, col + 1];
               

                // check corners
                // left
                bool topLeft = (row - 1 >= 0);
                if (topLeft) {
                    mapChunks[row, col].chunkNeighbors[(int)MapChunkNeighbor.TopLeft] = mapChunks[row - 1, col + 1];
                    mapChunks[row, col].chunkNeighborObjects[(int)MapChunkNeighbor.TopLeft] = mapChunksGameObjects[row - 1, col + 1];
                   
                }
                // right
                bool topRight = (row + 1 <= numberOfChunks - 1);
                if (topRight) {
                    mapChunks[row, col].chunkNeighbors[(int)MapChunkNeighbor.TopRight] = mapChunks[row + 1, col + 1];
                    mapChunks[row, col].chunkNeighborObjects[(int)MapChunkNeighbor.TopRight] = mapChunksGameObjects[row + 1, col + 1];
                    
                }
            }

            // Right : x + 1 <= resolution - 1
            if (row + 1 <= numberOfChunks - 1) {
                mapChunks[row, col].chunkNeighbors[(int)MapChunkNeighbor.Right] = mapChunks[row + 1, col];
                mapChunks[row, col].chunkNeighborObjects[(int)MapChunkNeighbor.Right] = mapChunksGameObjects[row + 1, col];
               
            }

            // Bottom : z - 1 >= 0
            if (col - 1 >= 0) {
                mapChunks[row, col].chunkNeighbors[(int)MapChunkNeighbor.Bottom] = mapChunks[row, col - 1];
                mapChunks[row, col].chunkNeighborObjects[(int)MapChunkNeighbor.Bottom] = mapChunksGameObjects[row, col - 1];
                

                // check corners
                // left
                if (row - 1 >= 0) {
                    mapChunks[row, col].chunkNeighbors[(int)MapChunkNeighbor.BottomLeft] = mapChunks[row - 1, col - 1];
                    mapChunks[row, col].chunkNeighborObjects[(int)MapChunkNeighbor.BottomLeft] = mapChunksGameObjects[row - 1, col - 1];
                    
                }
                // right
                if (row + 1 <= numberOfChunks - 1) {
                    mapChunks[row, col].chunkNeighbors[(int)MapChunkNeighbor.BottomRight] = mapChunks[row + 1, col - 1];
                    mapChunks[row, col].chunkNeighborObjects[(int)MapChunkNeighbor.BottomRight] = mapChunksGameObjects[row + 1, col - 1];
                   
                }
            }

            // Left : x - 1 >= 0
            if (row - 1 >= 0) {
                mapChunks[row, col].chunkNeighbors[(int)MapChunkNeighbor.Left] = mapChunks[row - 1, col];
                mapChunks[row, col].chunkNeighborObjects[(int)MapChunkNeighbor.Left] = mapChunksGameObjects[row - 1, col];

            }

            // generate mesh data
            MeshData _meshData = MeshGenerator.GenerateTerrainMesh(mapChunks[row, col], meshHeight, meshHieghtCurve,
                                                                   chunkSize, editorPreviewLOD, ref worldMaxHeight, ref worldMinHeight);
            mapChunks[row, col].meshData = _meshData;
            mapChunks[row, col].meshData.CreateMesh();
            mapChunks[row, col].meshData.RecalulateNormals();

            row++;
        }

        // 3. smooth the edges and merge normals
        SmoothChunkEdges();

        // 4. set material min max values
        material.SetFloat("_WorldMax", worldMaxHeight);
        material.SetFloat("_WorldMin", worldMinHeight);
        material.SetFloat("_NumChunks", numberOfChunks);

        row = 0;
        col = 0;
        // 5. generate mesh
        for (int c = 0; c < numberOfChunks * numberOfChunks; c++) {
            if (row == numberOfChunks) {
                row = 0;
                col++;
            }

            // create chunk mesh
            Mesh mesh = mapChunks[row, col].meshData.CreateMesh();

            // create game object of chunk
            mapChunksGameObjects[row, col].AddComponent<MeshFilter>().sharedMesh = mesh;
            mapChunksGameObjects[row, col].AddComponent<MeshCollider>().sharedMesh = mesh;
            mapChunksGameObjects[row, col].AddComponent<MeshRenderer>().sharedMaterial = material;
            mapChunksGameObjects[row, col].GetComponent<MeshRenderer>().sharedMaterial.mainTexture = mapChunks[row, col].heightmap;

            row++;
        }

        DoneGeneration = true;
    }

    public GameObject GetChunkGameObject(int _index) {
        return chunksObjects[_index];
    }

    public void GetMapChunks(ref List<GameObject> mapChunks) {
        mapChunks = chunksObjects;
    }

    /// <summary>
    /// SmoothChunkEdges: Smooths the edges of the chunks so that there are no seams between meshes.
    /// This is done by sampling the edge vertices and normalizing them with the adjacent neighboring 
    /// verticies.
    /// Reference for normals: https://answers.unity.com/questions/1293825/how-to-calculate-normal-direction-for-shared-verte.html
    /// </summary>
    private void SmoothChunkEdges() {
        int row = 0;
        int col = 0;
        int chunkLength = chunkResolution;

        for (int chunk = 0; chunk < numberOfChunks * numberOfChunks; chunk++) {
            // check and reset row
            if (row == numberOfChunks) {
                row = 0;
                col++;
            }

            MapChunk currChunk = mapChunks[row, col];
            MapChunk topNeighbor = currChunk.chunkNeighbors[(int)MapChunkNeighbor.Top];
            MapChunk rightNeighbor = currChunk.chunkNeighbors[(int)MapChunkNeighbor.Right];
            MapChunk bottomNeighbor = currChunk.chunkNeighbors[(int)MapChunkNeighbor.Bottom];
            MapChunk leftNeighbor = currChunk.chunkNeighbors[(int)MapChunkNeighbor.Left];


            // top
            for (int i = 0; i < chunkLength; i++) {
                // top
                if (topNeighbor != null) {
                    // smooth vertex heights
                    float yValue = currChunk.meshData.vertices[currChunk.topVerts[i]].y;
                    yValue += topNeighbor.meshData.vertices[topNeighbor.bottomVerts[i]].y;

                    topNeighbor.meshData.vertices[topNeighbor.bottomVerts[i]].y = yValue / 2f;
                    currChunk.meshData.vertices[currChunk.topVerts[i]].y = yValue / 2f;

                    // average normals
                    Vector3 currNormal = currChunk.meshData.normals[currChunk.topVerts[i]];
                    Vector3 neighborNormal = topNeighbor.meshData.normals[topNeighbor.bottomVerts[i]];
                    Vector3 vertexNormal = (currNormal + neighborNormal).normalized;

                    currChunk.meshData.normals[currChunk.topVerts[i]] = vertexNormal;
                    topNeighbor.meshData.normals[topNeighbor.bottomVerts[i]] = vertexNormal;
                }

                // right
                if (rightNeighbor != null) {
                    // smooth vertex heights
                    float yValue = currChunk.meshData.vertices[currChunk.rightVerts[i]].y;
                    yValue += rightNeighbor.meshData.vertices[rightNeighbor.leftVerts[i]].y;

                    rightNeighbor.meshData.vertices[rightNeighbor.leftVerts[i]].y = yValue / 2f;
                    currChunk.meshData.vertices[currChunk.rightVerts[i]].y = yValue / 2f;

                    // average normals
                    Vector3 currNormal = currChunk.meshData.normals[currChunk.rightVerts[i]];
                    Vector3 neighborNormal = rightNeighbor.meshData.normals[rightNeighbor.leftVerts[i]];
                    Vector3 vertexNormal = (currNormal + neighborNormal).normalized;

                    currChunk.meshData.normals[currChunk.rightVerts[i]] = vertexNormal;
                    rightNeighbor.meshData.normals[rightNeighbor.leftVerts[i]] = vertexNormal;
                }

                // bottom
                if (bottomNeighbor != null) {
                    // smooth vertex heights
                    float yValue = currChunk.meshData.vertices[currChunk.bottomVerts[i]].y;
                    yValue += bottomNeighbor.meshData.vertices[bottomNeighbor.topVerts[i]].y;

                    bottomNeighbor.meshData.vertices[bottomNeighbor.topVerts[i]].y = yValue / 2f;
                    currChunk.meshData.vertices[currChunk.bottomVerts[i]].y = yValue / 2f;

                    // average normals
                    Vector3 currNormal = currChunk.meshData.normals[currChunk.bottomVerts[i]];
                    Vector3 neighborNormal = bottomNeighbor.meshData.normals[bottomNeighbor.topVerts[i]];
                    Vector3 vertexNormal = (currNormal + neighborNormal).normalized;

                    currChunk.meshData.normals[currChunk.bottomVerts[i]] = vertexNormal;
                    bottomNeighbor.meshData.normals[bottomNeighbor.topVerts[i]] = vertexNormal;
                }

                // left
                if (leftNeighbor != null) {
                    // smooth vertex heights
                    float yValue = currChunk.meshData.vertices[currChunk.leftVerts[i]].y;
                    yValue += leftNeighbor.meshData.vertices[leftNeighbor.rightVerts[i]].y;

                    leftNeighbor.meshData.vertices[leftNeighbor.rightVerts[i]].y = yValue / 2f;
                    currChunk.meshData.vertices[currChunk.leftVerts[i]].y = yValue / 2f;

                    // average normals
                    Vector3 currNormal = currChunk.meshData.normals[currChunk.leftVerts[i]];
                    Vector3 neighborNormal = leftNeighbor.meshData.normals[leftNeighbor.rightVerts[i]];
                    Vector3 vertexNormal = (currNormal + neighborNormal).normalized;

                    currChunk.meshData.normals[currChunk.leftVerts[i]] = vertexNormal;
                    leftNeighbor.meshData.normals[leftNeighbor.rightVerts[i]] = vertexNormal;
                }
            }

            row++;
        }

    }

    /// <summary>
    /// GenerateMapData: Creates a MapData object that contains the relavent chunk information.
    /// </summary>
    /// <param name="center">The center of the chunk in world space units.</param>
    /// <param name="heightmap">The heightmap of the chunk.</param>
    /// <returns></returns>
    public MapData GenerateMapData(Texture2D heightmap) {
        // use hegihtmap
        float[,] noiseMap = Noise.GenerateNoiseMapFromHeightmap(heightmap, noiseProperties);
        return new MapData(noiseMap, noiseProperties);
    }

    

}


/// <summary>
/// MapChunk: The object that contains all data for a map chunk.
/// </summary>
[System.Serializable]
public class MapChunk {
    public Texture2D heightmap;
    public Vector2 center;
    public int chunkID;
    public MapData mapData;
    public MeshData meshData;

    public float[,] noiseValues;

    public MapChunk[] chunkNeighbors = new MapChunk[] { null, null, null, null, null, null, null, null };
    public GameObject[] chunkNeighborObjects = new GameObject[] { null, null, null, null, null, null, null, null };

    public List<int> leftVerts = new List<int>();
    public List<int> rightVerts = new List<int>();
    public List<int> topVerts = new List<int>();
    public List<int> bottomVerts = new List<int>();
    public List<int> cornerVerts = new List<int>();

    // corner order
    // bottom left, bottom right, top left, top right
    public MapChunk(Texture2D _heightmap, Vector2 _center, MapData _mapData, int _mapID) {
        this.heightmap = _heightmap;
        this.center = _center;
        this.mapData = _mapData;
        this.chunkID = _mapID;
        noiseValues = Noise.GenerateNoiseMap(_heightmap.width, _heightmap.height, _mapData.noiseProperties.seed,
                                             _mapData.noiseProperties.noiseScale, _mapData.noiseProperties.octaves,
                                             _mapData.noiseProperties.persistence, _mapData.noiseProperties.lacunarity, _center);
    }
}


[System.Serializable]
public struct MapData {
    public readonly float[,] heightValues;
    public readonly NoiseProperties noiseProperties;
    public MapData(float[,] _heightValues, NoiseProperties _noiseProperties) {
        this.heightValues = _heightValues;
        this.noiseProperties = _noiseProperties;
    }
}

[System.Serializable]
public struct NoiseMap {
    public float[,] heightValues;
}

[System.Serializable]
public struct NoiseProperties {
    public readonly float noiseInfluence;
    public readonly float noiseScale;
    public readonly int octaves;
    public readonly float persistence;
    public readonly float lacunarity;
    public readonly int seed;

    public NoiseProperties(float _noiseInfluence, float _noiseScale, int _octaves, float _persistence, float _lacunarity, int _seed) {
        noiseInfluence = _noiseInfluence;
        noiseScale = _noiseScale;
        octaves = _octaves;
        persistence = _persistence;
        lacunarity = _lacunarity;
        seed = _seed;
    }
}

/// <summary>
/// Public enum of neighbor directions for clarity of use.
/// </summary>
public enum MapChunkNeighbor {
    TopLeft,
    Top,
    TopRight,
    Right,
    BottomRight,
    Bottom,
    BottomLeft,
    Left
}




