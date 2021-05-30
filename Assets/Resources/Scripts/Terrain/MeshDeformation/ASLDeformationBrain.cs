using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class ASLDeformationBrain : MonoBehaviour
{
    public List<GameObject> localMapChunks = new List<GameObject>();

    public GameObject testCube = null;
    static GameObject cube = null;
    static Vector3 pos;
    static float deltaY;

    private static ASLObject brain;
    public GenerateMapFromHeightMap mapGen = null;
    bool IsInit = false;

    static Queue<Instruction> instructions = new Queue<Instruction>();

    bool IsChange = false;
    // Start is called before the first frame update
    void Start()
    {
        cube = testCube;
        //testCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        brain = GetComponent<ASLObject>();
        //brain.GetComponent<ASLObject>()._LocallySetFloatCallback(MyFloatFunction);
        StartCoroutine(Initialize());


    }

    // Update is called once per frame
    void Update()
    {
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

        while (instructions.Count > 0) {
            Instruction i = instructions.Dequeue();
            ExecuteInstruction(i.instructionID, i.delta);
        }

    }

    private void ExecuteInstruction(int _id, float _delta) {
        Vector3 position = localMapChunks[_id].transform.position;
        position.y += _delta;
        localMapChunks[_id].transform.position = position;
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

        int chunkID = Convert.ToInt32(_myFloats[0]);
        float delta = _myFloats[1];

        Instruction i = new Instruction(chunkID, delta);
        instructions.Enqueue(i);
    }


}

public struct Instruction {
    public int instructionID;
    public float delta;

    public Instruction(int _id, float _delta) {
        instructionID = _id;
        delta = _delta;
    }
}
