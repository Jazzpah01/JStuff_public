using JStuff.AI.Steering;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SteeringAgent), true), CanEditMultipleObjects]
public class SteeringAgentEditor : Editor
{
    SerializedProperty navSys;
    SerializedProperty transform;
    SerializedProperty radius;

    private void OnEnable()
    {
        navSys = serializedObject.FindProperty("navSystem");
        transform = serializedObject.FindProperty("transform");
        radius = serializedObject.FindProperty("radius");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        base.OnInspectorGUI();

        //EditorGUILayout.PropertyField(serializedObject.FindProperty("isSetup"));

        if (navSys.objectReferenceValue == null)
        {
            GameObject gameObject = GameObject.Find("NavigationSystem");

            if (gameObject == null)
            {
                Debug.LogError("There is no navigation system in the scene. NavigationSystem GameObject must have name 'NavigationSystem'.");
            } else
            {
                Debug.Log("Setting navSys");
                navSys.objectReferenceValue = gameObject.GetComponent<NavigationSystem>();
            }
        }

        serializedObject.ApplyModifiedProperties();
    }


    //private void OnSceneGUI()
    //{
    //    if (target == null)
    //        return;

    //    SteeringAgent agent = (SteeringAgent)target;

    //    //NavigationSystem s = agent.navSystem;
    //    NavigationSystem s = navSys.objectReferenceValue as NavigationSystem;

    //    if (s == null)
    //        return;

    //    Transform t = agent.transform;
    //    Vector2 navPosition = s.WorldToNavigation(t.position);
    //    float radius = this.radius.floatValue;

    //    Handles.DrawWireDisc(s.NavigationToWorld(navPosition), // position
    //                                  (s.up == UpAxis.Y) ? Vector3.up : Vector3.forward, // normal
    //                                  radius); // radius
    //}
}
