using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class ASLDeformationBrain : MonoBehaviour
{

    public static GameObject testCube = null;
    private static ASLObject brain;
    // Start is called before the first frame update
    void Start()
    {
        //testCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        brain = GetComponent<ASLObject>();
        SpawnMap();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            DeformMesh(1f);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            DeformMesh(-1f);
        }
    }

    private void SpawnMap() {
            ASLHelper.InstantiateASLObject("Cube", Vector3.zero, Quaternion.identity, null, null, OnChunkCreated);
    }

    private static void OnChunkCreated(GameObject _gameObject) {
        testCube = _gameObject;
        testCube.GetComponent<ASLObject>()._LocallySetFloatCallback(ASLRecieveCommand.MyFloatFunction);
    }

    // eventually take in
    private void DeformMesh(float f) {
        ASLObject cube = testCube.GetComponent<ASLObject>();



        cube.SendAndSetClaim(() => {
            //cube.SendMessage("MoveCubeExecute", f);
            //float[] message = new float[] { 0, f};
            //cube.SendFloatArray(message);


            // convert affected vertex list to float[]
            // convert V3 to float[]
            //cube.SendAndDeformMesh();
        });
    }

    
}
