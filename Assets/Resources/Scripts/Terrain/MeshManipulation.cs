using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshManipulation : MonoBehaviour {
    public float UPDATES_PER_SECOND = 2f;
    public GenerateMapFromHeightMap mapGen = null;

    public string currentSelections = "";
    public int hitVertexIndex = 0;
    public bool editMode = false;

    public float radius = 5f;
    public Gradient colorGradient;
    private List<GameObject> vertexPool = new List<GameObject>();
    private int currentPoolIndex = 0;
    public List<int> selectedVerts = new List<int>();
    // Start is called before the first frame update

    // mouse
    GameObject mouseObject;
    void Start() {
        mouseObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        float meshRes = mapGen.mapSize / (mapGen.heightmap.width / 32f) / 32f;
        mouseObject.transform.localScale = Vector3.one * meshRes / 2f;
        mouseObject.GetComponent<Renderer>().material.color = Color.red;
        mouseObject.gameObject.SetActive(editMode);

        GenerateVertexPool(20000);
        StartCoroutine(MeshManipulate());
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            editMode = !editMode;
            mouseObject.gameObject.SetActive(editMode);
        }

        //MeshManipulate();
    }

    // Update is called once per frame
    IEnumerator MeshManipulate() {
        while (true) {
            yield return new WaitForSeconds(1 / UPDATES_PER_SECOND);

            // reset pool
            ResetPool();

            if (editMode) {

                int layerMask = LayerMask.GetMask("Ground");
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 150f, layerMask)) {
                    if (hit.collider.CompareTag("Chunk")) {
                        // get a list of radial verts

                        mouseObject.transform.position = hit.point;
                        MapChunk mapChunk = hit.collider.GetComponent<ChunkData>().MapChunk;
                        currentSelections = hit.transform.name;
                        List<Vector3> v = GetRadialVerts(hit, radius);
                        //ShowRadialSelection(v, hit.point);

                    } else {
                        currentSelections = null;
                    }
                }
            }

            if (editMode && Input.GetMouseButtonDown(0)) {
                //MoveVerts(selectedVerts);

            }
        }
    }

    /*
    private void MoveVerts(List<int> verts) {

        //Mesh mesh = currentSelections.GetComponent<MeshFilter>().sharedMesh;
        Vector3[] vertices = mesh.vertices;
        foreach (var v in verts) {
            vertices[v].y += 10f;
        }
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
    */

    private void ShowRadialSelection(List<Vector3> verts, Vector3 center) {

        if (verts.Count >= vertexPool.Count * 0.8f) {
            GenerateVertexPool(1000);
        }

        foreach (var vert in verts) { 
            GameObject v = GetVertexFromPool();
            float distance = Vector3.Distance(vert, center);
            //v.GetComponent<Renderer>().material.color = colorGradient.Evaluate(distance / radius);
            v.transform.position = vert;
            float meshRes = mapGen.mapSize / (mapGen.heightmap.width / 32f) / 32f;
            v.transform.localScale = Vector3.one * meshRes / 4f;
        }
    }

    /*
    private List<int> EditSelection(RaycastHit hit) {
        List<int> vertsIndicies = new List<int>();
        List<Vector3> verts = new List<Vector3>();

        currentSelections = hit.collider.gameObject;
        Mesh mesh = currentSelections.GetComponent<MeshFilter>().sharedMesh;
        hitVertexIndex = hit.triangleIndex;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        Transform hitTransform = hit.collider.transform;

        for (int i = 0; i < 3; i++) {
            GameObject v = GetVertexFromPool();
            int index = triangles[hit.triangleIndex * 3 + i];
            vertsIndicies.Add(index);
            Vector3 p = vertices[index];
            p = hitTransform.TransformPoint(p);
            verts.Add(p);
            v.transform.position = p;
        }


        Debug.DrawLine(verts[0], verts[1]);
        Debug.DrawLine(verts[1], verts[2]);
        Debug.DrawLine(verts[2], verts[2]);

        return vertsIndicies;
    }
    */
    

    private void ResetPool() {
        selectedVerts.Clear();
        for (int i = 0; i < currentPoolIndex; i++) {
            //v.GetComponent<Renderer>().material.color = Color.white;
            vertexPool[i].SetActive(false);
        }
        currentPoolIndex = 0;
    }
    private GameObject GetVertexFromPool() {


        vertexPool[currentPoolIndex].SetActive(true);
        currentPoolIndex++;
        /*
        foreach (var v in vertexPool) {
            if (!v.activeSelf) {
                v.SetActive(true);
                return v;
            }
        }
        */
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

    private List<Vector3> GetRadialVerts(RaycastHit hit, float radius) {
        Vector3 center = hit.point;

        // check current chunk
        ChunkData chunk = hit.collider.GetComponent<ChunkData>();
        Vector3[] vertices = chunk.MapChunk.meshData.vertices;

        List<Vector3> verts = new List<Vector3>();


        Matrix4x4 localToWorld = hit.transform.localToWorldMatrix;

        for (int i = 0; i < vertices.Length; i++) {
            Vector3 realworldV3Position = localToWorld.MultiplyPoint3x4(vertices[i]);
            float distance = Mathf.Abs(Vector3.Distance(realworldV3Position, center));
            if (distance <= radius) {

                //Vector3 vert = transform.TransformPoint(vertices[i]);
                Vector3 vert = realworldV3Position;

                verts.Add(vert);

                GameObject v = GetVertexFromPool();
                v.GetComponent<Renderer>().material.color = colorGradient.Evaluate(distance / radius);
                v.transform.position = vert;
                float meshRes = mapGen.mapSize / (mapGen.heightmap.width / 32f) / 32f;
                v.transform.localScale = Vector3.one * meshRes / 2f;
            }
        }

        
        //check neighbors
        foreach (var neighbor in chunk.MapChunk.chunkNeighborObjects) {
            if (neighbor != null) {
                Matrix4x4 neighborToWorld = neighbor.transform.localToWorldMatrix;
                // check neighbor berts
                Vector3[] neighborVerts = neighbor.GetComponent<ChunkData>().MapChunk.meshData.vertices;


                for (int i = 0; i < neighborVerts.Length; i++) {

                    //Vector3 realworldV3Position = localToWorld.MultiplyPoint3x4(neighborVerts[i]);
                    Vector3 realworldV3Position = neighbor.transform.TransformPoint(neighborVerts[i]);


                    float distance = Mathf.Abs(Vector3.Distance(realworldV3Position, center));
                    if (distance <= radius) {

                        Vector3 vert = realworldV3Position;
                        verts.Add(vert);

                        GameObject v = GetVertexFromPool();
                        v.GetComponent<Renderer>().material.color = colorGradient.Evaluate(distance / radius);
                        v.transform.position = vert;
                        float meshRes = mapGen.mapSize / (mapGen.heightmap.width / 32f) / 32f;
                        v.transform.localScale = Vector3.one * meshRes / 2f;
                    }
                }
            }
        }
        
        return verts;
    }


}
