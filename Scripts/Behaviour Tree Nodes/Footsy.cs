using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class Footsy : EnemyAction
{
    public SharedGameObject Target;
    public SharedGameObject Self;
    public float _farDistance = 2f;
    public float _closeDistance = 1f;
    
    public float Duration = 5f;
    public bool RandomizeDuration = false;
    public float MinDuration = 1f;
    public float MaxDuration = 5f;

    protected float _randDuration;
    protected float _timer = 0f;
    
    public override void OnStart()
    {
        _timer = 0f;
        if(RandomizeDuration)
            _randDuration = Random.Range(MinDuration, MaxDuration);
        base.OnStart();
    }
    
    public override TaskStatus OnUpdate()
    {
        var fromTo = Target.Value.transform.position - transform.position;
        var distance = fromTo.magnitude;
        var dot = Vector3.Dot(fromTo.normalized, _enemyController.MeshContainer.transform.forward);
        if (dot < 0 && !_enemyController.IsTurning)
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
            _enemyController.IsMoving = false;
            _enemyController.IsMovingBackwards = false;
        }

        //Wait for the turn to finish before moving again.
        if (_enemyController.IsTurning){ return TaskStatus.Running;}
        
        //If target is far away, move towards it
        if(distance > _farDistance)
        {
            _enemyController.IsMoving = true;
        }
        else if (distance < _farDistance)
        {
            _enemyController.IsMoving = false;
        }
        
        //If is too close, move backward.
        if(distance < _closeDistance)
        {
            _enemyController.IsMoving = true;
            _enemyController.IsMovingBackwards = true;
        }
        
        //If is in the sweet spot, stop moving.
        if (distance > _closeDistance && distance < _farDistance)
        {
            _enemyController.IsMoving = false;
            _enemyController.IsMovingBackwards = false;
        }

        _timer += Time.deltaTime;
        if(RandomizeDuration)
            return _timer > _randDuration ? TaskStatus.Success : TaskStatus.Running;
        return _timer > Duration ? TaskStatus.Success : TaskStatus.Running;
    }

    public override void OnEnd()
    {
        _enemyController.IsMoving = false;
        _enemyController.IsMovingBackwards = false;
        base.OnEnd();
    }
}
