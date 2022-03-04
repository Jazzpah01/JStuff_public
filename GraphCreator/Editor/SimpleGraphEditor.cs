using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;
using JStuff.GraphCreator;


public class SimpleGraphEditor : EditorWindow
{
    SimpleGraphView graphView;
    InspectorView inspectorView;

    [OnOpenAsset(1)]
    public static bool OnOpenAsset(int instanceID, int line)
    {
        Graph obj = EditorUtility.InstanceIDToObject(instanceID) as Graph;
        //SimpleGraph obj = Selection.activeObject as SimpleGraph;
        if (obj != null)
        {
            obj.UpdateGraph();
            OpenWindow();
            return true;
        }
        return false;
    }

    [MenuItem("Window/SimpleGraphEditor/Editor ...")]
    public static void OpenWindow()
    {
        SimpleGraphEditor wnd = GetWindow<SimpleGraphEditor>();
        wnd.titleContent = new GUIContent("SimpleGraphEditor");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/JStuff_public/GraphCreator/Editor/SimpleGraphEditor.uxml");
        visualTree.CloneTree(root);

        // A stylesheet can be added to a VisualElement.
        // The style will be applied to the VisualElement and all of its children.
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/JStuff_public/GraphCreator/Editor/SimpleGraphEditor.uss");
        root.styleSheets.Add(styleSheet);

        graphView = root.Q<SimpleGraphView>();
        inspectorView = root.Q<InspectorView>();
        graphView.OnNodeSelected = OnNodeSelectionChanged;

        OnSelectionChange();
    }

    private void OnSelectionChange()
    {
        Graph g = Selection.activeObject as Graph;
        if (g != null)
        {
            graphView.PopulateView(g);
        }
    }

    void OnNodeSelectionChanged(SimpleNodeView nodeView)
    {
        if (nodeView == null)
            return;

        inspectorView.UpdateSelection(nodeView);
    }
}