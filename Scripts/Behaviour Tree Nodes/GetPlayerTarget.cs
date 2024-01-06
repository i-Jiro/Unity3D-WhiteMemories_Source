using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class GetPlayerTarget : EnemyAction
{
    public SharedGameObject Target;

    public override TaskStatus OnUpdate()
    {
        // If the target is already set, return success.
        if (Target.Value != null)
        {
            return TaskStatus.Success;
        }

        if (GameManager.Instance != null)
        {
            Target.Value = GameManager.Instance.Player.gameObject;
            return TaskStatus.Success;
        }

        Debug.LogError("No GameManager found in scene! Unable to get player.");
        return TaskStatus.Failure;
    }
}
