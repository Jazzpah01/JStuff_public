using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using JStuff.Threading;

[CustomPropertyDrawer(typeof(DedicatedThreads))]
public class DedicatedThreadsDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Don't make child fields be indented
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // Calculate rects
        var numberOfThreads = new Rect(position.x, position.y, 25, position.height);
        var priority = new Rect(position.x + 30, position.y, 25, position.height);

        // Draw fields - passs GUIContent.none to each so they are drawn without labels
        EditorGUI.PropertyField(numberOfThreads, property.FindPropertyRelative("numberOfThreads"), GUIContent.none);
        EditorGUI.PropertyField(priority, property.FindPropertyRelative("priority"), GUIContent.none);
        //EditorGUI.PropertyField(contextRect, property.FindPropertyRelative("context"), true);
        //EditorGUILayout.PropertyField()

        //var list = serializedObject.FindProperty("ListTest");
        //EditorGUILayout.PropertyField(list, new GUIContent("My List Test"), true);

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}