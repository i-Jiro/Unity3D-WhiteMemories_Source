using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using DG.Tweening;
using Sirenix.OdinInspector;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;

/// <summary>
/// Controller script handling physics movement of the player.
/// </summary>
[DefaultExecutionOrder(100)]
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovementController : MonoBehaviour
{
    [Header("Movement Properties")]
    [SerializeField] private float _maxSpeed = 6f;
    [SerializeField] private float _acceleration = 1f;
    [FormerlySerializedAs("_deceleration")] [SerializeField] private float _decceleration = 1f;
    [SerializeField] private float _velocityPower = 1f;
    [SerializeField] private float _frictionCoefficient = 0.6f;

    [Header("Dodge Properties")]
    [SerializeField] private float _dodgeForce = 1f;
    [Tooltip("Time in seconds to complete dodge.")]
    [SerializeField] private float _dodgeTime = 0.15f;
    [SerializeField] private Vector3 _dodgeBoxOffset = Vector3.zero;
    
    [Header("Jump Properties")]
    [SerializeField] private float _jumpHeight = 1f;
    [Range(0,1)]
    [SerializeField] private float _jumpCutMultiplier = 0.5f;
    [SerializeField] private float _airDirectionModifier = 0.1f;
    [SerializeField] private float _jumpCoyoteTime = 0.15f;
    [SerializeField] private float _jumpInputBufferTime = 0.15f;
    
    [Header("Rigidbody Ride Properties")]
    [SerializeField] private float _rideHeight = 1.75f; // rideHeight: desired distance to ground (Note, this is distance from the raycast position). 
    [SerializeField] private float _rayToGroundLength = 3f; // rayToGroundLength: max distance of raycast to ground (Note, this should be greater than the rideHeight).
    [SerializeField] public float _rideSpringStrength = 50f; // rideSpringStrength: strength of spring.
    [SerializeField] private float _rideSpringDamper = 5f; // rideSpringDampener: dampener of spring.

    [Header("Spherecast Settings")]
    [SerializeField] private Vector3 _rayLocalOffset = Vector3.zero;
    [SerializeField] private float _sphereRadius = 0.25f;
    [SerializeField] private float _sphereRayLength = 1f;
    [SerializeField] private LayerMask _groundLayerMask;
    
    [Header("Gravity Settings")]
    [SerializeField] private float _gravityScale = 1f;
    [SerializeField] private float _baseGravity = -9.81f;
    [SerializeField] private float _fallGravityMultiplier = 1.9f;
    
    [Header("Event Channel")]
    [SerializeField] private PlayerEventChannel _playerEventChannel;
    [SerializeField] private PlayerData _playerData;

    public UnityEvent OnDodgeStart;
    public UnityEvent OnDodgeEnd;

    [Header("Debug")]
    public bool DebugOn = false;
    private bool _isDodging = false;
    private bool _canMove = true;
    private Rigidbody _rigidBody;
    private Vector2 _inputDirection;
    private bool _isMoving = false;
    private bool _isJumping = false;
    private float _lastGroundedTime = 0.0f;
    private float _lastJumpTime = -0.01f;
    private bool _shouldMaintainHeight = true;
    private Vector3 _groundHitPos = Vector3.zero;
    private bool _isGrounded = false;
    
    //Events
    public delegate void PlayerJumpEventHandler();
    public event PlayerJumpEventHandler PlayerJumped;

    //Getters
    public bool IsJumping => _isJumping;
    public bool IsGrounded => _isGrounded;
    public bool IsMoving => _isMoving;
    public Vector3 Velocity => _rigidBody.velocity;


    private void OnEnable()
    {
        if (PlayerInputManager.Instance != null)
        {
            PlayerInputManager.Instance.MovePressed += OnMoveInput;
            PlayerInputManager.Instance.MoveCanceled += OnCancelMoveInput;
            PlayerInputManager.Instance.JumpPressed += OnJumpInput;
            PlayerInputManager.Instance.JumpReleased += OnCancelJumpInput;
            PlayerInputManager.Instance.DodgePressed += OnDodgeInput;
        }
        else Debug.LogWarning("Could not find instance of Player Input Manager.");
        
        if (_playerEventChannel != null)
        {
            _playerEventChannel.PlayerAttackStarted += OnPlayerAttackStarted;
            _playerEventChannel.PlayerAttackEnded += OnPlayerAttackEnded;
            _playerEventChannel.PlayerHitStunStarted += OnHitStunStarted;
            _playerEventChannel.PlayerHitStunEnded += OnHitStunEnded;
        }
    }
    
    private void OnDisable()
    {
        if (PlayerInputManager.Instance != null)
        {
            PlayerInputManager.Instance.MovePressed -= OnMoveInput;
            PlayerInputManager.Instance.MoveCanceled -= OnCancelMoveInput;
            PlayerInputManager.Instance.JumpPressed -= OnJumpInput;
            PlayerInputManager.Instance.JumpReleased -= OnCancelJumpInput;
            PlayerInputManager.Instance.DodgePressed -= OnDodgeInput;
        }
        
        if (_playerEventChannel != null)
        {
            _playerEventChannel.PlayerAttackStarted -= OnPlayerAttackStarted;
            _playerEventChannel.PlayerAttackEnded -= OnPlayerAttackEnded;
            _playerEventChannel.PlayerHitStunStarted -= OnHitStunStarted;
            _playerEventChannel.PlayerHitStunEnded -= OnHitStunEnded;
        }
    }

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        if (_rigidBody == null)
        {
            Debug.LogError($"PlayerMovementController: Rigidbody is null.");
            return;
        }
        _rigidBody.useGravity = false;
    }

    private void Update()
    {
        _isGrounded = CheckForGround();
        _lastGroundedTime -= Time.deltaTime;
        _lastJumpTime -= Time.deltaTime;
    }
    
    private void FixedUpdate()
    {
        _playerEventChannel.RaisePhysicsMoveStart();
        //Apply gravity to the player.
        Vector3 gravity = Vector3.up * (_baseGravity * _gravityScale);
        if (_rigidBody.velocity.y < 0  && !_isGrounded)
        {
            //Apply extra gravity when falling.
            gravity *= _fallGravityMultiplier;
            if (!_shouldMaintainHeight) //Maintain height again after reaching apex of jump.
                _shouldMaintainHeight = true;
        } 

        _rigidBody.AddForce(gravity, ForceMode.Acceleration);

        //Simulate grounded friction.
        if (_isGrounded && _rigidBody.velocity.magnitude > 0.01f)
        {
            var frictionForce = -_rigidBody.velocity * _frictionCoefficient ;
            _rigidBody.AddForce(frictionForce);
        }

        Move();
        if (_canMove)
        {
            Jump();
        }
        
        if(_shouldMaintainHeight)
            MaintainHeight();
    }

    private void MaintainHeight()
    {
        Vector3 _rayDir = Vector3.down;
        Ray ray = new Ray(transform.TransformPoint(_rayLocalOffset), _rayDir);
        if (Physics.Raycast(ray, out var rayHit, _rayToGroundLength ,_groundLayerMask))
        {
            Vector3 vel = _rigidBody.velocity;
            Vector3 otherVel = Vector3.zero;
            Rigidbody hitBody = rayHit.rigidbody;
            if (hitBody != null)
            {
                otherVel = hitBody.velocity;
            }
            float rayDirVel = Vector3.Dot(_rayDir, vel);
            float otherDirVel = Vector3.Dot(_rayDir, otherVel);

            float relVel = rayDirVel - otherDirVel;
            float currHeight = rayHit.distance - _rideHeight;
            float springForce = (currHeight * _rideSpringStrength) - (relVel * _rideSpringDamper);
            Vector3 maintainHeightForce = springForce * _rayDir;
            _rigidBody.AddForce(maintainHeightForce);

            // Apply force to objects beneath
            if (hitBody != null)
            {
                hitBody.AddForceAtPosition(-maintainHeightForce, rayHit.point);
            }
        }
    }
    
    private void Dodge()
    {
        if (!_isGrounded || _isDodging) return;
        if (_playerData != null)
        {
            if(_playerData.RollCost > _playerData.CurrentStamina) return;
            _playerData.CurrentStamina -= _playerData.RollCost;
        }
            //Using Rootmotion from anim to move player for now.
        /*
        var moveDir = _canMove ? (Vector3)_inputDirection : Vector3.zero;
        var direction = Vector3.zero;
        moveDir.y = 0f;
        direction = moveDir.x switch
        {
            > 0 => transform.forward,
            < 0 => -transform.forward,
            _ => direction
        };
        var dodgePow =  direction* _dodgeForce * _rigidBody.mass;
        _rigidBody.AddForce(dodgePow, ForceMode.Impulse);
        */
        _canMove = false;
        _isDodging = true;
        _playerEventChannel.RaisePlayerDodged();
        StartCoroutine(DodgeRoutine());
    }

    private IEnumerator DodgeRoutine()
    {
#if UNITY_EDITOR
        _drawDodgeBox = true;
#endif
        //7 is the player layer, 8 is the enemy layer
        //Ignore collision between player and enemy for set duration.

        OnDodgeStart?.Invoke();
        Physics.IgnoreLayerCollision(7, 8, true);
        var currentTime = 0f;
        while (currentTime < _dodgeTime)
        {
            yield return new WaitForEndOfFrame();
            currentTime += Time.deltaTime;
        }
        OnDodgeEnd?.Invoke();

        //Check if inside an enemy collider.
        int maxCol = 5;
        Collider[] enemyColliders = new Collider[maxCol];
        while(Physics.OverlapBoxNonAlloc(transform.TransformPoint(_dodgeBoxOffset), Vector3.one * 0.5f,
                  enemyColliders, Quaternion.identity, 8) > 0)
        {
            yield return new WaitForEndOfFrame();
        }
        //Re-enable collision between player and enemy once player outside of an enemy collider.
        Physics.IgnoreLayerCollision(7, 8, false);
        _canMove = true;
        _isDodging = false;
        
#if UNITY_EDITOR
        _drawDodgeBox = false;
#endif
        yield return null;
    }
    
    private void Jump()
    {
        if(_lastGroundedTime >= 0f && _lastJumpTime >= 0 && !_isJumping)
        {
            _isJumping = true;
            _shouldMaintainHeight = false;
            var jumpForce = Mathf.Sqrt(_jumpHeight * (Physics.gravity.y * _gravityScale) * -2f) * _rigidBody.mass;
            _rigidBody.AddForce(jumpForce * Vector3.up, ForceMode.Impulse);
            PlayerJumped?.Invoke();
        }
    }
    
    private void Move()
    {

        var moveDir = _canMove ? (Vector3)_inputDirection : Vector3.zero;
        moveDir.y = 0; //Remove the y component of the move direction.
        var targetSpeed = _maxSpeed * moveDir.x;

        var forwardVelocity = Vector3.Dot(transform.forward, _rigidBody.velocity);
        var speedDif =  targetSpeed - forwardVelocity;
        var accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? _acceleration : _decceleration;
        var movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, _velocityPower) * Mathf.Sign(speedDif);
        
        //TODO: Refactor later.
        switch (_isGrounded)
        {
            case true:
                if (CheckForSlope(out var slopeNormal)) //If the player is on a slope, apply movement along the slope.
                {
                    //Project the direction onto the slope normal to get the direction of movement along surface.
                    var slopeDir = Vector3.ProjectOnPlane(transform.forward, slopeNormal);
                    _rigidBody.AddForce(slopeDir * movement); 
                    if(DebugOn)
                        Debug.DrawRay(transform.position + Vector3.up * 2, (slopeDir * movement).normalized, Color.blue, 0.1f);
                }
                else
                {
                    _rigidBody.AddForce(transform.forward * movement);
                    if(DebugOn)
                        Debug.DrawRay(transform.position + Vector3.up * 2, (movement * transform.forward).normalized, Color.red, 0.1f);
                }
                break;
            //If the player is in the air, apply a modifier to the movement.
            case false:
                if (moveDir.Equals(Vector3.zero)) break;
                var direction = moveDir.x > 0 ? 1 : -1;
                _rigidBody.AddForce(transform.forward * (direction * _airDirectionModifier), ForceMode.Acceleration);
                break;
        }
    }

    //Spherecast check if the player is on the ground.
    private bool CheckForGround()
    {
        Ray groundRay = new Ray(transform.TransformPoint(_rayLocalOffset), Vector3.down);
        var grounded = Physics.SphereCast(groundRay, _sphereRadius,out var hit, _sphereRayLength, _groundLayerMask);
        if (grounded)
        {
            _lastGroundedTime = _jumpCoyoteTime;
            _groundHitPos = hit.point;
        }
        return grounded;
    }
    
    //Checks if player is on a sloped surface.
    private bool CheckForSlope(out Vector3 slopeNormal)
    {
        slopeNormal = Vector3.zero;
        Ray groundRay = new Ray(transform.TransformPoint(_rayLocalOffset), Vector3.down);
        if (!Physics.Raycast(groundRay, out var hitInfo, _sphereRayLength, _groundLayerMask)) return false;
        if (hitInfo.normal == Vector3.up) return false;
        slopeNormal = hitInfo.normal;
        return true;
    }

    #region PLAYER INPUT CHANNEL CALLBACKS

    private void OnDodgeInput()
    {
        Dodge();
    }
    
    private void OnJumpInput()
    {
        _lastJumpTime = _jumpInputBufferTime;
    }
    
    private void OnCancelJumpInput()
    {
        if (_rigidBody.velocity.y > 0f && _isJumping)
        {
            //Cut the jump short on releasing the jump button.
            _rigidBody.AddForce(Vector3.down * _rigidBody.velocity.y * (1- _jumpCutMultiplier), ForceMode.Impulse);
        }
        _shouldMaintainHeight = true;
        _isJumping = false;
        _lastJumpTime = 0f;
    }

    private void OnMoveInput(Vector2 direction)
    {
        _inputDirection = direction;
        //If there was no horizontal axis found, the player is not moving.
        _isMoving = direction.x == 0 ? false : true;
    }

    private void OnCancelMoveInput()
    {
        _inputDirection = Vector2.zero;
        _isMoving = false;
    }
    #endregion

    //Player Event Channel Callbacks
    #region PLAYER EVENT CHANNEL CALLBACKS
    private void OnPlayerAttackStarted(PlayerAttackController.AttackEventContext context)
    {
        _canMove = false;
    }
    private void OnPlayerAttackEnded()
    {
        _canMove = true;
    }
    
    private void OnHitStunStarted()
    {
        _canMove = false;
    }
    
    private void OnHitStunEnded()
    {
        _canMove = true;
    }
    
    #endregion
    
    #if UNITY_EDITOR
    [ShowInInspector]
    private bool _drawDodgeBox = false;
    private void OnDrawGizmos()
    {
        if(DebugOn == false) return;
        Gizmos.color = Color.yellow;
        if(_drawDodgeBox)
            Gizmos.DrawWireCube(transform.TransformPoint(_dodgeBoxOffset), Vector3.one * 0.5f);
        Gizmos.color = Color.red;
        if (_isGrounded)
        {
            Gizmos.color = Color.green;
        }
        else
        {
            Gizmos.DrawWireSphere(transform.TransformPoint(_rayLocalOffset) + (-transform.up * _sphereRayLength), _sphereRadius);
        }
        Gizmos.DrawWireSphere(transform.TransformPoint(_rayLocalOffset), 0.05f);
        Gizmos.DrawWireSphere(_groundHitPos, _sphereRadius);
        Gizmos.DrawRay(transform.TransformPoint(_rayLocalOffset), Vector3.down * _sphereRayLength);
    }
    #endif
}
