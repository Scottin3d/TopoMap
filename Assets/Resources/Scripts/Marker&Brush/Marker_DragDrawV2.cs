using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using ASL;

public static class Marker_DragDrawV2
{
    private static GameObject drawOrigin;
    private static GameObject drawLine;
    private static GameObject drawProjection;

    private static int fingerID = -1;

    #region RAYCAST_INPUT

    /// <summary>
    /// Raycasts in response to user action, and initiates draw & draw functionality if a marker object is hit.
    /// </summary>
    /// <param name="LShift">If left shift is held</param>
    /// <param name="mouseRay">The ray used to calculate raycasts</param>
    /// <param name="_sm">The small map</param>
    /// <param name="_lm">The large map</param>
    public static void ClickCast(bool LShift, Ray mouseRay, GameObject _sm, GameObject _lm)
    {
        RaycastHit hit;
        if (Physics.Raycast(mouseRay, out hit, 1000f))
        {
            if (EventSystem.current.IsPointerOverGameObject(fingerID)) return;
            
            if (LShift)
            {
                bool ToTable = (hit.collider.transform.parent.tag == "SpawnSmallMap");
                Vector3 markerPos = Vector3.zero;
                if (ToTable)
                {
                    markerPos = (hit.point - _sm.transform.position) * MarkerDisplay.GetScaleFactor() + _lm.transform.position;
                }
                else
                {
                    markerPos = hit.point;
                }
                MarkerGeneratorV2.InstantiateMarker("Marker", markerPos, ToTable);
            }
            else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Markers"))
            {
                MarkerGeneratorV2.SelectObject(hit.collider.gameObject);
            }
            else
            {
                MarkerGeneratorV2.DeselectObject();
            }
        }
        else
        {
            MarkerGeneratorV2.DeselectObject();
        }
    }

    /// <summary>
    /// Raycasts while action is held, provided drag & draw was successfully initiated. Used as a visual for the end state of the cast.
    /// </summary>
    /// <param name="mouseRay">The ray used in raycast calculations</param>
    /// <param name="drawTime">Time remaining before draw cast begins</param>
    public static void HoldCast(Ray mouseRay, float drawTime)
    {
        if (!HasDrawOrigin) return;
        bool FromLargeMap = OriginLargeMap();
        int layerMask = (FromLargeMap) ? LayerMask.GetMask("Ground") : LayerMask.GetMask("Holomap");
        RaycastHit hit;
        if (Physics.Raycast(mouseRay, out hit, 1000f, layerMask))
        {
            if (EventSystem.current.IsPointerOverGameObject(fingerID)) DrawCast(Vector3.zero, FromLargeMap, false);

            DrawCast(hit.point, FromLargeMap, drawTime <= 0f);
        }
        else
        {
            DrawCast(Vector3.zero, FromLargeMap, false);
        }
    }

    /// <summary>
    /// Raycasts at the completion of action, provided drag & draw was successfully initiated.
    /// </summary>
    /// <param name="mouseRay">The ray used in raycast calculations</param>
    /// <param name="_sm">The small map</param>
    /// <param name="_lm">The large map</param>
    /// <param name="drawTime">Time remaining before draw cast begins</param>
    public static void ReleaseCast(Ray mouseRay, GameObject _sm, GameObject _lm, float drawTime)
    {
        bool FromLargeMap = OriginLargeMap();
        int layerMask = (FromLargeMap) ? LayerMask.GetMask("Ground") : LayerMask.GetMask("Holomap");
        Debug.Log("Link from large map: " + FromLargeMap);
        Vector3 markerPos = Vector3.zero;

        RaycastHit hit;
        if (Physics.Raycast(mouseRay, out hit, 1000f, layerMask))
        {
            if (EventSystem.current.IsPointerOverGameObject(fingerID)) { DrawFinish(Vector3.zero, false);  return; }
            if (!FromLargeMap)
            {
                markerPos = (hit.point - _sm.transform.position) * MarkerDisplay.GetScaleFactor() + _lm.transform.position;
            }
            else
            {
                markerPos = hit.point;
            }

            DrawFinish(markerPos, drawTime <= 0f);
        }
        else
        {
            DrawFinish(Vector3.zero, false);
        }
    }

    #endregion

    #region POST_RAYCAST

    /// <summary>
    /// Sets the origin of drag & draw cast functions.
    /// </summary>
    /// <param name="_g">The marker to be used as the origin</param>
    public static void SetDrawOrigin(GameObject _g)
    {
        if (!_g.Equals(drawOrigin))
        {
            drawOrigin = _g;
        }

        drawProjection = GameObject.Instantiate(_g) as GameObject;
        Component.Destroy(drawProjection.GetComponent<ASLObject>());
        Component.Destroy(drawProjection.GetComponent<BoxCollider>());
        drawProjection.layer = LayerMask.NameToLayer("IgnoreMinimap");

        if(drawLine == null)
        {
            drawLine = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Component.Destroy(drawLine.GetComponent<CapsuleCollider>());
            drawLine.GetComponent<MeshRenderer>().material.color = Color.cyan;
            drawLine.layer = LayerMask.NameToLayer("IgnoreMinimap");
            drawLine.SetActive(false);
        }

        drawOrigin.GetComponent<MarkerObject>().Select(true);
    }

    /// <summary>
    /// Raycasts to place the projection marker where the user is looking
    /// </summary>
    /// <param name="castPoint"></param>
    /// <param name="IsOnLargeMap">Whether the cast is being performed on the large map</param>
    /// <param name="SuccessfulCast">Whether the raycast was successful</param>
    public static void DrawCast(Vector3 castPoint, bool IsOnLargeMap, bool SuccessfulCast)
    {
        if (MarkerGeneratorV2.HasSelectedObject)
        {
            float thickness = (IsOnLargeMap) ? 0.25f : 0.01f;
            if (drawProjection != null) drawProjection.transform.position = castPoint;

            Vector3 line = (drawProjection.transform.position - drawOrigin.transform.position);
            drawLine.transform.localScale = new Vector3(thickness, line.magnitude * 0.5f, thickness);
            drawLine.transform.position = drawOrigin.transform.position + 0.5f * line;
            drawLine.transform.up = line;

            drawLine.SetActive(SuccessfulCast);
            if (drawProjection != null) drawProjection.SetActive(SuccessfulCast);
        } else
        {
            drawLine.SetActive(false);
            if (drawProjection != null) drawProjection.SetActive(false);
        }        
    }

    public static void DrawFinish(Vector3 castPoint, bool SuccessfulCast)
    {
        if (SuccessfulCast)
        {
            MarkerGeneratorV2.InstantiateMarker(drawOrigin, "Marker", castPoint);
        }

        if (drawLine != null) drawLine.SetActive(false);
        Transform.Destroy(drawProjection);
    }

    #endregion

    /// <summary>
    /// Determines whether the current drag & draw has an origin marker
    /// </summary>
    public static bool HasDrawOrigin { get { return drawOrigin != null; } }

    /// <summary>
    /// Determins whether the origin marker is on the large map
    /// </summary>
    /// <returns>Returns true if drawOrigin is placed on the large map. Returns false if non-existent or on the small map</returns>
    public static bool OriginLargeMap()
    {
        if (drawOrigin == null) return false;
        return (ASLObjectTrackingSystem.GetObjects().Contains(drawOrigin.transform));
    }
}
