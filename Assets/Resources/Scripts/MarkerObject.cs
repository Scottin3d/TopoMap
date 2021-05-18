using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class MarkerObject : MonoBehaviour
{
    private MeshRenderer theMesh;
    private Color HoverColor = new Color(1f, 1f, 0, 1f);
    private Color NormColor;

    private bool Selected;

    // Start is called before the first frame update
    void Start()
    {
        if (gameObject.GetComponent<ASLObject>() != null) gameObject.GetComponent<ASLObject>()._LocallySetFloatCallback(MarkerCallback);
        theMesh = (gameObject.GetComponent<MeshRenderer>() != null) ?
            gameObject.GetComponent<MeshRenderer>() : gameObject.GetComponentInChildren<MeshRenderer>();
        Debug.Assert(theMesh != null);
        NormColor = theMesh.material.color;
        Selected = false;
    }

    private void MarkerCallback(string _id, float[] _f)
    {
        PlayerMarkerGenerator.DeletionCallback(gameObject);
    }

    void OnMouseEnter()
    {
        theMesh.material.color = HoverColor;
    }

    void OnMouseExit()
    {
        if(!Selected) theMesh.material.color = NormColor;
    }

    public void Select(bool IsSelected)
    {
        Selected = IsSelected;
        if (!IsSelected) theMesh.material.color = NormColor;
    }
}
