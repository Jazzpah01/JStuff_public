using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

//https://catlikecoding.com/unity/tutorials/editor/custom-list/
[CustomEditor(typeof(CellDungeonTemplate))]
public class CellDungeonTemplateEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxConnections"));

        CellDungeonTemplate o = (CellDungeonTemplate)serializedObject.targetObject;

        MaxConnections maxConnections = o.maxConnections;

        int goWidth = 50;
        int rangeWidth = 150;

        EditorExtensions.SerializeFlatList(serializedObject.FindProperty("none"), goWidth, rangeWidth);

        if (maxConnections >= MaxConnections.one)
        {
            EditorExtensions.SerializeFlatList(serializedObject.FindProperty("right"), goWidth, rangeWidth);
            EditorExtensions.SerializeFlatList(serializedObject.FindProperty("up"), goWidth, rangeWidth);
            EditorExtensions.SerializeFlatList(serializedObject.FindProperty("left"), goWidth, rangeWidth);
            EditorExtensions.SerializeFlatList(serializedObject.FindProperty("down"), goWidth, rangeWidth);
        }

        if (maxConnections >= MaxConnections.two)
        {
            EditorExtensions.SerializeFlatList(serializedObject.FindProperty("right_up"), goWidth, rangeWidth);
            EditorExtensions.SerializeFlatList(serializedObject.FindProperty("up_left"), goWidth, rangeWidth);
            EditorExtensions.SerializeFlatList(serializedObject.FindProperty("left_down"), goWidth, rangeWidth);
            EditorExtensions.SerializeFlatList(serializedObject.FindProperty("right_down"), goWidth, rangeWidth);

            EditorExtensions.SerializeFlatList(serializedObject.FindProperty("up_down"), goWidth, rangeWidth);
            EditorExtensions.SerializeFlatList(serializedObject.FindProperty("right_left"), goWidth, rangeWidth);
        }

        if (maxConnections >= MaxConnections.three)
        {
            EditorExtensions.SerializeFlatList(serializedObject.FindProperty("right_up_left"), goWidth, rangeWidth);
            EditorExtensions.SerializeFlatList(serializedObject.FindProperty("up_left_down"), goWidth, rangeWidth);
            EditorExtensions.SerializeFlatList(serializedObject.FindProperty("right_left_down"), goWidth, rangeWidth);
            EditorExtensions.SerializeFlatList(serializedObject.FindProperty("right_up_down"), goWidth, rangeWidth);
        }

        if (maxConnections >= MaxConnections.four)
        {
            EditorExtensions.SerializeFlatList(serializedObject.FindProperty("right_up_left_down"), goWidth, rangeWidth);
        }

        serializedObject.ApplyModifiedProperties();
    }
}