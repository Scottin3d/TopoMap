using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class GenerateMapFromHeightMap : MonoBehaviour {
    [Header("Heightmap Properties")]
    [Tooltip("The heightmap used to generate the terrain.")]
    public Texture2D heightmap;                 // base heightmap
    [Tooltip("The size of the map in meters.")]
    public int mapSize = 10;                    // the total size of the map
    private const int chunkResolution = 32;     // the texture resolution of each chunk
    private int numberOfChunks;                 // the number of chunks (width, height) the heightmap is made of. heightmap resolution / chunkResolution
    private int chunkSize;                      // the world unit size of each chunk. mapSize / numberOf Chunks 
    private  MapChunk[,] mapChunks;             // map chunk container

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
    [Tooltip("The algorithm used to blend the seam of the chunks.")]
    public NormalizeMode normalizeMode;

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

    

    private DrawMode drawMode = DrawMode.Mesh;
    [Header("Color Properties")]
    public TerrainType[] regions;

    /// <summary>
    /// 
    /// </summary>
    private void Awake() {
        Debug.Assert(heightmap != null && heightmap.width >= 32 && heightmap.width % 2 == 0, "Missing or invalid heightmap.");
        //heightMapChunks = (heightMapChunks % 2 == 0) ? heightMapChunks : 32;
        numberOfChunks = heightmap.width / chunkResolution;
        chunkSize = mapSize / numberOfChunks;
        chunkSize = (chunkSize > 0) ? chunkSize : 1;

        if (useDefaultMeshHeight) {
            meshHeight = mapSize / 10;
            meshHeight = (meshHeight > 0) ? meshHeight : 0.1f;
        }
        
    }

    /// <summary>
    /// 
    /// </summary>
    private void Start() {
        GenerateMap();
    }
    /// <summary>
    /// 
    /// </summary>
    public void GenerateMap() {
        mapChunks = new MapChunk[numberOfChunks, numberOfChunks];   // set map chunk container

        int mapWidth = heightmap.width;         // full heightmap resolution, min 32
        int mapHeight = heightmap.height;       // full heightmap resolution, min 32

        float mapLowerLeftX = mapSize / -2f;    // constructing the map from left -> right, bottom -> top
        float mapLowerLeftZ = mapSize / -2f;

        for (int z = 0; z < numberOfChunks; z++) {
            for (int x = 0; x < numberOfChunks; x++) {
                // find the center of the chunk
                float halfChunk = chunkSize / 2f;
                Vector2 chunkCenter = new Vector2(mapLowerLeftX + (x  * chunkSize) + halfChunk,
                                                 mapLowerLeftZ - (z * chunkSize) + halfChunk);

                //Vector2 chunkCenter = new Vector2(mapLowerLeftX + (x * chunkSize) + halfChunk,
                //                                  mapLowerLeftZ + (z * chunkSize) - halfChunk);

                // generate heightmap chunk
                Texture2D _heightmap = GetPixelTest((mapWidth / numberOfChunks) * x,
                                                    (mapHeight / numberOfChunks) * z,
                                                    mapWidth / numberOfChunks);
                // generate map data
                MapData _mapData = GenerateMapData(chunkCenter, _heightmap);

                // generate mesh data
                // errors most likely steming from here
                MeshData _meshData = MeshGenerator.GenerateTerrainMesh(_mapData.heightmap, meshHeight, meshHieghtCurve, 
                                                                       chunkSize, editorPreviewLOD, chunkCenter);

                // create chunk
                mapChunks[x, z] = new MapChunk(_heightmap, chunkCenter, _mapData, _meshData);

                // create chunk mesh
                Mesh mesh = mapChunks[x, z].meshData.CreateMesh();
                // mesh smoothing - TODO
                //mesh = mattatz.MeshSmoothingSystem.MeshSmoothing.LaplacianFilter(mesh, 2);
                //mesh = mattatz.MeshSmoothingSystem.MeshSmoothing.HCFilter(mesh, 10);

                // create game object of chunk
                GameObject chunk = new GameObject();
                chunk.transform.parent = transform;
                chunk.transform.localScale = Vector3.one;
                chunk.tag = "Chunk";
                chunk.name = "chunk" + z + x;
                chunk.transform.position = new Vector3(transform.position.x + chunkCenter.x, 0f, transform.position.y + chunkCenter.y);
                chunk.AddComponent<MeshFilter>().sharedMesh = mesh;
                chunk.AddComponent<MeshCollider>().sharedMesh = mesh;
                chunk.AddComponent<MeshRenderer>().sharedMaterial = Instantiate(material);
                chunk.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = mapChunks[x, z].heightmap;
            }
        }
    }

    public Texture2D GetPixelTest(int width, int height, int size) {
        //int mapWidth = heightmap.width;
        //int mapHeight = heightmap.height;
        Color[] pixelColors = new Color[size * size];
        pixelColors = heightmap.GetPixels(width, height, size, size);
        return TextureGenerator.TextureFromColorMap(pixelColors, size, size);

    }

    public MapData GenerateMapData(Vector2 center, Texture2D heightmap) {
        // use hegihtmap
        float[,] noiseMap = Noise.GenerateNoiseMapFromHeightmap(heightmap, meshHeight, normalizeMode);
        int mapWidth = heightmap.width;

        Color[] colorMap = new Color[mapWidth * mapWidth];
        for (int z = 0; z < mapWidth; z++) {
            for (int x = 0; x < mapWidth; x++) {
                float currHeight = noiseMap[x, z];

                for (int i = 0; i < regions.Length; i++) {
                    if (currHeight >= regions[i].height) {
                        colorMap[z * mapWidth + x] = regions[i].color;
                    } else {
                        break;
                    }
                }
            }
        }
        return new MapData(noiseMap, colorMap);
    }

    [System.Serializable]
    public struct TerrainType {
        public string name;
        public float height;
        public Color color;
    }

    [System.Serializable]
    public struct MapChunk {
        public Texture2D heightmap;
        public Vector2 center;
        public MapData mapData;
        public MeshData meshData;

        public MapChunk(Texture2D _heightmap, Vector2 _center, MapData _mapData, MeshData _meshData) {
            this.heightmap = _heightmap;
            this.center = _center;
            this.mapData = _mapData;
            this.meshData = _meshData;
        }
    }
}
