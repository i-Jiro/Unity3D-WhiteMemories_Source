using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class Turn : EnemyAction
{
    public override void OnStart()
    {
        base.OnStart();

        if (_enemyController.FacingDirection == FacingDirection.Forward)
        {
            _enemyController.ChangeDirection(FacingDirection.Back);
        }
        else if (_enemyController.FacingDirection == FacingDirection.Back)
        {
            _enemyController.ChangeDirection(FacingDirection.Forward);
        }
    }

    public override TaskStatus OnUpdate()
    {
        return _enemyController.IsTurning ? TaskStatus.Running : TaskStatus.Success;
    }
}
