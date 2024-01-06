using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Spline))]
public class SplineEditor : Editor
{
    private Spline spline;
    private Plane plane = new (Vector3.down, 0);
    private int? chosenHandle = null;
    private Vector3 worldMouse;
    private void OnEnable()
    {
        spline = (Spline)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Clear Points"))
        {
            Undo.RecordObject(spline, "Clear point");
            spline.ClearPoints();
        }
    }

    private void OnSceneGUI()
    {
        InputEvents();
        DrawSpline();
        DrawAnchorPoints();
        SceneView.RepaintAll();
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
    }

    private void InputEvents()
    {
        Event ev = Event.current;
        var mouseRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        var raySnap = HandleUtility.RaySnap(mouseRay);

        if (raySnap == null)
        {
            if (plane.Raycast(mouseRay, out float distance))
            {
                worldMouse = mouseRay.GetPoint(distance) - spline.transform.position;
            }
        }
        else
        {
            RaycastHit hit;
            if (Physics.Raycast(mouseRay, out hit))
            {
                worldMouse = hit.point - spline.transform.position;
            }
        }

        if (ev.shift && spline.NumPoints >= 3)
        {
            Handles.color = spline.PointColour;
            Handles.SphereHandleCap(0, worldMouse, Quaternion.identity, spline.PointSize, EventType.Repaint);
            Handles.color = spline.LineColour;
            Handles.DrawDottedLine(spline[spline.NumPoints - 1], worldMouse, spline.LineSize);
        }

        if (ev.type == EventType.MouseDown && ev.button == 0 && ev.shift)
        {
            Undo.RecordObject(spline, "Add point");
            spline.AddPoint(worldMouse);
        }

        if (ev.control)
        {
            Handles.color = spline.PointColour;
            Handles.SphereHandleCap(0, worldMouse, Quaternion.identity, spline.PointSize, EventType.Repaint);
        }

        if (ev.type == EventType.MouseDown && ev.button == 0 && ev.control)
        {
            float t = spline.GetNormalizedT(worldMouse);
            int segmentIndex = Mathf.FloorToInt(t * spline.NumSegments);

            Undo.RecordObject(spline, "Insert point");
            spline.InsertPoint(segmentIndex, worldMouse);
        }

        if (chosenHandle.HasValue && ev.keyCode == KeyCode.Backspace && chosenHandle.Value < spline.NumPoints - 2)
        {
            Undo.RecordObject(spline, "Delete point");
            spline.AnchorPoints.RemoveAt(chosenHandle.Value);
            chosenHandle = null;
        }

    }

    private void DrawAnchorPoints()
    {
        if (spline.NumPoints < 3)
        {
            return;
        }

        for (int i = 0; i < spline.NumPoints; i++)
        {
            Vector3 position = spline.GetPoint(i);
            Handles.color = spline.PointColour;
            if (Handles.Button(position, Quaternion.identity, spline.PointSize, 0.5f, Handles.SphereHandleCap))
            {
                chosenHandle = i;
            }
        }

        if (chosenHandle.HasValue)
        {
            EditorGUI.BeginChangeCheck();
            Vector3 position = spline.GetPoint(chosenHandle.Value);
            Vector3 newPosition = Handles.PositionHandle(position, Quaternion.identity);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(spline, "Move point");
                spline.SetPoint(chosenHandle.Value, newPosition);
            }
        }
    }

    private void DrawSpline()
    {
        if (spline.AlwaysVisible)
        {
            return;
        }

        spline.DrawSpline();
    }
}
