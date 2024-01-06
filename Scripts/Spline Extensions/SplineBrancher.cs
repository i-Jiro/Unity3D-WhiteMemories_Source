using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class SplineBrancher : MonoBehaviour
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
    [Range(0f,1f)]
    public float _branchPoint = 1f;
    [SerializeField] private Color _gizmoColor = new Color(0, 0, 255, 0.75f);
    
    [Header("Events")]
    [SerializeField] private UnityEvent _onPlayerEnter;
    [SerializeField] private UnityEvent _onPlayerExit;
    
    private PlayerSplineWalker _playerSplineWalker;
    private bool _playerInside = false;

    private void Awake()
    {
        this.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
    }

    private void OnValidate()
    {
        if(!_connectedSpline || !_baseSpline) return;
        if(_connectedSpline.AnchorPoints.Count < 3 || _baseSpline.AnchorPoints.Count < 3) return;
        var t = _branchPoint * ((float)_baseSpline.AnchorPoints.Count - 2);
        var point = _baseSpline.CalculatePoint(t);
        transform.position = point;
        if(_connectToEndPoint)
            _connectedSpline[_connectedSpline.NumPoints-1] = point;
        else
            _connectedSpline[1] = point;
        _collider.isTrigger = true;
    }

    private void OnEnable()
    {
        if(PlayerInputManager.Instance)
            PlayerInputManager.Instance.InteractPressed += OnInteractPressed;
    }

    private void OnDisable()
    {
        if(PlayerInputManager.Instance)
            PlayerInputManager.Instance.InteractPressed -= OnInteractPressed;
    }

    private void OnInteractPressed()
    {
        if(_playerSplineWalker == null) return;
        if(_playerSplineWalker.Spline == _baseSpline)
            _playerSplineWalker.SetSpline(_connectedSpline);
        else if (_playerSplineWalker.Spline == _connectedSpline)
            _playerSplineWalker.SetSpline(_baseSpline);
        else
            Debug.LogError("SplineWalker is not on either spline.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInside = true;
            _playerSplineWalker = other.GetComponent<PlayerSplineWalker>();
            _onPlayerEnter?.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInside = false;
            _playerSplineWalker = null;
            _onPlayerExit?.Invoke();
        }
    }

    private void OnDrawGizmos()
    {
        if (!_collider) return;
        Gizmos.color = _gizmoColor;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
        Gizmos.DrawCube(transform.InverseTransformPoint(_collider.bounds.center), new Vector3(_collider.size.x, _collider.size.y, _collider.size.z));
    }
}
