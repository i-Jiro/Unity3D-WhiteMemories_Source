using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class HurtboxController : MonoBehaviour
{

    public BoxCollider collider;
    public Color hurtBoxColor;
    public HurtboxMode Mode = HurtboxMode.NORMAL;
    private ColliderState _state = ColliderState.Open;
    [Header("Debug")]
    [ShowInInspector][ReadOnly]
    private GameObject _owner;
    public GameObject Owner => _owner;
    

    private void Awake()
    {
        //Hard-coded hurtbox layer (Layer 6 in project) on awake. This is to prevent the hurtbox from colliding with other colliders.
        gameObject.layer = 6;
    }
    
    public void SetOwner(GameObject owner)
    {
        _owner = owner;
        
    }

    private void OnDrawGizmos()
    {
        if(collider == null)
        {
            return;
        }
        Gizmos.color = hurtBoxColor;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(collider.center, collider.size);
    }
}

public enum HurtboxMode
{
    NORMAL, PARRY
}
