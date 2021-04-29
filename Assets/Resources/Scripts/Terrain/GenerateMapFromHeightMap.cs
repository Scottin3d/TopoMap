using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GenerateMapFromHeightMap will read in a heightmap of size greater than 32px x 32px and power of 2 and create meshes based on the input heightmap.
/// Each mesh has a chunk resolution of 64x64 triangles.
/// </summary>
public class GenerateMapFromHeightMap : MonoBehaviour {
    [Header("Heightmap Properties")]
    [Tooltip("The heightmap used to generate the terrain.")]
    public Texture2D heightmap;                             // base heightmap
    [Tooltip("The size of the map in meters.")]
    public int mapSize = 10;                                // the total size of the map
    private const int chunkResolution = 32;                 // the texture resolution of each chunk
    private int numberOfChunks;                             // the number of chunks (width, height) the heightmap is made of. heightmap resolution / chunkResolution
    private float chunkSize;                                // the world unit size of each chunk. mapSize / numberOf Chunks 
    private  MapChunk[,] mapChunks;                         // map chunk container

    [Header("Mesh Properties")]
    [Tooltip("This material will be instances on each chunk.")]
    public Material material = null;
    public bool useDefaultMeshHeight = true;
    [Range(0f, 100f)]
    public float meshHeight = 1f;
    public AnimationCurve meshHieghtCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
    public const int mapChunkSize = 241;
    //[Range(0, 6)]
    private int editorPreviewLOD = 0;
    //[Tooltip("The algorithm used to blend the seam of the chunks.")]
    //public NormalizeMode normalizeMode;

    /*
    [Header("Noise Properties - Inactive")]
    [Range(0.5f, 100f)]
    public float noiseScale = 0.3f;
    [Range(1, 8)]
    public int octaves = 4;
    [Range(0f, 1f)]
    public float persistence = 0.5f;
    [Range(0.01f, 5f)]
    public float lacunarity = 1f;
    public int seed = 69;
    public Vector2 offset = new Vector2(0, 0);
    */
    

    //private DrawMode drawMode = DrawMode.Mesh;
    //[Header("Color Properties")]
    //public TerrainType[] regions;

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
    }

    /// <summary>
    /// Generate the mehses at run time.
    /// </summary>
    private void Start() {
        GenerateMap();
    }

    /// <summary>
    /// The core logic to generating the terrain meshes by doing the following:
    /// 1. generate the chunk data of the terrain by dividing the heightmap into 32px X 32px textures
    /// 2. finds the neighboring chunks for fixing the seams later on
    /// 3. smooth the edges of the terrain chunks
    /// 4. generate the chunk meshes
    /// </summary>
    public void GenerateMap() {
        mapChunks = new MapChunk[numberOfChunks, numberOfChunks];   // set map chunk container

        int mapWidth = heightmap.width;                             // full heightmap resolution, min 32
        int mapHeight = heightmap.height;                           // full heightmap resolution, min 32

        float mapLowerLeftX = mapSize / -2f;                        // constructing the map from left -> right, bottom -> top
        float mapLowerLeftZ = mapSize / -2f;

        // 1. generate map chunks
        for (int z = 0; z < numberOfChunks; z++) {
            for (int x = 0; x < numberOfChunks; x++) {
                // find the center of the chunk
                float halfChunk = chunkSize / 2f;
                Vector2 _chunkCenter = new Vector2(transform.position.x + mapLowerLeftX + (x * chunkSize) + halfChunk,
                                                 transform.position.z + mapLowerLeftZ + (z * chunkSize) + halfChunk);

                //Vector2 chunkCenter = new Vector2(mapLowerLeftX + (x * chunkSize) + halfChunk,
                //                                  mapLowerLeftZ + (z * chunkSize) - halfChunk);

                // generate heightmap chunk
                Texture2D _heightmap = GetPixelMap((mapWidth / numberOfChunks) * x,
                                                    (mapHeight / numberOfChunks) * z,
                                                    mapWidth / numberOfChunks);
                // generate map data
                MapData _mapData = GenerateMapData(_heightmap);

                // create chunk
                mapChunks[x, z] = new MapChunk(_heightmap, _chunkCenter, _mapData);
            }
        }

        // 2. find neighbors
        for (int z = 0; z < numberOfChunks; z++) {
            for (int x = 0; x < numberOfChunks; x++) {
                //==sides==
                // Top : z + 1 <= resolution - 1
                if (z + 1 <= numberOfChunks - 1) {
                    mapChunks[x, z].chunkNeighbors[(int)MapChunkNeighbor.Top] = mapChunks[x, z + 1];
                    // check corners
                    // left
                    bool topLeft = (x - 1 >= 0);
                    if (topLeft) {
                        mapChunks[x, z].chunkNeighbors[(int)MapChunkNeighbor.TopLeft] = mapChunks[x - 1, z + 1];
                    }
                    // right
                    bool topRight = (x + 1 <= numberOfChunks - 1);
                    if (topRight) {
                        mapChunks[x, z].chunkNeighbors[(int)MapChunkNeighbor.TopRight] = mapChunks[x + 1, z + 1];
                    }
                }
                // Right : x + 1 <= resolution - 1
                if (x + 1 <= numberOfChunks - 1) {
                    mapChunks[x, z].chunkNeighbors[(int)MapChunkNeighbor.Right] = mapChunks[x + 1, z];
                }
                // Bottom : z - 1 >= 0
                if (z - 1 >= 0) {
                    mapChunks[x, z].chunkNeighbors[(int)MapChunkNeighbor.Bottom] = mapChunks[x, z - 1];
                    // check corners
                    // left
                    if (x - 1 >= 0) {
                        mapChunks[x, z].chunkNeighbors[(int)MapChunkNeighbor.TopLeft] = mapChunks[x - 1, z - 1];
                    }
                    // right
                    if (x + 1 <= numberOfChunks - 1) {
                        mapChunks[x, z].chunkNeighbors[(int)MapChunkNeighbor.TopRight] = mapChunks[x + 1, z - 1];
                    }

                }
                // Left : x - 1 >= 0
                if (x - 1 >= 0) {
                    mapChunks[x, z].chunkNeighbors[(int)MapChunkNeighbor.Left] = mapChunks[x - 1, z];
                }

                // generate mesh data
                // errors most likely steming from here
                MeshData _meshData = MeshGenerator.GenerateTerrainMesh(_mapData.heightmap, meshHeight, meshHieghtCurve, 
                                                                       chunkSize, editorPreviewLOD, chunkCenter);


                MeshData _meshData = MeshGenerator.GenerateTerrainMesh(mapChunks[x, z], meshHeight, meshHieghtCurve,
                                                                       chunkSize, editorPreviewLOD);
                mapChunks[x, z].meshData = _meshData;
            }
        }

        // 3. smooth the edges
        SmoothChunkEdges();

        // 4. generate mesh
        for (int z = 0; z < numberOfChunks; z++) {
            for (int x = 0; x < numberOfChunks; x++) {

                // create chunk mesh
                Mesh mesh = mapChunks[x, z].meshData.CreateMesh();

                // create game object of chunk
                GameObject chunk = new GameObject();
                chunk.transform.parent = transform;
                chunk.transform.localScale = Vector3.one;
                chunk.tag = "Chunk";
                chunk.name = "Chunk" + z + ", " + x;
                chunk.transform.position = new Vector3(mapChunks[x, z].center.x, transform.position.y, mapChunks[x, z].center.y);
                chunk.AddComponent<MeshFilter>().sharedMesh = mesh;
                chunk.AddComponent<MeshCollider>().sharedMesh = mesh;
                chunk.AddComponent<MeshRenderer>().sharedMaterial = Instantiate(material);
                chunk.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = mapChunks[x, z].heightmap;
            }
        }
    }

    /// <summary>
    /// SmoothChunkEdges: Smooths the edges of the chunks so that there are no seams between meshes.
    /// This is done by sampling the edge vertices and normalizing them with the adjacent neighboring 
    /// verticies.
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

            // top
            for (int i = 0; i < chunkLength; i++) {
                MapChunk currChunk = mapChunks[row, col];
                MapChunk topNeighbor = currChunk.chunkNeighbors[(int)MapChunkNeighbor.Top];
                MapChunk rightNeighbor = currChunk.chunkNeighbors[(int)MapChunkNeighbor.Right];
                MapChunk bottomNeighbor = currChunk.chunkNeighbors[(int)MapChunkNeighbor.Bottom];
                MapChunk leftNeighbor = currChunk.chunkNeighbors[(int)MapChunkNeighbor.Left];

                // top
                if (topNeighbor != null) {

                    float yValue = currChunk.meshData.vertices[currChunk.topVerts[i]].y;
                    yValue += topNeighbor.meshData.vertices[topNeighbor.bottomVerts[i]].y;

                    topNeighbor.meshData.vertices[topNeighbor.bottomVerts[i]].y = yValue / 2f;
                    currChunk.meshData.vertices[currChunk.topVerts[i]].y = yValue / 2f;
                }

                // right
                if (rightNeighbor != null) {
                    float yValue = currChunk.meshData.vertices[currChunk.rightVerts[i]].y;
                    yValue += rightNeighbor.meshData.vertices[rightNeighbor.leftVerts[i]].y;

                    rightNeighbor.meshData.vertices[rightNeighbor.leftVerts[i]].y = yValue / 2f;
                    currChunk.meshData.vertices[currChunk.rightVerts[i]].y = yValue / 2f;
                }

                // bottom
                if (bottomNeighbor != null) {
                    float yValue = currChunk.meshData.vertices[currChunk.bottomVerts[i]].y;
                    yValue += bottomNeighbor.meshData.vertices[bottomNeighbor.topVerts[i]].y;

                    bottomNeighbor.meshData.vertices[bottomNeighbor.topVerts[i]].y = yValue / 2f;
                    currChunk.meshData.vertices[currChunk.bottomVerts[i]].y = yValue / 2f;
                }

                // left
                if (leftNeighbor != null) {
                    float yValue = currChunk.meshData.vertices[currChunk.leftVerts[i]].y;
                    yValue += leftNeighbor.meshData.vertices[leftNeighbor.rightVerts[i]].y;

                    leftNeighbor.meshData.vertices[leftNeighbor.rightVerts[i]].y = yValue / 2f;
                    currChunk.meshData.vertices[currChunk.leftVerts[i]].y = yValue / 2f;
                }
            }
            row++;
        }
    }

    /// <summary>
    /// GetPixelMap: Extracts a portion of a larger texture map.
    /// </summary>
    /// <param name="width">The width of the map to extract.</param>
    /// <param name="height">The height of the map to extract.</param>
    /// <param name="size">The pixel size of the map to return.</param>
    /// <returns></returns>
    public Texture2D GetPixelMap(int width, int height, int size) {
        Color[] pixelColors = new Color[size * size];
        pixelColors = heightmap.GetPixels(width, height, size, size);
        return TextureGenerator.TextureFromColorMap(pixelColors, size, size);

    }

    /// <summary>
    /// GenerateMapData: Creates a MapData object that contains the relavent chunk information.
    /// </summary>
    /// <param name="center">The center of the chunk in world space units.</param>
    /// <param name="heightmap">The heightmap of the chunk.</param>
    /// <returns></returns>
    public MapData GenerateMapData(Texture2D heightmap) {
        // use hegihtmap
        float[,] noiseMap = Noise.GenerateNoiseMapFromHeightmap(heightmap);
        return new MapData(noiseMap);
    }
}

/// <summary>
/// MapChunk: The object that contains all data for a map chunk.
/// </summary>
[System.Serializable]
public class MapChunk {
    public Texture2D heightmap;
    public Vector2 center;
    public MapData mapData;
    public MeshData meshData;

    public MapChunk[] chunkNeighbors = new MapChunk[] { null, null, null, null, null, null, null, null };

    public List<int> leftVerts = new List<int>();
    public List<int> rightVerts = new List<int>();
    public List<int> topVerts = new List<int>();
    public List<int> bottomVerts = new List<int>();
    public List<int> cornerVerts = new List<int>();

    // corner order
    // bottom left, bottom right, top left, top right
    public MapChunk(Texture2D _heightmap, Vector2 _center, MapData _mapData) {
        this.heightmap = _heightmap;
        this.center = _center;
        this.mapData = _mapData;
    }
}

[System.Serializable]
public struct MapData {
    public readonly float[,] heightValues;

    public MapData(float[,] _heightValues) {
        this.heightValues = _heightValues;
    }
}

[System.Serializable]
public struct NoiseMap {
    public float[,] heightValues;
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


