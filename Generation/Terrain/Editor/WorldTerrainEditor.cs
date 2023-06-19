using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using JStuff.Generation.Terrain;

[CustomEditor(typeof(WorldTerrain))]
public class WorldTerrainEditor : Editor
{
    //https://answers.unity.com/questions/34610/get-the-position-of-the-editor-camera.html
    public override void OnInspectorGUI()
    {
        WorldTerrain to = ((WorldTerrain)serializedObject.targetObject);

        if (!Application.isPlaying)
        {
            if (to.blocks != null && GUILayout.Button("EditorCleanup"))
            {
                to.Cleanup();
            }
            if (GUILayout.Button("Generate Terrain"))
            {
                to.GenerateEditorTerrain(SceneView.lastActiveSceneView.camera.transform.position);
            }
        }

        base.OnInspectorGUI();
    }
}
