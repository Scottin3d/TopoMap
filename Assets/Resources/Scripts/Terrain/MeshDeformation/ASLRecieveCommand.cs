using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;
public class ASLRecieveCommand : MonoBehaviour
{
    public static Transform t;
    public static ASLObject a;

    private void Start() {
        t = transform;
    }

    public static void MoveCubeExecute(float f) {
            Vector3 pos = t.position;
            pos.y += f;
            t.position = pos;
        
    }

    public static void MyFloatFunction(string _id, float[] _myFloats) {
        if (ASL.ASLHelper.m_ASLObjects.TryGetValue(_id, out ASL.ASLObject myObject)) {
            Debug.Log("The name of the object that sent these floats is: " + myObject.name);
        }

        MoveCubeExecute(_myFloats[0]);
    }
}
