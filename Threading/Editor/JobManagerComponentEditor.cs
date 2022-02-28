using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using JStuff.Threading;

[CustomEditor(typeof(JobManagerComponent))]
public class JobManagerComponentEditor : Editor
{
    SerializedProperty managerData;
    SerializedProperty updateDelay;

    private void OnEnable()
    {
        managerData = serializedObject.FindProperty("managerData");
        updateDelay = serializedObject.FindProperty("updateDelay");
    }

    public override void OnInspectorGUI()
    {
        if (Application.isPlaying)
        {
            EditorGUILayout.LabelField("Jobmanager Runtime", EditorStyles.boldLabel);
            EditorGUILayout.BeginFoldoutHeaderGroup(false, "Current number of threads");

            EditorGUILayout.EndFoldoutHeaderGroup();

            if (JobManagerComponent.instance != null)
            {
                EditorGUILayout.IntField(JobManagerComponent.instance.manager.NumberOfThreads);
            }
            return;
        }

        EditorGUILayout.PropertyField(managerData);
        EditorGUILayout.PropertyField(updateDelay);
        serializedObject.ApplyModifiedProperties();
    }
}