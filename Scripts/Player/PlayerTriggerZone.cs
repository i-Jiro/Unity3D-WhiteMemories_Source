using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
public class PlayerTriggerZone : MonoBehaviour
{
    public UnityEvent OnPlayerEnter;
    private bool _triggered = false;
    private BoxCollider _collider;

    private void OnValidate()
    {
        _collider = GetComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && !_triggered)
        {
            OnPlayerEnter?.Invoke();
            _triggered = true;
        }
    }
    
    //Draw collider in gizmo
    private void OnDrawGizmos()
    {
        if (_collider == null)
        {
            _collider = GetComponent<BoxCollider>();
        }
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + _collider.center, _collider.size);
    }
}
