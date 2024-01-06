using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using DG.Tweening;
using UnityEngine;

public class MoveToTarget : EnemyAction
{
    public SharedGameObject Target;
    public float MoveForce = 10f;


    public override void OnStart()
    {
       _enemyController.IsMoving = true;
    }

    public override TaskStatus OnUpdate()
    {
        return TaskStatus.Running;
    }

    public override void OnEnd()
    {
        _enemyController.IsMoving = false;
        base.OnEnd();
    }
}

