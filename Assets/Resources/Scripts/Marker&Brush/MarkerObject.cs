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
    private int failedCallbacks;

    // Start is called before the first frame update
    void Start()
    {
        failedCallbacks = 0;
        if (gameObject.GetComponent<ASLObject>() != null) gameObject.GetComponent<ASLObject>()._LocallySetFloatCallback(MarkerCallback);
        //Debug.Log("This marker has the ASL Object script: " + (gameObject.GetComponent<ASLObject>() != null));
        theMesh = (gameObject.GetComponent<MeshRenderer>() != null) ?
            gameObject.GetComponent<MeshRenderer>() : gameObject.GetComponentInChildren<MeshRenderer>();
        Debug.Assert(theMesh != null);
        NormColor = theMesh.material.color;
        Selected = false;
    }

    /// <summary>
    /// Float callback. Attempt to remove
    /// </summary>
    /// <param name="_id"></param>
    /// <param name="_f"></param>
    private void MarkerCallback(string _id, float[] _f)
    {
        if (Selected) return;
        if (RouteDisplayV2.RemoveRouteMarker(gameObject.transform, true)) PlayerMarkerGenerator.RemoveMarker(gameObject);
        else OnFailedCallback();
    }

    public bool IsSelected { get { return Selected; } }


    /// <summary>
    /// Action performed when the user hovers over this object with the mouse
    /// </summary>
    void OnMouseEnter()
    {
        theMesh.material.color = HoverColor;
    }

    /// <summary>
    /// Action performed when the user stops hovering over the object
    /// </summary>
    void OnMouseExit()
    {
        if(!Selected) theMesh.material.color = NormColor;
    }

    /// <summary>
    /// Select or deselect this object
    /// </summary>
    /// <param name="IsSelected">Whether this object has been selected or deselected</param>
    public void Select(bool IsSelected)
    {
        Selected = IsSelected;

        if (!IsSelected) theMesh.material.color = NormColor;
    }

    /// <summary>
    /// Increment the failed callback counter. If this equals or exceeds the number of players, force the deletion of this object
    /// </summary>
    public void OnFailedCallback()
    {
        failedCallbacks++;
        if(failedCallbacks >= GameLiftManager.GetInstance().m_Players.Count)
        {
            Debug.Log("Forcing deletion of " + gameObject.name);
            ForceDeletion();
        }
    }

    /// <summary>
    /// Forces the deletion of a marker object because it is not "owned" by an active player
    /// </summary>
    private void ForceDeletion()
    {
        if (gameObject.GetComponent<ASL.ASLObject>() != null)
        {
            ASLObjectTrackingSystem.RemoveObjectToTrack(gameObject.GetComponent<ASL.ASLObject>());
            gameObject.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
            {
                gameObject.GetComponent<ASL.ASLObject>().DeleteObject();
            });
        }
        else
        {
            Destroy(gameObject.transform);
        }
    }
}
