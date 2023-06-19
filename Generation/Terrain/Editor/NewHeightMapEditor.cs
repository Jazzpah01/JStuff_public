using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.Generation.Terrain;
using UnityEditor;

[CustomEditor(typeof(NewHeightMap), true)]
public class NewHeightMapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        NewHeightMap t = serializedObject.targetObject as NewHeightMap;

        if (t != null)
        {
            Texture2D texture = t.GetTexture();

            if (texture != null)
            {
                Rect rect = new Rect(Vector2.zero, Vector2.one * 150);

                EditorGUI.DrawPreviewTexture(rect, texture);

                EditorGUILayout.Space(150);
            }
        }
        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();
        if (EditorGUI.EndChangeCheck())
        {
            t.visualizationSeed = -1;
        }
    }
}
