using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;

/// <summary>
/// Enemy roams around the map around its starting position given a max distance.
/// </summary>
public class Roam : EnemyAction
{
    
    public float MaxDistance = 3f;
    public float MaxTime = 1f;
    public float RayDistance = 2f;
    public float YposRayOffset = 0.5f;
    public bool OverrideDefaultSpeed;
    public float RoamSpeed = 1f;
    public LayerMask RayLayerMask;
    public bool ShowRay = false;
    
    private Vector3 _spawnPosition;
    private Vector3 _relativeStartPosition;
    private bool _reachedMaxDistance = false;
    private float _timer;
    private Ray _ray;
    private float _origSpeed;
    
    
    /// <summary>
    /// Store the initial position of the enemy based on placement on scene.
    /// </summary>
    public override void OnAwake()
    {
        base.OnAwake();
    }

    /// <summary>
    /// Change direction randomly on start of the action.
    /// </summary>
    public override void OnStart()
    {
        if (OverrideDefaultSpeed)
        {
            _origSpeed = _enemyController.speed;
            _enemyController.speed = RoamSpeed;
        }
        _timer = 0f;
        _spawnPosition = transform.position;
        var currentDir = _enemyController.FacingDirection;
        var currentDistance = Vector3.Distance(_spawnPosition, transform.position);
        
        //If the enemy is already at the max distance, change direction.
        if (currentDistance >= MaxDistance)
        {
            _enemyController.ChangeDirection(currentDir == FacingDirection.Forward ? FacingDirection.Back : FacingDirection.Forward);
        }
        else
        {
            //Randomly decide change direction.
            var randDir = Random.Range(0, 2);
            switch (randDir)
            {
                case 0:
                    _enemyController.ChangeDirection(currentDir == FacingDirection.Forward ? FacingDirection.Back : FacingDirection.Forward);
                    break;
                case 1:
                    //Dont change direction.
                    break;
                default:
                    Debug.LogWarning("Roam: Random direction is out of range.");
                    break;
            }
            
        }
        //Toggle enemy to move forward.
        _enemyController.IsMoving = true;
    }

    /// <summary>
    /// Move the enemy forward until it reaches the max distance or walk time.
    /// </summary>
    /// <returns></returns>
    public override TaskStatus OnUpdate()
    {
        //Wait for the turn to finish before moving again.
        if(_enemyController.IsTurning) return TaskStatus.Running;
        
        _ray = _enemyController.FacingDirection == FacingDirection.Forward ? new Ray(transform.position + (Vector3.up * YposRayOffset), transform.forward)
            : new Ray(transform.position + (Vector3.up * YposRayOffset), -transform.forward);
        
        var currentDistance = Vector3.Distance(_spawnPosition, transform.position);
        _timer += Time.deltaTime;
        
        //Wall detection; Will change direction if the enemy is about to hit a wall.
        if (ShowRay)
        {
            Debug.DrawRay(_ray.origin, _ray.direction * RayDistance, Color.red, 0.1f);
        }
        
        if(Physics.Raycast(_ray,RayDistance))
        {
            _enemyController.ChangeDirection(_enemyController.FacingDirection == FacingDirection.Forward ? FacingDirection.Back : FacingDirection.Forward);
        }
        
        //If the enemy is at the max distance or the max time has passed, stop moving.
        if (currentDistance >= MaxDistance || _timer >= MaxTime)
        {
            return TaskStatus.Success;
        }
        return TaskStatus.Running;
    }
    
    public override void OnEnd()
    {
        if(OverrideDefaultSpeed)
            _enemyController.speed = _origSpeed;
        _enemyController.IsMoving = false;
        base.OnEnd();
    }
}
