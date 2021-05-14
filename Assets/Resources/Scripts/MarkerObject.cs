using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class MarkerObject : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (gameObject.GetComponent<ASLObject>() != null) gameObject.GetComponent<ASLObject>()._LocallySetFloatCallback(MarkerCallback);
    }

    private void MarkerCallback(string _id, float[] _f)
    {
        PlayerMarkerGenerator.DeletionCallback(gameObject);
    }
}
