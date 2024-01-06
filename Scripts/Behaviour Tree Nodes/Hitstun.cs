using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using TMPro;
using UnityEngine;

public class Hitstun : EnemyAction
{
    public SharedFloat HitstopDuration;
    public float Duration = 0.25f;
    private float _timer;
    public override void OnStart()
    {
        /*
        _enemyController.PlayHitAnimation();
        if(_enemyController.RecievedAttack != null)
            HitStopManager.Instance?.HitStop(_enemyController.RecievedAttack.HitStopDuration);
        */
        _enemyController.StopAttack();
        _timer = 0;
    }
    public override TaskStatus OnUpdate()
    
    {
        _timer += Time.deltaTime;
        return _timer >= Duration ? TaskStatus.Success : TaskStatus.Running;
    }
}
