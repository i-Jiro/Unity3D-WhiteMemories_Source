using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class FaceTarget : EnemyAction
{
    public SharedGameObject Target;
    public SharedGameObject Self;
    
    public override void OnStart()
    {
        base.OnStart();
        var fromTo = Target.Value.transform.position - _enemyController.transform.position;
        var forward = _enemyController.MeshContainer.transform.forward;
        var dot = Vector3.Dot(fromTo.normalized, forward);
        if (dot < 0)
        {
            switch (_enemyController.FacingDirection)
            {
                case FacingDirection.Forward:
                    _enemyController.ChangeDirection(FacingDirection.Back);
                    break;
                case FacingDirection.Back:
                    _enemyController.ChangeDirection(FacingDirection.Forward);
                    break;
            }
        }
    }

    public override TaskStatus OnUpdate()
    {
        return _enemyController.IsTurning ? TaskStatus.Running : TaskStatus.Success;
    }
}
