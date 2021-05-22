using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public static class MarkerGeneratorV2
{
    private static List<GameObject> markerList = new List<GameObject>();
    private static GameObject insertPoint;
    private static GameObject selectedObject;

    private static int MarkerCount { get { return markerList.Count; } }
    public static bool HasSelectedObject { get { return selectedObject != null; } }

    public static void SelectObject(GameObject _selected)
    {
        selectedObject = _selected;
        if (_selected != null)
        {
            if (_selected.GetComponent<MarkerObject>() != null) _selected.GetComponent<MarkerObject>().Select(true);
            Marker_DragDrawV2.SetDrawOrigin(_selected);
        }
    }

    public static void DeselectObject()
    {
        if(selectedObject != null)
        {
            if (selectedObject.GetComponent<MarkerObject>() != null) selectedObject.GetComponent<MarkerObject>().Select(false);
            selectedObject = null;
        }
    }    

    #region CREATION

    /// <summary>
    /// Instantiates a prefab as a marker from either the large or small maps.
    /// Adds marker to the end of the marker list
    /// </summary>
    /// <param name="optionValue">The string name of the prefab to instantiate</param>
    /// <param name="pos">The position of the instantiated marker object</param>
    /// <param name="IsOnLargeMap">Is this marker being instantiated on the large map</param>
    public static void InstantiateMarker(string optionValue, Vector3 pos, bool IsOnLargeMap)
    {
        if(IsOnLargeMap)
        {
            ASLHelper.InstantiateASLObject(optionValue, pos, Quaternion.identity, "", "", GetLargerFromLarger);
        } else
        {
            ASLHelper.InstantiateASLObject(optionValue, pos, Quaternion.identity, "", "", GetLargerFromSmaller);
        }
    }

    /// <summary>
    /// Instantiates a prefab as a marker using an existing marker as an insertion point.
    /// </summary>
    /// <param name="actObject">The object to use as an insertion point</param>
    /// <param name="optionValue">The string name of the prefab to instantiate</param>
    /// <param name="pos">The position of the instantiated marker object</param>
    public static void InstantiateMarker(GameObject actObject, string optionValue, Vector3 pos)
    {
        insertPoint = actObject;
        ASLHelper.InstantiateASLObject(optionValue, pos, Quaternion.identity, "", "", InsertMarker);
    }

    /// <summary>
    /// Callback for instantiating a marker from the large map
    /// </summary>
    /// <param name="_myGameObject">The game object that initiated this callback</param>
    private static void GetLargerFromLarger(GameObject _myGameObject)
    {
        _myGameObject.transform.parent = MarkerStorage.Instance.gameObject.transform;
        ASLObjectTrackingSystem.AddObjectToTrack(_myGameObject.GetComponent<ASL.ASLObject>());
        RouteDisplayV2.AddRouteMarker(_myGameObject.transform);
        markerList.Add(_myGameObject);
        Marker_DragDrawV2.SetDrawOrigin(_myGameObject);
    }

    /// <summary>
    /// Callback for instantiating a marker from the small map
    /// </summary>
    /// <param name="_myGameObject">THe game object that initiated this callback</param>
    private static void GetLargerFromSmaller(GameObject _myGameObject)
    {
        _myGameObject.transform.parent = MarkerStorage.Instance.gameObject.transform;
        ASLObjectTrackingSystem.AddObjectToTrack(_myGameObject.GetComponent<ASL.ASLObject>());
        RouteDisplayV2.AddRouteMarker(_myGameObject.transform);
        markerList.Add(_myGameObject);
    }

    /// <summary>
    /// Callback for instantiating via insertion
    /// </summary>
    /// <param name="_myGameObject">The game object that initiated this callback</param>
    private static void InsertMarker(GameObject _myGameObject)
    {
        _myGameObject.transform.parent = MarkerStorage.Instance.gameObject.transform;
        ASLObjectTrackingSystem.AddObjectToTrack(_myGameObject.GetComponent<ASL.ASLObject>());
        int insertNdx = RouteDisplayV2.InsertMarkerAt(insertPoint.transform, _myGameObject.transform);
        //how to grab corresponding marker from tracked markers?
        if (insertNdx < 0)
        {
            markerList.Add(_myGameObject);
        }
        else
        {
            markerList.Insert(insertNdx + 1, _myGameObject);
        }
    }

    #endregion   

    #region DELETION

    /// <summary>
    /// Deletes the currently selected object, if one is selected.
    /// </summary>
    public static void DeleteSelected()
    {
        if (selectedObject != null) CallDelete(selectedObject);
    }

    /// <summary>
    /// Deletes the marker at the end of the marker list
    /// </summary>
    public static void DeleteLastPlaced()
    {
        if(MarkerCount > 0) CallDelete(markerList[MarkerCount - 1]);
    }

    /// <summary>
    /// Deletes all markers in the marker list, starting from the front of the list
    /// </summary>
    public static void DeleteAll()
    {
        int ndx = 0;
        while (MarkerCount > ndx) if(!CallDelete(markerList[ndx])) ndx++;
    }

    /// <summary>
    /// Private deletion call for readability
    /// </summary>
    /// <param name="_g">The game object being deleted</param>
    private static bool CallDelete(GameObject _g)
    {
        MarkerObject _mo = _g.GetComponent<MarkerObject>();
        if(_mo != null)
        {
            if (_mo.IsSelected) return false;
        }
        ASLObject _asl = _g.GetComponent<ASLObject>();
        if(_asl != null)
        {
            if (RouteDisplayV2.RemoveRouteMarker(_g.transform, false))
            {
                markerList.Remove(_g);
                ASLObjectTrackingSystem.RemoveObjectToTrack(_asl);
                _asl.SendAndSetClaim(() =>
                {
                    _asl.DeleteObject();
                });
            }
            else
            {
                float[] _f = new float[1];
                _asl.SendAndSetClaim(() =>
                {
                    _asl.SendFloatArray(_f);
                });
                return false;
            }
        } else
        {
            RouteDisplayV2.RemoveRouteMarker(_g.transform, false);
            markerList.Remove(_g);
            Transform.Destroy(_g.transform);
        }
        return true;
    }

    #endregion
}

public class MarkerStorage : MonoBehaviour
{
    private static MarkerStorage _Instance;

    public static MarkerStorage Instance
    {
        get
        {
            if (_Instance == null) _Instance = new GameObject("MarkerStorage").AddComponent<MarkerStorage>();
            return _Instance;
        }
    }

}