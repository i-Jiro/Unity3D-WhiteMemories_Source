using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

[CustomEditor(typeof(HitboxController))]
public class HitboxControllerEditor : Editor
{
    HitboxController hitboxController;
    private bool resizeMode = false;

    private void OnEnable()
    {
        hitboxController = (HitboxController)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Change Hitbox Size"))
        {
            resizeMode = !resizeMode;
            Tools.current = Tool.None;
        }
    }

    private void OnSceneGUI()
    {
        ShowScaleHandle();
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        SceneView.RepaintAll();
    }

    public void ShowScaleHandle()
    {
        if (!resizeMode)
        {
            return;
        }

        Vector3 position = hitboxController.transform.position - (hitboxController.hitBoxSize * 0.5f);
        EditorGUI.BeginChangeCheck();
        Vector3 newScale = Handles.ScaleHandle(hitboxController.hitBoxSize, Tools.handlePosition, Quaternion.Euler(0f, 90f, 0f), HandleUtility.GetHandleSize(position));

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(hitboxController, "Scale Hitbox");
            hitboxController.hitBoxSize = newScale;
        }
    }
}
