using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// Helper script to adjust the forward rotation and position of a rigidbody to allow movement on a spline.
/// This does not handle movement, only position and rotation. Actual forward movement should be handled by a different script.
/// Assumption: The spline is on the xz plane and the character is applying forces along it's forward (x-axis) vector. Y axis is unaffected.
/// </summary>

public class SplineWalker : MonoBehaviour
{
    [Header("Properties")]
    [Tooltip("Toggles spline based movement.")] //You can disable spline based movement by setting this or the component to false.
    public bool IsEnabled = true;
    [Tooltip("The number of steps to take when calculating the nearest point on the spline. Higher the number, the more accurate the position will be at the cost of performance.")]
    public int SplineIterations = 150;
    [Tooltip("Can change splines while on a spline.")]
    public bool CanChangeSplines = true;
    [Header("Spline Debug")]
    [SerializeField] private Spline _spline;
    [ReadOnly]
    [SerializeField] private bool _isOnSpline;
    [InfoBox("These settings below should be off by default in most cases.")]
    [LabelText("X-Axis Look Rotation Influence")]
    [Tooltip("If true, the X-axis rotation of the spline will be applied to the character. If false, the character will maintain it's current x-axis rotation.")]
    public bool xRotInfluence = false;
    [LabelText("Y-Plane Influence")]
    [Tooltip("If true, the Y plane will be accounted for determining the nearest point on the spline. If false, calculations only consider xz plane.")]
    public bool yPlaneInfluence = false;

    
    public Spline Spline => _spline;
    
    public virtual void SetSpline(Spline spline)
    {
        if (spline != null)
        {
            _spline = spline;
            _isOnSpline = true;
        }
        else
        {
            _spline = null;
            _isOnSpline = false;
        }
    }
    
    //Should be called before the character starts moving.
    protected virtual void ApplySplinePositionAndRotation()
    { 
        if(IsEnabled == false || !_isOnSpline) return;
        var pointToBe = _spline.GetNearestPoint(transform.position, out var direction, SplineIterations, !yPlaneInfluence);
        //Remove y component to allow free vertical movement
        pointToBe.y = transform.position.y;
        transform.position = pointToBe;
        
        //Set forward rotation to the tangent of the spline at the nearest point.
        Vector3 forward = direction;
        if(!xRotInfluence)
            forward.y = 0f;
        var lookRot = Quaternion.LookRotation(forward, Vector3.up);
        transform.rotation = lookRot;
        
        //Sync physics transforms to prevent jittering
        Physics.SyncTransforms();
    }
    
    protected virtual void OnValidate()
    {
        _isOnSpline = _spline;
    }

    protected virtual void FixedUpdate()
    {
        ApplySplinePositionAndRotation();
    }
}
