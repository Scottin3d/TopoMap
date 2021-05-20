using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DisplayManager))]
public class RtDisplayEditor : Editor
{
    private DisplayManager display;

    private void OnSceneGUI()
    {
        display = target as DisplayManager;
        if(RouteDisplayV2.myPath != null)
        {
            Handles.color = display.GetColor();
            for(int ndx = 0; ndx < RouteDisplayV2.myPath.vectorPath.Count - 1; ndx++)
            {
                Handles.DrawLine(RouteDisplayV2.myPath.vectorPath[ndx], RouteDisplayV2.myPath.vectorPath[ndx + 1], 5);
            }
        }
    }
}
