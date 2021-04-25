using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor {
    public override void OnInspectorGUI() {
        MapGenerator mapGen = (MapGenerator)target;

        if (DrawDefaultInspector()) {
            if (mapGen.autoGenerate) {

                mapGen.DrawMapInEditor();
            }

        }

        if (GUILayout.Button("Generate Map")) {
            mapGen.DrawMapInEditor();
        }
    }
}
