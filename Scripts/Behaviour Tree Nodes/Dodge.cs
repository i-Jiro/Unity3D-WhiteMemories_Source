using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// A parallel task that will constantly run until it recieves a PlayerAttackStarted event to dodge.
/// Once it recieves the event, it will check if the player is within a certain distance and if so, will attempt to dodge.
/// </summary>
public class Dodge : EnemyAction
{
    public float DodgeChance = 0.5f;
    public PlayerEventChannel PlayerEventChannel;
    public SharedGameObject Target;
    public float _minDistanceToDodge = 2f;
    private bool _isDodging = false;
    private bool _recievedPlayerAttackEvent;
    
    public override void OnStart()
    {
        PlayerEventChannel.PlayerAttackStarted += OnPlayerAttackStarted;
        _isDodging = false;
        base.OnStart();
        
    }
    
    public override TaskStatus OnUpdate()
    {
        if (_isDodging)
        {
            _enemyController.Dodge();
            _isDodging = false;
            return TaskStatus.Success;
        }
        return TaskStatus.Running;
    }
    
    //Event callback from the PlayerEventChannel.
    private void OnPlayerAttackStarted(PlayerAttackController.AttackEventContext context)
    {
        var distance = Vector3.Distance(transform.position, Target.Value.transform.position);
        //If the player is too far away, dont dodge.
        if (distance > _minDistanceToDodge)
        {
            return;
        }
        var randomValue = Random.Range(0f, DodgeChance);
        if (randomValue <= DodgeChance)
        {
            _isDodging = true;
        }
    }

    public override void OnEnd()
    {
        PlayerEventChannel.PlayerAttackStarted -= OnPlayerAttackStarted;
        base.OnEnd();
    }
}
