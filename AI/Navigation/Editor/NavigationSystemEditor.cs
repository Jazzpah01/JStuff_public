using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using JStuff.Utilities;

[CustomEditor(typeof(NavigationSystem))]
public class NavigationSystemEditor : Editor
{
    private void OnSceneGUI()
    {
        NavigationSystem navSys = serializedObject.targetObject as NavigationSystem;

        if (navSys.pathVisualization == null)
            return;

        //Debug.Log("draw path: " + navSys.pathVisualization.Count);

        for (int i = 0; i < navSys.pathVisualization.Count - 1; i++)
        {
            Vector3 s = new Vector3(navSys.pathVisualization[i].x, 0, navSys.pathVisualization[i].y);
            Vector3 t = new Vector3(navSys.pathVisualization[i + 1].x, 0, navSys.pathVisualization[i + 1].y);

            Vector3 dir = t - s;
            Vector3 pos = s;
            Quaternion orientation = Quaternion.LookRotation(dir, Vector3.up);
            float size = dir.magnitude;
            //EventType eventType = new 

            Handles.ArrowHandleCap(0, s, orientation, size, EventType.Repaint);

            if (i == navSys.pathVisualization.Count - 2)
            {
                Handles.DrawWireDisc(t, // position
                                      Vector3.up, // normal
                                      1); // radius
            }
        }
    }
}
