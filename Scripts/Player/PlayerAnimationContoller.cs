using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Handles the animation of the player.
/// </summary>


[DefaultExecutionOrder(101)]
public class PlayerAnimationContoller : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private PlayerMovementController _playerMovementController;
    [SerializeField] private PlayerBaseController _playerBaseController;

    private bool _isFocusing;
    
    [Header("Event Channel")]
    [SerializeField] private PlayerEventChannel _playerEventChannel;

    private void OnEnable()
    {
        if (_playerMovementController != null)
        {
            _playerMovementController.PlayerJumped += OnJump;
        }

        if (_playerEventChannel != null)
        {
            _playerEventChannel.PlayerAttackStarted += OnAttackStarted;
            _playerEventChannel.PlayerDodged += OnDodge;
            _playerEventChannel.PlayerFocusStarted += OnFocusStart;
            _playerEventChannel.PlayerFocusEnded += OnFocusEnd;
            _playerEventChannel.PlayerHitStunStarted += OnHitStunStart;
        }
    }
    
    private void OnDisable()
    {
        if (_playerMovementController != null)
        {
            _playerMovementController.PlayerJumped -= OnJump;
        }

        if (_playerEventChannel != null)
        {
            _playerEventChannel.PlayerAttackStarted -= OnAttackStarted;
            _playerEventChannel.PlayerDodged -= OnDodge;
            _playerEventChannel.PlayerFocusStarted -= OnFocusStart;
            _playerEventChannel.PlayerFocusEnded -= OnFocusEnd;
        }
    }

    private void Awake()
    {
        if (_animator != null) return;
        _animator = GetComponent<Animator>();
        if(_animator == null)
            Debug.LogError("Animator is null and could not be found.");
        if (_playerMovementController == null)
        {
            Debug.LogError("PlayerMovementController is null. Disabling script.");
            enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_playerMovementController.Velocity.magnitude > 0.1f && _playerMovementController.IsMoving
                                                                && _playerMovementController.IsGrounded)
        {
            _animator.SetBool("IsMoving", true);
            _animator.SetFloat("xVelocity", Mathf.Abs(_playerMovementController.Velocity.magnitude));
            if(_isFocusing)
            {
                var forwardVelocity = Vector3.Dot(transform.root.forward, _playerMovementController.Velocity); 
                if (forwardVelocity < 0)
                {
                    if(_playerBaseController.FacingDirection == FacingDirection.Back)
                        _animator.SetBool("IsMovingBackwards", false);
                    if(_playerBaseController.FacingDirection == FacingDirection.Forward)
                        _animator.SetBool("IsMovingBackwards", true);
                }
                else
                {
                    if(_playerBaseController.FacingDirection == FacingDirection.Back)
                        _animator.SetBool("IsMovingBackwards", true);
                    if(_playerBaseController.FacingDirection == FacingDirection.Forward)
                        _animator.SetBool("IsMovingBackwards", false);
                }
            }
        }
        else
        {
            _animator.SetBool("IsMoving", false);
            _animator.SetFloat("xVelocity", 0);
            _animator.SetBool("IsMovingBackwards", false);
        }

        var isFalling = _playerMovementController.Velocity.y < 0 && !_playerMovementController.IsGrounded;
        _animator.SetBool("IsFalling", isFalling);
    }

    #region PLAYER EVENT CALLBACKS
    
    private void OnHitStunStart()
    {
        var rand = Random.Range(1, 3);
        switch (rand)
        {
            case 1:
                _animator.SetTrigger("Hit1");
                break;
            case 2:
                _animator.SetTrigger("Hit2");
                break;
        }
    }
    
    private void OnAttackStarted(PlayerAttackController.AttackEventContext context)
    {
        _animator.Play(context.AnimationStateName);
    }

    private void OnJump()
    {
        _animator.SetTrigger("Jump");
    }
    
    private void OnDodge()
    {
        _animator.SetTrigger("Dodge");
    }

    private void OnFocusStart()
    {
        _isFocusing = true;
    }

    private void OnFocusEnd()
    {
        _isFocusing = false;
        _animator.SetBool("IsMovingBackwards", false);
    }
    #endregion
}
