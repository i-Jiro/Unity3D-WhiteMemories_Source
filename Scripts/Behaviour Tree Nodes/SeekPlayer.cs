using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// Raycasts forward to check if the player is in front of the enemy.
/// </summary>
public class SeekPlayer : EnemyAction
{
    public SharedGameObject Target;
    public SharedBool FoundTarget;
    public float YposRayOffset = 0.5f;
    public LayerMask PlayerLayer = 7;
    public float RayDistance = 2f;
    public bool DebugRay = false;

    public override TaskStatus OnUpdate()
    {
        var ray = _enemyController.FacingDirection == FacingDirection.Forward ? new Ray(transform.position + (Vector3.up * YposRayOffset), transform.forward)
            : new Ray(transform.position + (Vector3.up * YposRayOffset), -transform.forward);
        if (DebugRay)
        {
            Debug.DrawRay(ray.origin, ray.direction * RayDistance, Color.blue, 0.1f);
        }
        if(Physics.Raycast(ray, out var hit, RayDistance, PlayerLayer))
        {
            if (hit.collider.CompareTag("Player"))
            {
                Target.Value = hit.collider.gameObject;
                FoundTarget.Value = true;
                return TaskStatus.Success;
            }
        }
        return TaskStatus.Running;
    }
}
