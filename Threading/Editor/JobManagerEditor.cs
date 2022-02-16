using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using JStuff.Threading;

[CustomEditor(typeof(JobManagerData))]
public class JobManagerEditor : Editor
{
    SerializedProperty threadAmount;
    SerializedProperty maxJobs;
    SerializedProperty allowInEditMode;
    SerializedProperty setThreads;

    SerializedProperty dedicatedThreads;
    SerializedProperty dedicatedThreadsEnabled;

    SerializedProperty numberOfPriorities;

    SerializedProperty threadArray;

    SerializedProperty consumeOnFrame;
    SerializedProperty advancedOptions;

    SerializedProperty overloadProtocol;

    SerializedProperty allowMultithreading;

    SerializedProperty consumeAsStartCoroutine;
    SerializedProperty updateProtocol;

    bool advanced = false;

    private void OnEnable()
    {
        threadAmount = serializedObject.FindProperty("threadAmount");
        maxJobs = serializedObject.FindProperty("maxJobs");
        allowInEditMode = serializedObject.FindProperty("allowInEditMode");
        setThreads = serializedObject.FindProperty("setThreads");
        threadArray = serializedObject.FindProperty("threads");
        consumeOnFrame = serializedObject.FindProperty("consumeOnFrame");

        dedicatedThreads = serializedObject.FindProperty("dedicatedThreads");
        dedicatedThreadsEnabled = serializedObject.FindProperty("dedicatedThreadsEnabled");

        numberOfPriorities = serializedObject.FindProperty("numberOfPriorities");

        advancedOptions = serializedObject.FindProperty("advancedOptions");

        overloadProtocol = serializedObject.FindProperty("overloadProtocol");

        allowMultithreading = serializedObject.FindProperty("allowMultithreading");

        consumeAsStartCoroutine = serializedObject.FindProperty("consumeAsStartCoroutine");
        updateProtocol = serializedObject.FindProperty("updateProtocol");
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(allowMultithreading);

        if (!allowMultithreading.boolValue)
        {
            serializedObject.ApplyModifiedProperties();
            return;
        }

        //EditorGUILayout.BeginFoldoutHeaderGroup(false, "JobManager properties");
        //EditorGUILayout.EndFoldoutHeaderGroup();

        //JobManager t = (JobManager)target;
        EditorGUILayout.LabelField("JobManager Settings", EditorStyles.boldLabel);


        EditorGUILayout.PropertyField(maxJobs);
        EditorGUILayout.PropertyField(setThreads);
        if (setThreads.boolValue)
        {
            EditorGUILayout.PropertyField(threadAmount);
        }

        // Advanced options
        bool oldAdvanced = advancedOptions.boolValue;

        EditorGUILayout.PropertyField(advancedOptions);


        if (advancedOptions.boolValue)
        {
            // Display advanced properties

            EditorGUILayout.PropertyField(consumeAsStartCoroutine);
            EditorGUILayout.PropertyField(updateProtocol);

            //if (consumeProtocol.enumValueIndex == 0)
            //{
            //    EditorGUILayout.PropertyField(consumeOnFrame);
            //}

            EditorGUILayout.PropertyField(overloadProtocol);
            EditorGUILayout.PropertyField(numberOfPriorities);
            if (numberOfPriorities.intValue > 1)
            {
                EditorGUILayout.PropertyField(dedicatedThreads);
                dedicatedThreadsEnabled.boolValue = true;
            }
        } else
        {
            if (oldAdvanced)
            {
                // Reset advanced properties to default values
                dedicatedThreadsEnabled.boolValue = false;
                numberOfPriorities.intValue = 1;
                overloadProtocol.enumValueIndex = 0;
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}