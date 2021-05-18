using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class RouteObject : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (gameObject.GetComponent<ASLObject>() != null) gameObject.GetComponent<ASLObject>()._LocallySetFloatCallback(RouteCallback);
    }

    /// <summary>
    /// Sets color, UV scaling, and transparency for this route object
    /// </summary>
    /// <param name="_id">The ID of the ASLObject</param>
    /// <param name="_f">The array of floats sent to each client</param>
    private void RouteCallback(string _id, float[] _f)
    {
        Color theColor = new Color(_f[0], _f[1], _f[2], _f[3]);
        gameObject.GetComponent<MeshRenderer>().material.SetColor("_mainColor", theColor);
        gameObject.GetComponent<MeshRenderer>().material.SetColor("_lineColor", theColor);
        gameObject.GetComponent<MeshRenderer>().material.SetFloat("_height", _f[4]);
        gameObject.GetComponent<MeshRenderer>().material.SetFloat("_transparency", _f[5]);
    }
}
