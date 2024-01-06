using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Spline : MonoBehaviour
{
    [SerializeField] private Color pointColour = Color.white;
    [Range(0.0f, 1.0f)]
    [SerializeField] private float pointSize = 0.1f;
    [SerializeField] private Color lineColour = Color.white;
    [Range(1f, 10.0f)]
    [SerializeField] private float lineSize = 1.5f;
    [SerializeField] private bool useWorldSpace = false;
    [SerializeField] private bool closed = false;
    [Space(10f)]
    [SerializeField] private bool alwaysVisible = false;
    [SerializeField] private List<Vector3> anchorPoints = new List<Vector3> { new Vector3(0, 0, 0), new Vector3(0, 0, 0) };
    public List<Vector3> AnchorPoints { get { return anchorPoints; } set { } }
    public Color PointColour { get { return pointColour; } }
    public Color LineColour { get { return lineColour; } }
    public float PointSize { get { return pointSize; } }
    public float LineSize { get { return lineSize; } }
    public int NumPoints { get { return anchorPoints.Count; } }
    public bool Closed { get { return closed; } }
    public bool UseWorldSpace { get { return useWorldSpace; } }
    public bool AlwaysVisible { get { return alwaysVisible; } }
    public int NumSegments { get { return anchorPoints.Count; } }
    public Vector3 this[int i] { get { return anchorPoints[i]; } set { anchorPoints[i] = value; } }

    public void AddPoint(Vector3 position)
    {
        anchorPoints.Add(position);
    }

    public void ClearPoints()
    {
        anchorPoints = new List<Vector3> { new Vector3(0, 0, 0), new Vector3(0, 0, 0) };
        closed = false;
    }
    public Vector3 GetPoint(int index)
    {
        return useWorldSpace ? this[index] : this.transform.TransformPoint(anchorPoints[index]);
    }

    public void SetPoint(int index, Vector3 position)
    {
        anchorPoints[index] = useWorldSpace ? position : this.transform.InverseTransformPoint(position);
    }

    public void InsertPoint(int segmentIndex, Vector3 position)
    {
        anchorPoints.Insert(segmentIndex, position);
    }
    public float GetNormalizedT(Vector3 point)
    {
        float minDistance = Mathf.Infinity;
        int closestIndex = 0;
        for (int i = 0; i < anchorPoints.Count; i++)
        {
            float distance = Vector3.Distance(this[i], point);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestIndex = i;
            }
        }

        float t = (float)closestIndex / (anchorPoints.Count - 1);

        return t;
    }

    //Gets the nearest point on the whole spline relative to a given point.
    public Vector3 GetNearestPoint(Vector3 point, out Vector3 tangent, int iterations = 10, bool ignoreY = false)
    {
        int segmentCount = anchorPoints.Count - 3;
        float minDistance = Mathf.Infinity;
        Vector3 nearestPoint = Vector3.zero;
        tangent = Vector3.zero;

        for (int i = 0; i < segmentCount; i++)
        {
            Vector3 p0 = anchorPoints[i];
            Vector3 p1 = anchorPoints[i + 1];
            Vector3 p2 = anchorPoints[i + 2];
            Vector3 p3 = anchorPoints[i + 3];

            float t = FindNearestPointOnSegment(point, p0, p1, p2, p3, iterations, ignoreY);
            Vector3 interpolatedPoint = CalculatePoint(p0, p1, p2, p3, t);
            if (ignoreY)
            {
                interpolatedPoint.y = 0f;
                point.y = 0f;
            }
            float distance = Vector3.Distance(point, interpolatedPoint);

            if (distance < minDistance)
            {
                minDistance = distance;
                nearestPoint = interpolatedPoint;
                tangent = GetTangent(p0, p1, p2, p3, t);
            }
        }

        return nearestPoint;
    }
    
    //Gets the nearest point in t, on a segment of control points to a given point.
    private float FindNearestPointOnSegment(Vector3 point, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int iterations = 10, bool ignoreY = false)
    {
        float t = 0f;
        float minDistance = Mathf.Infinity;
        
        float step = 1f / iterations;

        for (int i = 0; i <= iterations; i++)
        {
            float currentT = i * step;
            Vector3 interpolatedPoint = CalculatePoint(p0, p1, p2, p3, currentT);
            if (ignoreY)
            {
                interpolatedPoint.y = 0f;
                point.y = 0f;
            }
            float distance = Vector3.Distance(point, interpolatedPoint);

            if (distance < minDistance)
            {
                minDistance = distance;
                t = currentT;
            }
        }

        return t;
    }

    //Calculates a point on the spline at a given t value between 4 control points
    public Vector3 CalculatePoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float tt = t * t;
        float ttt = tt * t;

        float q1 = -ttt + 2.0f * tt - t;
        float q2 = 3.0f * ttt - 5.0f * tt + 2.0f;
        float q3 = -3.0f * ttt + 4.0f * tt + t;
        float q4 = ttt - tt;

        float x = 0.5f * (p0.x * q1 + p1.x * q2 + p2.x * q3 + p3.x * q4);
        float y = 0.5f * (p0.y * q1 + p1.y * q2 + p2.y * q3 + p3.y * q4);
        float z = 0.5f * (p0.z * q1 + p1.z * q2 + p2.z * q3 + p3.z * q4);

        return new Vector3(x, y, z);
    }
    //Calculates the tangent on the spline at a given t value between 4 control points
    public Vector3 GetTangent(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float tt = t * t;

        float q1 = -3.0f * tt + 4.0f*t - 1;
        float q2 = 9.0f*tt - 10.0f*t;
        float q3 = -9.0f*tt + 8.0f*t + 1.0f;
        float q4 = 3.0f*tt - 2.0f*t;

        float x = 0.5f * (p0.x * q1 + p1.x * q2 + p2.x * q3 + p3.x * q4);
        float y = 0.5f * (p0.y * q1 + p1.y * q2 + p2.y * q3 + p3.y * q4);
        float z = 0.5f * (p0.z * q1 + p1.z * q2 + p2.z * q3 + p3.z * q4);

        return new Vector3(x, y, z).normalized;
    }

    //Calculates a point on the spline at a given un-normalized t (1 to anchorPoints.count-3) of the whole spline.
    public Vector3 CalculatePoint(float t)
    {
        int startIndex = Mathf.FloorToInt(t) % anchorPoints.Count;
        Vector3 p0 = this[(startIndex - 1 + anchorPoints.Count) % anchorPoints.Count];
        Vector3 p1 = this[startIndex];
        Vector3 p2 = this[(startIndex + 1) % anchorPoints.Count];
        Vector3 p3 = this[(startIndex + 2) % anchorPoints.Count];

        float u = t - Mathf.Floor(t);

        Vector3 point = 0.5f * (
            (-p0 + 3f * p1 - 3f * p2 + p3) * (u * u * u)
            + (2f * p0 - 5f * p1 + 4f * p2 - p3) * (u * u)
            + (-p0 + p2) * u
            + 2f * p1
        );

        return point;
    }
    
    public void DrawSpline()
    {
        if (anchorPoints.Count < 3)
        {
            return;
        }

        float tOffset = 0.05f;
        for (int i = 0; i < anchorPoints.Count - 3; i++)
        {
            Vector3 p0 = this[i];
            Vector3 p1 = this[i + 1];
            Vector3 p2 = this[i + 2];
            Vector3 p3 = this[i + 3];

#if UNITY_EDITOR
            Handles.color = lineColour;
            for (float t = 0; t <= 1; t += 0.02f)
            {
                Vector3 point = CalculatePoint(p0, p1, p2, p3, t);
                Handles.DrawLine(useWorldSpace ? point :
                    transform.TransformPoint(point),
                    useWorldSpace ? CalculatePoint(p0, p1, p2, p3, t + tOffset) :
                    transform.TransformPoint(CalculatePoint(p0, p1, p2, p3, t + tOffset)), lineSize);
            }
#endif
        }

        this[0] = this[1];
        this[anchorPoints.Count - 2] = this[anchorPoints.Count - 1];

        if (closed)
        {
            this[1] = this[anchorPoints.Count - 1];
        }
    }

    private void OnDrawGizmos()
    {
        if (!alwaysVisible) 
        {
            return;
        }

        DrawSpline();
    }
}
