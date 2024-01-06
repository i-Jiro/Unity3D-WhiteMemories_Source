using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SplineInteractZone : MonoBehaviour
{
    [SerializeField] private Spline _spline;
    [Range(0f, 1f)]
    [SerializeField] private float _pointOnSpline;
    [SerializeField] private BoxCollider _collider;
    [SerializeField] private Color _gizmoColor = Color.blue;
    private bool _playerInside;
    [Header("Events")]
    public UnityEvent OnInteract;
    [SerializeField] private UnityEvent _onPlayerEnter;
    [SerializeField] private UnityEvent _onPlayerExit;
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
    
    private void OnValidate()
    {
        if(_spline == null) return;
        if(_spline.AnchorPoints.Count < 3) return;
        var t = _pointOnSpline * ((float)_spline.AnchorPoints.Count - 2);
        var point = _spline.CalculatePoint(t);
        transform.position = point;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _playerInside = true;
            _onPlayerEnter?.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _playerInside = false;
            _onPlayerExit?.Invoke();
        }
    }
    
    private void OnInteractPressed()
    {
        if(_playerInside)
            OnInteract?.Invoke();
    }
    
    private void OnDrawGizmos()
    {
        if (!_collider) return;
        Gizmos.color = _gizmoColor;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
        Gizmos.DrawCube(transform.InverseTransformPoint(_collider.bounds.center), new Vector3(_collider.size.x, _collider.size.y, _collider.size.z));
    }
}
