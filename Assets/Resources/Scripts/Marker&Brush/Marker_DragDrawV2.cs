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

    public static void ClickCast(bool LShift, bool ToTable, Ray mouseRay, GameObject _sm, GameObject _lm, string optionValue)
    {
        RaycastHit hit;
        if (Physics.Raycast(mouseRay, out hit, 1000f))
        {
            if (EventSystem.current.IsPointerOverGameObject(fingerID)) return;

            if (LShift)
            {
                
                Vector3 markerPos = Vector3.zero;
                if (ToTable)
                {
                    markerPos = (hit.point - _sm.transform.position) * MarkerDisplay.GetScaleFactor() + _lm.transform.position;
                }
                else
                {
                    markerPos = hit.point;
                }
                MarkerGeneratorV2.InstantiateMarker(optionValue, markerPos, ToTable);
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

    public static void HoldCast(Ray mouseRay, float drawTime)
    {
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

    public static void ReleaseCast(Ray mouseRay, GameObject _sm, GameObject _lm, string optionValue, float drawTime)
    {
        bool FromLargeMap = OriginLargeMap();
        int layerMask = (FromLargeMap) ? LayerMask.GetMask("Ground") : LayerMask.GetMask("Holomap");

        Vector3 markerPos = Vector3.zero;

        RaycastHit hit;
        if (Physics.Raycast(mouseRay, out hit, 1000f, layerMask))
        {
            if (EventSystem.current.IsPointerOverGameObject(fingerID)) DrawFinish(Vector3.zero, optionValue, false);
            if (!FromLargeMap)
            {
                markerPos = (hit.point - _sm.transform.position) * MarkerDisplay.GetScaleFactor() + _lm.transform.position;
            }
            else
            {
                markerPos = hit.point;
            }

            DrawFinish(markerPos, optionValue, drawTime <= 0f);
        }
        else
        {
            DrawFinish(Vector3.zero, optionValue, false);
        }
    }

    #endregion

    #region POST_RAYCAST

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

    public static void DrawCast(Vector3 castPoint, bool IsOnLargeMap, bool SuccessfulCast)
    {
        if (MarkerGeneratorV2.HasSelectedObject)
        {
            float thickness = (IsOnLargeMap) ? 0.25f : 0.01f;
            drawProjection.transform.position = castPoint;

            Vector3 line = (drawProjection.transform.position - drawOrigin.transform.position);
            drawLine.transform.localScale = new Vector3(thickness, line.magnitude * 0.5f, thickness);
            drawLine.transform.position = drawOrigin.transform.position + 0.5f * line;
            drawLine.transform.up = line;

            drawLine.SetActive(SuccessfulCast);
            drawProjection.SetActive(SuccessfulCast);
        } else
        {
            drawLine.SetActive(false);
            drawProjection.SetActive(false);
        }        
    }

    public static void DrawFinish(Vector3 castPoint, string optionValue, bool SuccessfulCast)
    {
        if (SuccessfulCast)
        {
            MarkerGeneratorV2.InstantiateMarker(drawOrigin, optionValue, castPoint);
        }

        drawLine.SetActive(false);
        Transform.Destroy(drawProjection);
    }

    #endregion

    public static bool HasDrawOrigin { get { return drawOrigin != null; } }

    public static bool OriginLargeMap()
    {
        return (ASLObjectTrackingSystem.GetObjects().Contains(drawOrigin.transform));
    }
}
