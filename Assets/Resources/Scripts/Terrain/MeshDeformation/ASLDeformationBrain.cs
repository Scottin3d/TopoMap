using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class ASLDeformationBrain : MonoBehaviour
{
    public static ASLDeformationBrain current;
    public GenerateMapFromHeightMap mapGen = null;
    private static ASLObject brain;

    public List<GameObject> localMapChunks = new List<GameObject>();
    public static Queue<Instruction> instructions = new Queue<Instruction>();


    bool IsInit = false;
    bool IsChange = false;


    private void Awake() {
        current = this;
    }

    void Start()
    {
        brain = GetComponent<ASLObject>();
        StartCoroutine(Initialize());
    }
    void Update()
    {
        /*
        int layerMask = LayerMask.GetMask("Ground");
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (IsInit && Input.GetMouseButtonDown(0)) {
            
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, layerMask)) { 
                DeformMesh(hit, 1f);
            }
        }
        if (IsInit && Input.GetMouseButtonDown(1)) {
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, layerMask)) {
                DeformMesh(hit, -1f);
            }
        }
        */

        while (instructions.Count > 0) {
            ExecuteInstruction(instructions.Dequeue());
        }

    }

    /// <summary>
    /// Converts a deform instruction into a float array and send it over ASL.
    /// **CURRENTLY ONLY AFFECTS SELECTED CHUNK**
    /// </summary>
    /// <param name="i">The deform instruction object</param>
    public void QueueInstruction(DeformInstruction i) {
        GameObject chunk = localMapChunks[i.id];
        chunk.TryGetComponent<ChunkData>(out ChunkData chunkData);
        if (chunkData != null) {
            int chunkID = chunkData.MapChunk.chunkID;
            float[] id = new float[1] { Convert.ToSingle(chunkID) };

            int count = i.deformVertices[0].Count;
            float[] vertexCount = new float[1] { Convert.ToSingle(count) };

            float[] affectedVertexIndices = new float[i.deformVertices[0].Count];
            int c = 0;
            foreach (var v in i.deformVertices[0]) {
                affectedVertexIndices[c] = i.deformVertices[0][c].index;
                c++;
            }

            float[] affectedVertexStrength = new float[i.deformVertices[0].Count];
            c = 0;
            foreach (var v in i.deformVertices[0]) {
                affectedVertexStrength[c] = i.deformVertices[0][c].defromStrength;
                c++;
            }

            float[] payload = CombineFloatArrays(id, vertexCount, affectedVertexIndices, affectedVertexStrength);

            brain.SendAndSetClaim(() => {
                brain.SendFloatArray(payload);
            });
        }
    }

    public static List<float[]> SplitPayload(float[] _payload) {
        List<float[]> splitPayload = new List<float[]>();
        splitPayload.Add(new float[1] { _payload[0] });
        splitPayload.Add(new float[1] { _payload[1] });
        int count = Convert.ToInt32(_payload[1]);

        int p = 2;
        float[] v1 = new float[count];
        for (int i = 0; i < count; i++, p++) {
            v1[i] = _payload[p];
        }
        splitPayload.Add(v1);

        float[] v2 = new float[count];
        for (int i = 0; i < count; i++, p++) {
            v2[i] = _payload[p];
        }
        splitPayload.Add(v2);


        return splitPayload;
    }

    public float[] CombineFloatArrays(float[] array1, float[] array2, float[] array3, float[] array4) {
        // lengths
        int a1 = array1.Length;
        int a2 = array2.Length;
        int a3 = array3.Length;
        int a4 = array4.Length;

        // new array
        float[] array = new float[a1 + a2 + a3 + a4];

        // add array 1 elements -- id
        int a = 0;
        for (int i = 0; i < a1; i++, a++) {
            array[a] = array1[i];
        }
        // add array 2 elements -- vertex count
        for (int i = 0; i < a2; i++, a++) {
            array[a] = array2[i];
        }
        // add array 3 elements -- vertex indices
        for (int i = 0; i < a3; i++, a++) {
            array[a] = array3[i];
        }
        // add array 4 elements -- vertex deformation (Y)
        for (int i = 0; i < a4; i++, a++) {
            array[a] = array4[i];
        }
        return array;
    }

    private void ExecuteInstruction(Instruction i) {
        GameObject chunk = localMapChunks[i.id];
        Mesh mesh = chunk.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;

        for (int v = 0; v < i.vertexIndices.Length; v++) {
            Vector3 pos = vertices[i.vertexIndices[v]];
            pos.y += i.vertexDeformation[v];
            vertices[i.vertexIndices[v]] = pos;
        }

        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

    }

    IEnumerator Initialize() {
        brain.GetComponent<ASLObject>()._LocallySetFloatCallback(MyFloatFunction);

        while (!mapGen.IsGenerated) {
            yield return new WaitForSeconds(0.1f);
        }

        localMapChunks = mapGen.ChunksObjects;
        IsInit = true;
    }

    // eventually take in
    private void DeformMesh(RaycastHit hit, float f) {
        hit.collider.TryGetComponent<ChunkData>(out ChunkData chunk);
        if (chunk != null) {
            int chunkID = chunk.MapChunk.chunkID;

            float[] payload = new float[] { Convert.ToSingle(chunkID), f };

            brain.SendAndSetClaim(() => {
                brain.SendFloatArray(payload);
            });
        }
    }

    public static void MyFloatFunction(string _id, float[] _myFloats) {
        if (ASL.ASLHelper.m_ASLObjects.TryGetValue(_id, out ASL.ASLObject myObject)) {
            Debug.Log("The name of the object that sent these floats is: " + myObject.name);
        }

        List<float[]> splitPayload = SplitPayload(_myFloats);
        int id = Convert.ToInt32(splitPayload[0][0]);
        int count = Convert.ToInt32(splitPayload[1][0]);

        int[] vertexIndices = new int[count];
        for (int v = 0; v < count; v++) {
            vertexIndices[v] = Convert.ToInt32(splitPayload[2][v]);
        }


        Instruction i = new Instruction(id, vertexIndices, splitPayload[3]);
        instructions.Enqueue(i);
    }
}

public struct Instruction {
    public int id;
    public int[] vertexIndices;
    public float[] vertexDeformation;

    public Instruction(int _id, int[] indices, float[] deformation) {
        id = _id;
        vertexIndices = indices;
        vertexDeformation = deformation;
    }
}
