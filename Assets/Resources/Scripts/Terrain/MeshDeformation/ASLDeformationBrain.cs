using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

/// <summary>
/// ASLDeformationBrain is the core logic for handling the manipulation of chunk meshes using the ASL library.  
/// Deformation is broken down into payloads sent over ASL and then reconstructed.  Beause ASL function callbacks
/// are handled asynchronously, when a payload is recieved, it is reconstructed into an instruction object and added
/// to a queue that will then continuously try to execute in the Update() function.
/// </summary>
public class ASLDeformationBrain : MonoBehaviour {
    public static ASLDeformationBrain current;

    public List<GameObject> localMapChunks = new List<GameObject>();

    public GameObject testCube = null;
    static GameObject cube = null;
    static Vector3 pos;
    static float deltaY;

    static Queue<Instruction> instructions = new Queue<Instruction>();
    private static ASLObject brain;
    public GenerateMapFromHeightMap mapGen = null;
    bool IsInit = false;

    /// <summary>
    /// Set a static reference to the script for access outside of scope.
    /// </summary>
    private void Awake() {
        current = this;
    }

    /// <summary>
    /// Assing the ASL brian and initialize variables.
    /// </summary>
    void Start()
    {
        brain = GetComponent<ASLObject>();
        StartCoroutine(Initialize());
    }

    /// <summary>
    /// Initalizes the chunk objects from the map generator script.
    /// </summary>
    IEnumerator Initialize() {
        brain.GetComponent<ASLObject>()._LocallySetFloatCallback(MyFloatFunction);

        while (!mapGen.IsGenerated) {
            yield return new WaitForSeconds(0.1f);
        }

        mapGen.GetMapChunks(ref localMapChunks);
        IsInit = true;
    }

    /// <summary>
    /// Executes instructions if they exist in the queue.
    /// </summary>
    void Update() {
        while (IsInit && instructions.Count > 0) {
            ExecuteInstruction(instructions.Dequeue());
        }
    }

    /// <summary>
    /// Converts a deform instruction into a float array and send it over ASL.
    /// </summary>
    /// <param name="i">The deform instruction object</param>
    public void QueueInstruction(DeformInstruction i) {
        GameObject chunk = localMapChunks[i.id];
        chunk.TryGetComponent<ChunkData>(out ChunkData chunkData);

        // selected chunk
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

            // a pay load is a single mesh instruction
            float[] payload = CombineFloatArrays<float>(id, vertexCount, affectedVertexIndices, affectedVertexStrength);

            // send payload
            brain.SendAndSetClaim(() => {
                brain.SendFloatArray(payload);
            });
        }

        // selected chunk neighbors
        for (int n = 1; n < i.deformVertices.Count; n++) {
            // check if neighbor exists
            MapChunk neighborChunk = chunkData.MapChunk.chunkNeighbors[n - 1];

            if (neighborChunk != null) {
                // [0] -- chunk id
                int chunkID = neighborChunk.chunkID;
                float[] id = new float[1] { Convert.ToSingle(chunkID) };

                // [1] -- vertex count
                int count = i.deformVertices[n].Count;
                float[] vertexCount = new float[1] { Convert.ToSingle(count) };

                // [2] -- vertex indices
                float[] affectedVertexIndices = new float[i.deformVertices[n].Count];
                // [3] -- vertex deltas
                float[] affectedVertexStrength = new float[i.deformVertices[n].Count];
                int c = 0;
                foreach (var v in i.deformVertices[n]) {
                    affectedVertexIndices[c] = i.deformVertices[n][c].index;
                    affectedVertexStrength[c] = i.deformVertices[n][c].defromStrength;
                    c++;
                }

                // a pay load is a single mesh instruction
                float[] payload = CombineFloatArrays<float>(id, vertexCount, affectedVertexIndices, affectedVertexStrength);

                // send payload
                brain.SendAndSetClaim(() => {
                    brain.SendFloatArray(payload);
                });
            }
        }
    }

    /// <summary>
    /// Executes a mesh deformation instruction.  Modified selected vertices.
    /// </summary>
    /// <param name="i">The instruction object</param>
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

        chunk.GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    /// <summary>
    /// Splits a float[] payload into a list of instruction steps.
    /// </summary>
    /// <param name="_payload">The float[] to be split</param>
    /// <returns>The instruction steps in list form</returns>
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

    /// <summary>
    /// Combines 2 to 4 arrays.  
    /// </summary>
    /// <param name="array1">The first array</param>
    /// <param name="array2">The second array</param>
    /// <param name="array3">The third array (optional)</param>
    /// <param name="array4">The fourth array (optional)</param>
    /// <returns></returns>
    public T[] CombineFloatArrays<T>(T[] array1, T[] array2, T[] array3 = default, T[] array4 = default) {
        // lengths
        int a1 = array1.Length;
        int a2 = array2.Length;
        int a3 = (array3.Length > 0) ? array3.Length : 0;
        int a4 = (array4.Length > 0) ? array4.Length : 0;

        // new array
        T[] array = new T[a1 + a2 + a3 + a4];

        // add array 1 elements -- id
        int a = 0;
        for (int i = 0; i < a1; i++, a++) {
            array[a] = array1[i];
        }
        // add array 2 elements -- vertex count
        for (int i = 0; i < a2; i++, a++) {
            array[a] = array2[i];
        }
        if (a3 > 0) {
            // add array 3 elements -- vertex indices
            for (int i = 0; i < a3; i++, a++) {
                array[a] = array3[i];
            }
        }

        if (a4 > 0) {
            // add array 4 elements -- vertex deformation (Y)
            for (int i = 0; i < a4; i++, a++) {
                array[a] = array4[i];
            }
        }
        return array;
    }

    /// <summary>
    /// The SendFloatArray Callback function associated with ASL and the deformation brain.
    /// </summary>
    /// <param name="_id">The ASLObject id</param>
    /// <param name="_myFloats">The float array payload</param>
    public static void MyFloatFunction(string _id, float[] _myFloats) {
        if (ASL.ASLHelper.m_ASLObjects.TryGetValue(_id, out ASL.ASLObject myObject)) {
            Debug.Log("The name of the object that sent these floats is: " + myObject.name);
        }

        int chunkID = Convert.ToInt32(_myFloats[0]);
        float delta = _myFloats[1];

        Instruction i = new Instruction(chunkID, delta);
        instructions.Enqueue(i);
    }


}

/// <summary>
/// The structure of a deformation instruction
/// </summary>
public struct Instruction {
    public int instructionID;
    public float delta;

    public Instruction(int _id, float _delta) {
        instructionID = _id;
        delta = _delta;
    }
}
