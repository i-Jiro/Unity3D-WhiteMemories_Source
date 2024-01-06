using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class SplineConnector : MonoBehaviour
{
    [FormerlySerializedAs("_spline")]
    [Header("Splines")]
    [Required]
    [SerializeField] private Spline _baseSpline;
    [Required]
    [SerializeField] private Spline _connectedSpline;
    
    [Header("Trigger Collider")]
    [Required]
    [SerializeField] private BoxCollider _collider;
    
    [Header("Properties")]
    [InfoBox("By default, component will attempt to connect to the starting/first point of the connected spline. " +
             "If this is checked, it will connect to the last point of the connected spline.")]
    [SerializeField] private bool _connectToEndPoint = false;
    [SerializeField] private Color _gizmoColor = new Color(0, 255, 0, 0.75f);
    
    private void Awake()
    {
        this.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
    }

    private void OnValidate()
    {
        Connect();
        Vector3 colliderSize = new Vector3(1f, 4f, 0.02f);
        if (!_collider) return;
        _collider.size = colliderSize;
        _collider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<SplineWalker>(out var splineWalker)) return;
        if (!splineWalker.CanChangeSplines) return;
        if(splineWalker.Spline == _baseSpline)
            splineWalker.SetSpline(_connectedSpline);
        else if (splineWalker.Spline == _connectedSpline)
            splineWalker.SetSpline(_baseSpline);
        else
            Debug.LogError("SplineWalker is not on either spline.");
    }

    [Button]
    private void Connect()
    {
        if(!_baseSpline || !_connectedSpline) return;
        if(_connectedSpline.AnchorPoints.Count < 3 || _baseSpline.AnchorPoints.Count < 3) return;
        if(_connectToEndPoint)
            _connectedSpline[_connectedSpline.NumPoints-1] = _baseSpline[_baseSpline.AnchorPoints.Count-1];
        else
            _connectedSpline[1] = _baseSpline[_baseSpline.AnchorPoints.Count-1];
        transform.position = _baseSpline[_baseSpline.AnchorPoints.Count-1];
        var LookDir = _connectedSpline.GetTangent(_baseSpline[0], _baseSpline[1], _baseSpline[2], _baseSpline[3],0f);
        transform.rotation = Quaternion.LookRotation(LookDir, Vector3.up);
    }
    
    //draw box collider
    private void OnDrawGizmos()
    {
        if (!_collider) return;
        Gizmos.color = _gizmoColor;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
        Gizmos.DrawCube(transform.InverseTransformPoint(_collider.bounds.center), new Vector3(_collider.size.x, _collider.size.y, _collider.size.z));
    }
}
