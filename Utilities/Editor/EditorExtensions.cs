using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//https://catlikecoding.com/unity/tutorials/editor/custom-list/
//https://forum.unity.com/threads/propertyfield-without-a-label.960150/
//https://www.youtube.com/watch?v=RImM7XYdeAc
public static class EditorExtensions
{
    public static void SerializeFlatList(SerializedProperty list, params int[] sizes)
	{
		list.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(list.isExpanded, list.displayName);

		if (!list.isExpanded)
        {
			EditorGUILayout.EndFoldoutHeaderGroup();
			return;
		}

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Size");
		list.arraySize = EditorGUILayout.IntField(list.arraySize);
		EditorGUILayout.EndHorizontal();

		if (list.arraySize == 0)
        {
			EditorGUILayout.EndFoldoutHeaderGroup();
			return;
		}

		int objectAmount = list.GetArrayElementAtIndex(0).CountInProperty();

		GUILayout.ExpandWidth(false);

		EditorGUI.indentLevel += 1;
		EditorGUILayout.BeginHorizontal();
		int j = 0;
		int totalWidth = Screen.width - 15 * 2;// 15 is indent width
		foreach (SerializedProperty item in list.GetArrayElementAtIndex(0))
        {
			if (sizes != null && j < sizes.Length)
			{
				EditorGUILayout.LabelField(item.displayName, GUILayout.MinWidth(sizes[j]));
				GUILayout.FlexibleSpace();
				j++;
			} else
			{
				EditorGUILayout.LabelField(item.displayName, GUILayout.MinWidth(30));
				GUILayout.FlexibleSpace();
			}
		}
		EditorGUILayout.EndHorizontal();

		for (int i = 0; i < list.arraySize; i++)
		{
			EditorGUILayout.BeginHorizontal();
			j = 0;
			foreach (SerializedProperty item in list.GetArrayElementAtIndex(i))
            {
				if (sizes != null && j < sizes.Length)
				{
					EditorGUILayout.PropertyField(item, GUIContent.none, GUILayout.MinWidth(sizes[j]));
					GUILayout.FlexibleSpace();
					j++;
				}
				else
				{
					EditorGUILayout.PropertyField(item, GUIContent.none, GUILayout.MinWidth(30));
					GUILayout.FlexibleSpace();
				}
			}
			EditorGUILayout.EndHorizontal();
		}
		EditorGUI.indentLevel -= 1;
		EditorGUILayout.EndFoldoutHeaderGroup();
	}
}