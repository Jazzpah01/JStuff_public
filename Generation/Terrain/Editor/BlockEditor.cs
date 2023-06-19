using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using JStuff.Generation.Terrain;
using System.IO;

[CustomEditor(typeof(Block))]
public class BlockEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (!Application.isPlaying)
        {
            Block to = serializedObject.targetObject as Block;

            if (to.transform.parent.name == "Blocks")
            {
                if (GUILayout.Button("Load Whole Block"))
                {
                    to.EditorGenerateAll();
                }

                if (GUILayout.Button("Save Block"))
                {
                    to.gameObject.name = "SavedBlock" + to.GetCoordinates();

                    to.terrain.SaveBlock(to);
                }
            } else
            {
                GUILayout.Label("Saved Block");
            }
        }

        base.OnInspectorGUI();
    }
}