using UnityEditor;
using UnityEngine;

namespace JStuff.AI.Flocking
{
    [CustomPropertyDrawer(typeof(WeightedBehavior))]
    public class WeightedBehaviorDrawer : PropertyDrawer
    {
        // Taken from https://docs.unity3d.com/ScriptReference/PropertyDrawer.html
        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Calculate rects
            var behaviorRect = new Rect(position.x, position.y, 140, position.height);
            var weightRect = new Rect(position.x + 145, position.y, 50, position.height);
            var flockAsContextRect = new Rect(position.x + 145 + 50, position.y, 50, position.height);
            //var contextRect = new Rect(position.x + 200, position.y, position.width - 90, position.height);
            //var contextRect = new Rect(position.x + 145+ 30+5, position.y, 30, position.height);

            // Draw fields - passs GUIContent.none to each so they are drawn without labels
            EditorGUI.PropertyField(behaviorRect, property.FindPropertyRelative("behavior"), GUIContent.none);
            EditorGUI.PropertyField(weightRect, property.FindPropertyRelative("weight"), GUIContent.none);
            EditorGUI.PropertyField(flockAsContextRect, property.FindPropertyRelative("flockAsContext"), GUIContent.none);
            //EditorGUI.PropertyField(contextRect, property.FindPropertyRelative("context"), true);
            //EditorGUILayout.PropertyField()

            //var list = serializedObject.FindProperty("ListTest");
            //EditorGUILayout.PropertyField(list, new GUIContent("My List Test"), true);

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
        //public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        //{
        //    int lineCount = 2;
        //    return EditorGUIUtility.singleLineHeight * lineCount + EditorGUIUtility.standardVerticalSpacing * (lineCount - 1);
        //}
    }
}