using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using JStuff.Collections;

[CustomPropertyDrawer(typeof(DropDownList))]
public class DropDownListDrawer : PropertyDrawer
{
    //https://answers.unity.com/questions/1612072/how-to-create-a-drop-down-menu-in-inspector.html
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty userIndexProperty = property.FindPropertyRelative("_index");
        SerializedProperty values = property.FindPropertyRelative("values");

        string[] nvalues = new string[values.arraySize];

        for (int i = 0; i < values.arraySize; i++)
        {
            nvalues[i] = values.GetArrayElementAtIndex(i).stringValue;
        }

        EditorGUI.BeginChangeCheck();
        int _choiceIndex = EditorGUI.Popup(position, userIndexProperty.intValue, nvalues);
        if (EditorGUI.EndChangeCheck())
        {
            //userIndexProperty.intValue = _choiceIndex;
            (fieldInfo.GetValue(property.serializedObject.targetObject) as DropDownList).index = _choiceIndex;
        }
    }
}