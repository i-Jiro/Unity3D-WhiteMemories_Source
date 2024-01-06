using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Runtime Sets/Spline Runtime Set")]
public class SplineRuntimeSet : RuntimeSets<Spline>
{
    public Spline GetSplineByName(string name)
    {
        foreach (var spline in Items)
        {
            if (spline.name == name)
            {
                return spline;
            }
        }

        return null;
    }
}
