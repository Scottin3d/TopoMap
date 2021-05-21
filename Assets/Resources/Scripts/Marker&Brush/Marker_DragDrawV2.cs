using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public static class Marker_DragDrawV2
{
    private static GameObject drawOrigin;
    private static GameObject drawLine;
    private static GameObject drawProjection;

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
}
