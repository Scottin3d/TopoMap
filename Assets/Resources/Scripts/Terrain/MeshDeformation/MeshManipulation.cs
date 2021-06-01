using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;
public partial class MeshManipulation : MonoBehaviour {
    public float UPDATES_PER_SECOND = 2f;

    public GameObject currentSelection = null;
    private bool selectMode = false;
    private bool editMode = false;

    private float mouseScrollDelta;
    public float radius;
    private float radiusMin;
    private float deformationStrength;

    public Gradient colorGradient;
    public AnimationCurve blendStength;
    private List<GameObject> vertexPool = new List<GameObject>();
    private int currentPoolIndex = 0;
    private List<List<VertToDeform>> selectedVerts = new List<List<VertToDeform>>();

    ASLObject terrainBrain;
    public GenerateMapFromHeightMap mapGen = null;
    static bool isUpdating = false;
    static int chunkID;
    static float[] detlas;
    static int[] vertices;
    // mouse
    GameObject mouseObject;
    void Start() {
        terrainBrain = GetComponent<ASLObject>();
        radius = mapGen.mapSize / 20f;
        radiusMin = mapGen.ChunkSize / 10f;
        ChangeRadius?.Invoke(radius.ToString());
        deformationStrength = radius / 20f;
        ChangeStrength?.Invoke(deformationStrength.ToString());


        mouseObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        float meshRes = mapGen.mapSize / mapGen.NumberOfChunks / 32f;
        mouseObject.transform.localScale = Vector3.one * meshRes / 2f;
        mouseObject.GetComponent<Renderer>().material.color = Color.red;
        mouseObject.gameObject.SetActive(selectMode);

        selectedVerts.Add(new List<VertToDeform>());
        int enumSize = System.Enum.GetNames(typeof(MapChunkNeighbor)).Length;
        for (int i = 0; i < enumSize; i++) { 
            selectedVerts.Add(new List<VertToDeform>());
        }

        terrainBrain._LocallySetFloatCallback(DefomationCallBack);

        GenerateVertexPool(20000);
        StartCoroutine(MeshManipulate());
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            selectMode = !selectMode;
            mouseObject.gameObject.SetActive(selectMode);

            if (!selectMode) {
                ResetPool();
            }
        }

        // update radius
        if (Input.GetKey(KeyCode.LeftControl)) {
            mouseScrollDelta = Input.mouseScrollDelta.y * 0.5f;
            ChangeDeformationRadius(mouseScrollDelta);
        }

        if (selectMode && currentSelection && Input.GetMouseButtonDown(0)) {
            DeformObject sendDeformInfo = new DeformObject(currentSelection, selectedVerts, deformationStrength);
            //meshDefoController.SendAndSetClaim(() => {
                //meshDefoController.SendMessage("ASLModifyMesh", sendDeformInfo);
            //});

            ModifyMesh(deformationStrength);
        }

        if (selectMode && currentSelection && Input.GetMouseButton(0)) {
            ModifyMesh(deformationStrength);
        }

        // min strength -2.5f
        if (Input.GetKeyDown(KeyCode.LeftBracket)) {
            ChangeDeformationStrength(-0.1f);
        }

        // max stregnth 2.5f
        if (Input.GetKeyDown(KeyCode.RightBracket)) {
            ChangeDeformationStrength(0.1f);
        }

        //
        /*
        if (isUpdating) {
            GameObject chunk = mapGen.GetChunkGameObject(chunkID);
            Vector3[] chunkVertices = chunk.GetComponent<MeshFilter>().mesh.vertices;
            int i = 0;
            foreach (var v in vertices) {
                Vector3 vert = chunkVertices[v];
                vert.y = detlas[i];
                i++;
                chunkVertices[v] = vert;
            }

            chunk.GetComponent<MeshFilter>().mesh.RecalculateBounds();
            chunk.GetComponent<MeshFilter>().mesh.RecalculateNormals();
            isUpdating = !isUpdating;
        }
        */
    }

    public static void DefomationCallBack(string _id, float[] _myFloats) {
        if (ASL.ASLHelper.m_ASLObjects.TryGetValue(_id, out ASL.ASLObject myObject)) {
            Debug.Log("The name of the object that sent these floats is: " + myObject.name);
        }
        // [0] id
        int chunkId = Convert.ToInt32(_myFloats[0]);
        // [1] number of affected vertices
        int numberOfAffectedVertices = Convert.ToInt32(_myFloats[1]);
        int[] affectedVertices = new int[numberOfAffectedVertices];

        // [2] affected vertex indices
        int v = 2;
        for (int i = 0; i < numberOfAffectedVertices; i++, v++) {
            affectedVertices[i] = Convert.ToInt32(_myFloats[v]);
        }

        // [3] affected vertex deltas
        float[] affectedVertexDelta = new float[numberOfAffectedVertices];
        for (int i = 0; i < numberOfAffectedVertices; i++, v++) {
            affectedVertexDelta[i] = _myFloats[v];
        }

        // need a static copy of the meshes?
        chunkID = chunkId;
        detlas = affectedVertexDelta;
        vertices = affectedVertices;
        isUpdating = true;
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
                        currentSelection = hit.transform.gameObject;

                        ClearSelectedVerts();
                        GetRadialVerts(hit, radius);

                    } else {
                        ClearSelectedVerts();
                        currentSelection = null;
                    }
                } else {
                    ClearSelectedVerts();
                    currentSelection = null;
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
        if (currentPoolIndex >= vertexPool.Count * 0.8f) {
            GenerateVertexPool(1000);
        }
        vertexPool[currentPoolIndex].SetActive(true);
        currentPoolIndex++;
        return vertexPool[currentPoolIndex];
    }

    private void GenerateVertexPool(int count) {
        for (int i = 0; i < count; i++) {
            GameObject v = CreateVertex(Vector3.zero);
            v.transform.parent = transform;
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

    public void ASLModifyMesh(DeformObject deformObject) {
        MapChunk chunk = deformObject.currentSelection.GetComponent<ChunkData>().MapChunk;

        for (int i = 0; i < deformObject.deformVertices.Count; i++) {
            if (i == 0) {
                Vector3[] vertices = chunk.meshData.vertices;

                foreach (var v in deformObject.deformVertices[i]) {
                    float strength = 1 - (v.distance / radius);

                    vertices[v.index].y += deformObject.deformDelta * strength;
                }

                currentSelection.GetComponent<MeshFilter>().mesh.vertices = vertices;
                currentSelection.GetComponent<MeshFilter>().mesh.RecalculateBounds();
                currentSelection.GetComponent<MeshFilter>().mesh.RecalculateNormals();
            } else {
                // neighbor
                if (chunk.chunkNeighbors[i - 1] != null) {
                    Vector3[] vertices = chunk.chunkNeighbors[i - 1].meshData.vertices;

                    foreach (var v in deformObject.deformVertices[i]) {
                        float strength = 1 - (v.distance / radius);
                        vertices[v.index].y += deformObject.deformDelta * strength;
                    }
                    chunk.chunkNeighborObjects[i - 1].GetComponent<MeshFilter>().mesh.vertices = vertices;
                    chunk.chunkNeighborObjects[i - 1].GetComponent<MeshFilter>().mesh.RecalculateBounds();
                    chunk.chunkNeighborObjects[i - 1].GetComponent<MeshFilter>().mesh.RecalculateNormals();
                }
            }

        }

        // recalculate normals
    }


    private float[] ConvertVertexIndices(List<VertToDeform> convertList) {
        float[] array = new float[convertList.Count];

        for (int i = 0; i < convertList.Count; i++) {
            array[i] = convertList[i].index;
        }
        return array;
    }

    private float[] ConvertVertexVector3(List<Vector3> convertList) {
        float[] array = new float[convertList.Count * 3];

        for (int i = 0, a = 0; i < convertList.Count; i++, a += 3) {
            array[a] = convertList[i].x;
            array[a + 1] = convertList[i].y;
            array[a + 2] = convertList[i].z;
        }

        return array;
    }
    */

    private void ModifyMesh(float delta) {
        
        MapChunk chunk = currentSelection.GetComponent<ChunkData>().MapChunk;

        for (int i = 0; i < selectedVerts.Count; i++) {
            
            // selection
            if (i == 0) {
                ASLObject asl = currentSelection.GetComponent<ASLObject>();
                terrainBrain.SendAndSetClaim(() => {
                    // chunk id
                    float[] id = new float[] { currentSelection.GetComponent<ChunkData>().MapChunk.chunkID };

                    // convert affected vertex list to float[]
                    float[] verticesToChange = ConvertVertexIndices(selectedVerts[0]);

                    // number of verts to change
                    float[] numberOfAffectedVertices = new float[] { verticesToChange.Length };

                    Vector3[] vertices = chunk.meshData.vertices;
                    List<Vector3> vertexV3s = new List<Vector3>();
                    float[] affectedVertexDelta = new float[verticesToChange.Length];

                    int i = 0;
                    foreach (var v in selectedVerts[i]) {
                        float strength = 1 - (v.distance / radius);
                        vertices[v.index].y += delta * strength;
                        vertexV3s.Add(vertices[v.index]);
                        affectedVertexDelta[i] = vertices[v.index].y;
                        i++;
                    }

                    // convert V3 to float[]
                    float[] verticesVector3s = ConvertVertexVector3(vertexV3s);


                    float[] payload = CombineFloatArrays(id, numberOfAffectedVertices, verticesToChange, affectedVertexDelta);
                    terrainBrain.SendFloatArray(payload);
                    //asl.SendAndDeformMesh(verticesToChange, verticesVector3s);


                    // non asl manipulation
                    //currentSelection.GetComponent<MeshFilter>().mesh.vertices = vertices;
                    //currentSelection.GetComponent<MeshFilter>().mesh.RecalculateBounds();
                    //currentSelection.GetComponent<MeshFilter>().mesh.RecalculateNormals();
                
                });

            } else {
                // neighbor
                if (chunk.chunkNeighbors[i - 1] != null) {
                    Vector3[] vertices = chunk.chunkNeighbors[i - 1].meshData.vertices;
                    foreach (var v in selectedVerts[i]) {
                        float strength = 1 - (v.distance / radius);
                        vertices[v.index].y += delta * strength;
                    }
                    chunk.chunkNeighborObjects[i - 1].GetComponent<MeshFilter>().mesh.vertices = vertices;
                    chunk.chunkNeighborObjects[i - 1].GetComponent<MeshFilter>().mesh.RecalculateBounds();
                    chunk.chunkNeighborObjects[i - 1].GetComponent<MeshFilter>().mesh.RecalculateNormals();
                }
            }

        }

        // recalculate normals

    }
    public float[] CombineFloatArrays(float[] array1, float[] array2, float[] array3, float[] array4) {
        int a1 = array1.Length;
        int a2 = array2.Length;
        int a3 = array3.Length;
        int a4 = array4.Length;

        float[] array = new float[a1 + a2 + a3 + a4];

        int a = 0;
        for (int i = 0; i < a1; i++, a++) {
            array[a] = array1[i];
        }

        for (int i = 0; i < a2; i++, a++) {
            array[a] = array2[i];
        }

        for (int i = 0; i < a3; i++, a++) {
            array[a] = array3[i];
        }

        for (int i = 0; i < a4; i++, a++) {
            array[a] = array4[i];
        }

        return array;
    }
    */
}



public struct DeformObject {
    public GameObject currentSelection;
    public List<List<VertToDeform>> deformVertices;
    public float deformDelta;

    public DeformObject(GameObject _currentSelection, List<List<VertToDeform>> _deformVertices, float _deformDelta) {
        currentSelection = _currentSelection;
        deformVertices = _deformVertices;
        deformDelta = _deformDelta;
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
