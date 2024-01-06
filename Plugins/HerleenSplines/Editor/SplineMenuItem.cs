using UnityEngine;
using UnityEditor;
public class SplineMenuItem : MonoBehaviour
{
    [MenuItem("GameObject/Spline/New Spline")]
    public static void CreateSpline(MenuCommand menuCommand)
    {
        var splinePrefab = Resources.Load("Spline") as GameObject;
        var spline = Instantiate(splinePrefab, Vector3.zero, Quaternion.identity);
        spline.name = splinePrefab.name;
    }
}
