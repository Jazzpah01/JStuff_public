using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NavRegion))]
public class NavRegionEditor : Editor
{
    public Vector2[,] points;
    public float density = 0;
    public float standardAgentSize = 0;
    public Vector2 minPosition = new Vector2(0, 0);
    public Vector2 maxPosition = new Vector2(0, 0);

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        base.OnInspectorGUI();

        //EditorGUILayout.PropertyField(serializedObject.FindProperty("isSetup"));

        SerializedProperty navSys = serializedObject.FindProperty("navigationSystem");

        if (navSys.objectReferenceValue == null)
        {
            GameObject gameObject = GameObject.Find("NavigationSystem");

            if (gameObject == null)
            {
                Debug.LogError("There is no navigation system in the scene. NavigationSystem GameObject must have name 'NavigationSystem'.");
            }

            Debug.Log("Setting navSys");
            navSys.objectReferenceValue = gameObject.GetComponent<NavigationSystem>();
        } else
        {
            NavigationSystem navSysComp = (NavigationSystem) navSys.objectReferenceValue;
            //serializedObject.FindProperty("up").enumValueFlag = (int)navSysComp.up;

            serializedObject.FindProperty("standardAgentSize").floatValue = navSysComp.standardAgentSize;
            serializedObject.FindProperty("density").floatValue = navSysComp.density;
        }

        serializedObject.ApplyModifiedProperties();

        if (GUILayout.Button("Print Stationaries"))
        {
            NavRegion obj = serializedObject.targetObject as NavRegion;

            Debug.Log(obj.StationaryToString());
        }
    }

    public void OnSceneGUI()
    {
        NavRegion r = this.target as NavRegion;

        if (r.standardAgentSize <= 0 || r.density <= 0)
            return;

        if (points == null || points.Length == 0 || 
            r.density != density || r.standardAgentSize != standardAgentSize || minPosition != r.MinPosition || maxPosition != r.MaxPosition)
        {
            points = r.GetNodes(r.standardAgentSize);
            density = r.density;
            standardAgentSize = r.standardAgentSize;
            minPosition = r.MinPosition;
            maxPosition = r.MaxPosition;
        }

        if (points == null)
            return;

        foreach (Vector2 v in points)
        {
            Handles.DrawWireDisc(new Vector3(v.x, 0, v.y), // position
                                      Vector3.up, // normal
                                      r.standardAgentSize); // radius
        }
    }
}