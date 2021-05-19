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
