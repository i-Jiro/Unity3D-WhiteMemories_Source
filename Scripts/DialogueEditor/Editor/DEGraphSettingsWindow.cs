using UnityEditor;
using UnityEngine;


public class DEGraphSettingsWindow : EditorWindow
{

    [MenuItem("Window/UI Toolkit/DEGraphSettingsWindow")]
    public static void Open()
    {
        DEGraphSettingsWindow wnd = GetWindow<DEGraphSettingsWindow>("Graph Settings");
    }

    private void OnGUI()
    {
        if (DEEditorWindow.GraphSettings != null)
        {
            SerializedObject serializedObject = new(DEEditorWindow.GraphSettings);
            SerializedProperty serializedProperty = serializedObject.GetIterator();
            serializedProperty.NextVisible(true);
            DrawProperties(serializedProperty);
            serializedObject.ApplyModifiedProperties();
            
            EditorGUILayout.HelpBox("Restart Dialogue Editor to see visual changes", MessageType.Info);

            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset to Default"))
            {
                DEEditorWindow.GraphSettings.ResetValues();
            }

            if (GUILayout.Button("Close"))
            {
                Close();
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private void DrawProperties(SerializedProperty p)
    {
        while (p.NextVisible(false))
        {
            EditorGUILayout.PropertyField(p, true);
        }
    }

}