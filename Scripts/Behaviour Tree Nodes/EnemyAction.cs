using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public abstract class EnemyAction : Action
{
    protected BaseEnemyController _enemyController;
    public override void OnAwake()
    {
        _enemyController = GetComponent<BaseEnemyController>();
        base.OnAwake();
    }

    public override void OnStart()
    {
        if (_enemyController == null)
        {
            Debug.LogWarning($"{transform.gameObject.name}: Unable to find an enemy controller.");;
        }
        base.OnStart();
    }
}
