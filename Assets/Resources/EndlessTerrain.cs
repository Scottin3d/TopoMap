using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EndlessTerrain : MonoBehaviour {
    public static EndlessTerrain current;
    const float scale = 1f;

    const float viewerMoveThresholdForChunkUpdate = 25f;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

    public LODInfo[] detailLevels;
    public static float maxViewDistance;

    public Transform viewer;
    public Material mapMaterial;
    public static Vector2 viewerPosition;
    public static Vector2 viewerPositionPrevious;

    public static MapGenerator mapGenerator;
    int chunkSize;
    int chunksVisibleInViewDistance;

    Dictionary<Vector2, TerrainChunk> terrainChunkDict = new Dictionary<Vector2, TerrainChunk>();
    static List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    #region Mono Functions
    private void Awake() {
        current = this;
    }
    private void Start() {
        
        mapGenerator = FindObjectOfType<MapGenerator>();

        maxViewDistance = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;
        chunkSize = MapGenerator.mapChunkSize - 1;
        chunksVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / chunkSize);
        //UpdateVisibleChunks();
    }

    private void FixedUpdate() {
        if (viewer) {

            viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / scale;

            if ((viewerPositionPrevious - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate) {
                viewerPositionPrevious = viewerPosition;
                UpdateVisibleChunks();
            }
        }
    }

    public void StartTerrain(Transform _viewer) {
        viewer = _viewer;
        UpdateVisibleChunks();
    }

    #endregion
    void UpdateVisibleChunks() {
        // set all active in list to false
        for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++) {
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();

        // get viewer coordinate
        int currentChunkCoordX = Mathf.RoundToInt(viewer.position.x / chunkSize);
        int currentChunkCoordZ = Mathf.RoundToInt(viewer.position.z / chunkSize);

        for (int zOffset = -chunksVisibleInViewDistance; zOffset <= chunksVisibleInViewDistance; zOffset++) {
            for (int xOffset = -chunksVisibleInViewDistance; xOffset <= chunksVisibleInViewDistance; xOffset++) {

                Vector2 viewedChunkCord = new Vector2(currentChunkCoordX + xOffset,
                                                      currentChunkCoordZ + zOffset);

                // check dictionary for chunk at coord
                if (terrainChunkDict.ContainsKey(viewedChunkCord)) {
                    terrainChunkDict[viewedChunkCord].UpdateTerrainChunk();
                    
                } else {
                    terrainChunkDict.Add(viewedChunkCord, new TerrainChunk(viewedChunkCord, chunkSize, detailLevels, transform, mapMaterial));
                }
            }
        }
    }

    class TerrainChunk {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        MeshFilter meshFilter;
        MeshRenderer meshRenderer;
        LODInfo[] detailLevels;
        LODMesh[] lodMeshes;
        int previousLODIndex = -1;

        MapData mapData;
        bool mapDataRecieved;

        // constructor
        public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material) {
            this.detailLevels = detailLevels;
            position = coord * size;
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);
            bounds = new Bounds(position, Vector2.one * size);

            meshObject = new GameObject("TerrainChunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshRenderer.material = material;
            meshFilter = meshObject.AddComponent<MeshFilter>();

            meshObject.transform.parent = parent;
            meshObject.transform.position = positionV3 * scale;
            meshObject.transform.localScale = Vector3.one * scale;

            SetVisible(false);

            lodMeshes = new LODMesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++) {
                lodMeshes[i] = new LODMesh(detailLevels[i].LOD, UpdateTerrainChunk);
            }

            mapGenerator.RequestMapData(position, OnMapDataRecieved);
        }

        void OnMapDataRecieved(MapData mapData) {
            this.mapData = mapData;
            mapDataRecieved = true;

            Texture2D texture = TextureGenerator.TextureFromColorMap(mapData.colorMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
            meshRenderer.material.mainTexture = texture;

            UpdateTerrainChunk();
        }

        public void UpdateTerrainChunk() {
            if (mapDataRecieved) {
                float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(EndlessTerrain.viewerPosition));
                bool visible = viewerDistanceFromNearestEdge <= maxViewDistance;

                if (visible) {
                    int lodIndex = 0;
                    // do not need to check the last because it is checked above
                    for (int i = 0; i < detailLevels.Length - 1; i++) {
                        if (viewerDistanceFromNearestEdge > detailLevels[i].visibleDistanceThreshold) {
                            lodIndex = i + 1;
                        } else {
                            break;
                        }
                    }

                    if (lodIndex != previousLODIndex) {
                        LODMesh lodMesh = lodMeshes[lodIndex];
                        if (lodMesh.mesh) {
                            previousLODIndex = lodIndex;
                            meshFilter.mesh = lodMesh.mesh;
                        } else if (!lodMesh.hasRequestedMesh) {
                            lodMesh.RequestMesh(mapData);
                        }
                    }

                    terrainChunksVisibleLastUpdate.Add(this);
                }
                SetVisible(visible);
            }
        }

        public void SetVisible(bool visible) {
            meshObject.SetActive(visible);
        }

        public bool IsVisible() {
            return meshObject.activeSelf;
        }
    }

    class LODMesh {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        int lod;

        Action updateCallback;
        public LODMesh(int lod, Action updateCallback) {
            this.lod = lod;
            this.updateCallback = updateCallback;
        }

        void OnMeshDataRecieved(MeshData meshData) {
            mesh = meshData.CreateMesh();
            hasMesh = true;
            updateCallback();
        }
        public void RequestMesh(MapData mapData) {
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(mapData, lod, OnMeshDataRecieved);
        }
    }

    [System.Serializable]
    public struct LODInfo {
        public int LOD;
        public float visibleDistanceThreshold;

    }
}


