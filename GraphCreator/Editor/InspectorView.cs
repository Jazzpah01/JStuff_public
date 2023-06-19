using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor;

public class InspectorView : VisualElement
{
    public new class UxmlFactory : UxmlFactory<InspectorView, UxmlTraits> { }

    Editor editor;

    public InspectorView()
    {

    }

    internal void UpdateSelection(SimpleNodeView nodeView)
    {
        Clear();

        UnityEngine.Object.DestroyImmediate(editor);

        if (nodeView == null || nodeView.node == null)
        {
            return;
        }

        editor = Editor.CreateEditor(nodeView.node);
        IMGUIContainer container = new IMGUIContainer(() => { 
            if (editor.target)
            {
                editor.OnInspectorGUI();
            }
        });
        Add(container);
    }
}