using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public enum DrawMode {
    NoiseMap,
    ColorMap,
    Mesh
}
public class MapGenerator : MonoBehaviour {
    public bool autoGenerate = true;
    [Header("Heightmap Properties")]
    public bool useHeightmap = false;
    public Texture2D heightmap;
    public bool useRiverCurve = false;
    public AnimationCurve riverCurve;

    [Header("Noise Properties")]
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

    [Header("Color Properties")]
    public DrawMode drawMode;
    public TerrainType[] regions;

    [Header("Mesh Properties")]
    [Range(1f, 100f)]
    public float meshHeight = 1f;
    public AnimationCurve meshHieghtCurve;
    //private float[,] noiseMap;
    //private Color[] colorMap;
    public const int mapChunkSize = 241;
    [Range(0, 6)]
    public int editorPreviewLOD;
    public NormalizeMode normalizeMode;
    // thread queues
    Queue<MapThreadingInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadingInfo<MapData>>();
    Queue<MapThreadingInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadingInfo<MeshData>>();

    #region Mono Functions
    private void Update() {
        if (mapDataThreadInfoQueue.Count > 0) {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++) {
                MapThreadingInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
        if (meshDataThreadInfoQueue.Count > 0) {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++) {
                MapThreadingInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }
    #endregion

    #region MapData Callbacks
    public void RequestMapData(Vector2 center, Action<MapData> callback) {
        ThreadStart threadStart = delegate {
            MapDataThread(center, callback);
        };
        new Thread(threadStart).Start();
    }

    private void MapDataThread(Vector2 center, Action<MapData> callback) {
        MapData mapData = GenerateMapData(center);
        // locks variable until thread is finished
        lock (mapDataThreadInfoQueue) {
            mapDataThreadInfoQueue.Enqueue(new MapThreadingInfo<MapData>(callback, mapData));
        }
    }
    #endregion

    #region MeshData Callbacks
    public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback) {
        ThreadStart threadStart = delegate {
            MeshDataThread(mapData, lod, callback);
        };
        new Thread(threadStart).Start();
    }

    private void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback) {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightmap, meshHeight, meshHieghtCurve, lod);
        // locks variable until thread is finished
        lock (mapDataThreadInfoQueue) {
            meshDataThreadInfoQueue.Enqueue(new MapThreadingInfo<MeshData>(callback, meshData));
        }
    }
    #endregion



    private MapData GenerateMapData(Vector2 center) {
        // use hegihtmap
        float[,] noiseMap;
        if (useHeightmap && heightmap != null) {
            if (useRiverCurve) {
                noiseMap = Noise.GenerateNoiseMapFromHeightmapWithCurve(heightmap, riverCurve);
            } else {
                noiseMap = Noise.GenerateNoiseMapFromHeightmap(heightmap);
            }
            // use noise map
        } else {
            noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistence, lacunarity, center + offset, normalizeMode);
        }

        Color[] colorMap = new Color[mapChunkSize * mapChunkSize];
        for (int z = 0; z < mapChunkSize; z++) {
            for (int x = 0; x < mapChunkSize; x++) {
                float currHeight = noiseMap[x, z];

                for (int i = 0; i < regions.Length; i++) {
                    if (currHeight >= regions[i].height) {
                        colorMap[z * mapChunkSize + x] = regions[i].color;
                    } else {
                        break;
                    }
                }
            }
        }

        return new MapData(noiseMap, colorMap);
    }

    public void DrawMapInEditor() {
        MapData mapData = GenerateMapData(Vector2.zero);
        MapDisplay display = GetComponent<MapDisplay>();
        switch (drawMode) {
            case DrawMode.NoiseMap:
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightmap));
                return;
            case DrawMode.ColorMap:
                display.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
                return;
            case DrawMode.Mesh:
                int width = (useHeightmap && heightmap != null) ? heightmap.width : mapChunkSize;
                int height = (useHeightmap && heightmap != null) ? heightmap.height : mapChunkSize;

                display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightmap, meshHeight, meshHieghtCurve, editorPreviewLOD), TextureGenerator.TextureFromColorMap(mapData.colorMap, width, height));
                return;
        }
    }

    #region OnValidate
    private void OnValidate() {
        lacunarity = (lacunarity < 1) ? 1 : lacunarity;
        octaves = (octaves < 0) ? 0 : octaves;

    }
    #endregion

    struct MapThreadingInfo<T> {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadingInfo(Action<T> callback, T parameter) {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}

#region Structs
[System.Serializable]
public struct TerrainType {
    public string name;
    public float height;
    public Color color;
}

[System.Serializable]
public struct MapData {
    public readonly float[,] heightmap;
    public readonly Color[] colorMap;

    public MapData(float[,] heightmap, Color[] colorMap) {
        this.heightmap = heightmap;
        this.colorMap = colorMap;
    }
}
#endregion

