using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using JStuff.Generation;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapGenerator s = (MapGenerator)target;

        DrawDefaultInspector();

        if (GUILayout.Button("Initialize"))
        {
            s.Initialize();
        }

        if (GUILayout.Button("Cleanup"))
        {
            s.Clean();
        }

        //if (s.transform.hasChanged)
        //{
        //    s.UpdateMeshes();
        //}
    }
}