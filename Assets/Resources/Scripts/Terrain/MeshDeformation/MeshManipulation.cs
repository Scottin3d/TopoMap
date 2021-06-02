using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

/// <summary>
/// MeshManipulation is the core logic for the user to interact with the mesh.  With every
/// "Update" if in <see cref="editMode"/>, the script will display which vertices will be
/// influenced by interaction.
/// </summary>
public partial class MeshManipulation : MonoBehaviour {
    public float UPDATES_PER_SECOND = 2f;

    private bool selectMode = false;
    private bool editMode = false;

    // mouse 
    private GameObject mouseObject;
    private float mouseScrollDelta;
    private float radius;
    private float radiusMin;
    private float deformationStrength;

    // display vertices
    public Gradient colorGradient;
    public AnimationCurve blendStength;
    private List<GameObject> vertexPool = new List<GameObject>();
    private int currentPoolIndex = 0;


    public GameObject currentSelection = null;
    private List<List<VertToDeform>> selectedVerts = new List<List<VertToDeform>>();
    private ASLObject terrainBrain;
    public GenerateMapFromHeightMap mapGen = null;
    static Queue<Instruction> instructions = new Queue<Instruction>();

    /// <summary>
    /// Monobehaviour Start() -- Sets variables and initalizes mesh update
    /// </summary>
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

        //terrainBrain._LocallySetFloatCallback(DefomationCallBack);

        GenerateVertexPool(20000);
        StartCoroutine(MeshManipulate());
    }

    /// <summary>
    /// Monobehavior Update() -- handles user input.
    /// </summary>
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
    }

    /// <summary>
    /// A psuedo update method used to control the number of times the deformation script tries to update the mesh. 
    /// The number of updates the script makes can be controlled with <see cref="UPDATES_PER_SECOND"/>.
    /// </summary>
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

    #region Helper Functions
    /// <summary>
    /// Clears the list of selected vertices
    /// </summary>
    private void ClearSelectedVerts() {
        foreach (var l in selectedVerts) {
            l.Clear();
        }
    }
    #endregion

    #region Vertex Display Pool
    /// <summary>
    /// Sets all of the vertex pool to off.
    /// </summary>
    private void ResetPool() {
        for (int i = 0; i < currentPoolIndex; i++) {
            //v.GetComponent<Renderer>().material.color = Color.white;
            vertexPool[i].SetActive(false);
        }
        currentPoolIndex = 0;
    }

    /// <summary>
    /// Get an unused vertex from the object pool.
    /// </summary>
    /// <returns>A refernce to an unused vertex</returns>
    private GameObject GetVertexFromPool() {
        if (currentPoolIndex >= vertexPool.Count * 0.8f) {
            GenerateVertexPool(1000);
        }
        vertexPool[currentPoolIndex].SetActive(true);
        currentPoolIndex++;
        return vertexPool[currentPoolIndex];
    }

    /// <summary>
    /// Generates an object pool of display vertices.
    /// </summary>
    /// <param name="count">How many to generate</param>
    private void GenerateVertexPool(int count) {
        for (int i = 0; i < count; i++) {
            GameObject v = CreateVertex(Vector3.zero);
            v.transform.parent = transform;
            v.transform.localScale *= 0.5f;
            vertexPool.Add(v);
            v.SetActive(false);
        }
    }

    /// <summary>
    /// Creates a vertex for the vertex display pool.
    /// </summary>
    /// <param name="position">The position where the object is created</param>
    /// <returns>The GameObject of the vertex</returns>
    private GameObject CreateVertex(Vector3 position) {
        GameObject vertex = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        vertex.transform.position = position;
        // TODO: set size based on map size
        vertex.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        return vertex;
    }

    #endregion

    /// <summary>
    /// Gets the vertices within a radial of the mouse hit position.
    /// </summary>
    /// <param name="hit">The mouse hit position</param>
    /// <param name="radius">The raidus to check</param>
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="delta"></param>
    private void ModifyMesh(float delta) {
        
        MapChunk chunk = currentSelection.GetComponent<ChunkData>().MapChunk;

        DeformInstruction i = new DeformInstruction(chunk.chunkID, selectedVerts, delta);
        ASLDeformationBrain.current.QueueInstruction(i);
    }
}


# region Struct and Objects
/// <summary>
/// A deform mesh instruction object used to pass information between ASL objects.
/// </summary>
public struct DeformInstruction {
    public int id;
    public List<List<VertToDeform>> deformVertices;
    public float deformDelta;

    public DeformObject(GameObject _currentSelection, List<List<VertToDeform>> _deformVertices, float _deformDelta) {
        currentSelection = _currentSelection;
        deformVertices = _deformVertices;
        deformDelta = _deformDelta;
    }
}

/// <summary>
/// A per-vertex structure that stores affect vertex information.
/// </summary>
public struct VertToDeform {
    public int index;
    public float distance;


    public VertToDeform(int _index, float _distance) {
        index = _index;
        distance = _distance;
    }
}

#endregion