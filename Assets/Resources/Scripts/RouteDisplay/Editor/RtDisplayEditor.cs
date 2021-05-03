using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RouteDisplayV2))]
public class RtDisplayEditor : Editor
{
    private RouteDisplayV2 display;

    private void OnSceneGUI()
    {
        display = target as RouteDisplayV2;
        if(display.myPath != null)
        {
            Handles.color = display.GetColor();
            for(int ndx = 0; ndx < display.myPath.vectorPath.Count - 1; ndx++)
            {
                Handles.DrawLine(display.myPath.vectorPath[ndx], display.myPath.vectorPath[ndx + 1], 5);
            }
        }
    }
}
