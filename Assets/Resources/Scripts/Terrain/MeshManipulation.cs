using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshManipulation : MonoBehaviour {
    public float UPDATES_PER_SECOND = 2f;
    public GenerateMapFromHeightMap mapGen = null;

    private GameObject currentSelections = null;
    private bool selectMode = false;
    private bool editMode = false;

    private float mouseScrollDelta;
    public float radius = 5f;
    private float radiusMin = 1f;

    public Gradient colorGradient;
    public AnimationCurve blendStength;
    private List<GameObject> vertexPool = new List<GameObject>();
    private int currentPoolIndex = 0;
    private List<List<VertToDeform>> selectedVerts = new List<List<VertToDeform>>();

    // mouse
    GameObject mouseObject;
    void Start() {
        mouseObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        float meshRes = mapGen.mapSize / (mapGen.heightmap.width / 32f) / 32f;
        mouseObject.transform.localScale = Vector3.one * meshRes / 2f;
        mouseObject.GetComponent<Renderer>().material.color = Color.red;
        mouseObject.gameObject.SetActive(selectMode);

        selectedVerts.Add(new List<VertToDeform>());
        int enumSize = System.Enum.GetNames(typeof(MapChunkNeighbor)).Length;
        for (int i = 0; i < enumSize; i++) { 
            selectedVerts.Add(new List<VertToDeform>());
        }

        GenerateVertexPool(20000);
        StartCoroutine(MeshManipulate());
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            selectMode = !selectMode;
            mouseObject.gameObject.SetActive(selectMode);
        }

        if (selectMode && Input.GetMouseButtonDown(0) || editMode && Input.GetMouseButtonDown(0)) {
            editMode = !editMode;
            selectMode = !selectMode;
        }

        // update radius
        if (Input.GetKey(KeyCode.LeftControl)) {
            mouseScrollDelta = Input.mouseScrollDelta.y * 0.5f;
            radius += mouseScrollDelta;
            radius = (radius <= radiusMin) ? radiusMin : radius;
        }

        if (editMode && Input.GetKeyDown(KeyCode.Alpha0)) {
            ModifyMesh(0.25f);
        }

        if (editMode && Input.GetKeyDown(KeyCode.Alpha9)) {
            ModifyMesh(-0.25f);
        }
    }

    // Update is called once per frame
    IEnumerator MeshManipulate() {
        while (true) {
            yield return new WaitForSeconds(1 / UPDATES_PER_SECOND);

            if (selectMode) {
                // reset pool
                ResetPool();

                int layerMask = LayerMask.GetMask("Ground");
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 150f, layerMask)) {
                    if (hit.collider.CompareTag("Chunk")) {
                        // get a list of radial verts

                        mouseObject.transform.position = hit.point;
                        MapChunk mapChunk = hit.collider.GetComponent<ChunkData>().MapChunk;
                        currentSelections = hit.transform.gameObject;

                        ClearSelectedVerts();
                        GetRadialVerts(hit, radius);

                    } else {
                        ClearSelectedVerts();
                        currentSelections = null;
                    }
                }
            }
        }
    }

    private void ClearSelectedVerts() {
        foreach (var l in selectedVerts) {
            
            l.Clear();
        }
    }


    #region Vertex Display Pool
    private void ResetPool() {
        for (int i = 0; i < currentPoolIndex; i++) {
            //v.GetComponent<Renderer>().material.color = Color.white;
            vertexPool[i].SetActive(false);
        }
        currentPoolIndex = 0;
    }

    private GameObject GetVertexFromPool() {
        vertexPool[currentPoolIndex].SetActive(true);
        currentPoolIndex++;
        return vertexPool[currentPoolIndex];
    }

    private void GenerateVertexPool(int count) {
        for (int i = 0; i < count; i++) {
            GameObject v = CreateVertex(Vector3.zero);
            v.transform.localScale *= 0.5f;
            vertexPool.Add(v);
            v.SetActive(false);
        }
    }

    private GameObject CreateVertex(Vector3 position) {
        GameObject vertex = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        vertex.transform.position = position;
        // TODO: set size based on map size
        vertex.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        return vertex;
    }

    #endregion

    /// <summary>
    /// Sets the reference vertices in the scene based on the mouse position.
    /// </summary>
    /// <param name="hit"></param>
    /// <param name="radius"></param>
    private void  GetRadialVerts(RaycastHit hit, float radius) {


        Vector3 center = hit.point;

        // check current chunk
        ChunkData chunk = hit.collider.GetComponent<ChunkData>();
        Vector3[] vertices = chunk.MapChunk.meshData.vertices;

        Matrix4x4 localToWorld = hit.transform.localToWorldMatrix;

        for (int i = 0; i < vertices.Length; i++) {
            Vector3 realworldV3Position = localToWorld.MultiplyPoint3x4(vertices[i]);
            float distance = Mathf.Abs(Vector3.Distance(realworldV3Position, center));
            if (distance <= radius) {

                // store vert for deformation
                VertToDeform vert = new VertToDeform(i, distance);
                selectedVerts[0].Add(vert);

                // place display vert
                GameObject v = GetVertexFromPool();
                float curveValue = blendStength.Evaluate(distance / radius);
                v.GetComponent<Renderer>().material.color = colorGradient.Evaluate(curveValue);
                v.transform.position = realworldV3Position;
                float meshRes = mapGen.mapSize / (mapGen.heightmap.width / 32f) / 32f;
                v.transform.localScale = Vector3.one * meshRes / 2f;
            }
        }


        //check neighbors
        int neighborIndex = 1;
        foreach (var neighbor in chunk.MapChunk.chunkNeighborObjects) {
            if (neighbor != null) {

                Matrix4x4 neighborToWorld = neighbor.transform.localToWorldMatrix;

                // check neighbor berts
                Vector3[] neighborVerts = neighbor.GetComponent<ChunkData>().MapChunk.meshData.vertices;

                for (int i = 0; i < neighborVerts.Length; i++) {
                    Vector3 realworldV3Position = neighbor.transform.TransformPoint(neighborVerts[i]);

                    float distance = Mathf.Abs(Vector3.Distance(realworldV3Position, center));
                    if (distance <= radius) {

                        VertToDeform vert = new VertToDeform(i, distance);
                        selectedVerts[neighborIndex].Add(vert);

                        GameObject v = GetVertexFromPool();
                        v.GetComponent<Renderer>().material.color = colorGradient.Evaluate(distance / radius);
                        v.transform.position = realworldV3Position;
                        float meshRes = mapGen.mapSize / (mapGen.heightmap.width / 32f) / 32f;
                        v.transform.localScale = Vector3.one * meshRes / 2f;
                    }
                }
            }
            neighborIndex++;
        }
    }

    private void ModifyMesh(float delta) {
        MapChunk chunk = currentSelections.GetComponent<ChunkData>().MapChunk;
        for (int i = 0; i < selectedVerts.Count; i++) {
            if (i == 0) {
                Vector3[] vertices = chunk.meshData.vertices;
                foreach (var v in selectedVerts[i]) {
                    vertices[v.index].y += 1 - (delta * v.distance);
                }

                currentSelections.GetComponent<MeshFilter>().mesh.vertices = vertices;
                currentSelections.GetComponent<MeshFilter>().mesh.RecalculateBounds();
                currentSelections.GetComponent<MeshFilter>().mesh.RecalculateNormals();
            } else {
                // neighbor
                if (chunk.chunkNeighbors[i - 1] != null) {
                    Vector3[] vertices = chunk.chunkNeighbors[i - 1].meshData.vertices;
                    foreach (var v in selectedVerts[i]) {
                        vertices[v.index].y += 1 - (delta * v.distance);
                    }
                    chunk.chunkNeighborObjects[i - 1].GetComponent<MeshFilter>().mesh.vertices = vertices;
                    chunk.chunkNeighborObjects[i - 1].GetComponent<MeshFilter>().mesh.RecalculateBounds();
                    chunk.chunkNeighborObjects[i - 1].GetComponent<MeshFilter>().mesh.RecalculateNormals();
                }
            }

        }

        // recalculate normals

    }
}

public struct VertToDeform {
    public int index;
    public float distance;


    public VertToDeform(int _index, float _distance) {
        index = _index;
        distance = _distance;
    }
}
