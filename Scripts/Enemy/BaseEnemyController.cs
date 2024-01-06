using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class BaseEnemyController : MonoBehaviour, IDamageable, IParryable
{
    [FormerlySerializedAs("_mesh")]
    [Header("Component References")]
    [Required]
    [SerializeField] protected Rigidbody _rigidBody;
    [Required]
    [SerializeField] protected Collider _collider;
    [Required]
    [SerializeField] protected GameObject _meshContainer;
    [Required]
    [SerializeField] protected Animator _animator;
    [Required]
    [SerializeField] protected HitboxManager _hitboxManager;
    [SerializeField] protected HurtboxManager _hurtboxManager;
    [SerializeField] protected BehaviorTree _behaviorTree;
    [SerializeField] protected UIMeterDelay _healthMeter;
    [FoldoutGroup("FX", false)]
    [SerializeField] protected ParticleSystem _weaponTrailFX;
    [FoldoutGroup("FX", false)]
    [SerializeField] protected ParticleSystem _hitFX;
    
    [Header("Properties")]
    [SerializeField] protected FacingDirection _startingFacingDirection = FacingDirection.Forward;
    [LabelText("Current Facing Direction")]
    [ReadOnly][ShowInInspector] protected FacingDirection _facingDirection = FacingDirection.Forward;
    public float MaxHealth = 100f;
    [ProgressBar(0, "@MaxHealth",1f,0.278f,0.341f)]
    public float health = 100f;
    [ProgressBar(0,100,0.341f,0.376f,0.435f)]
    public float poise = 100f;
    [SerializeField] protected AttackDataSet _attackData;
    [SerializeField] protected float _rotationTime = 0.25f;
    
    [Header("Movement Properties")]
    public bool IsMoving = false;
    public bool IsMovingBackwards = false;
    public float speed = 4f;
    [SerializeField] private float _acceleration = 4f;
    [SerializeField] private float _deceleration = 4f;
    [SerializeField] private float _velocityPower = 1f;
    
    [Header("On Hit Properties")]
    public Vector3 shake;
    public int vibrato;
    public float duration;
    
    [Header("Gravity Properties")]
    [SerializeField] bool _applyGravity = true;
    [SerializeField] protected float _gravity = -9.81f;
    [SerializeField] protected float _gravityScale = 1f;
    [SerializeField] protected float _fallMultiplier = 2.5f;

    protected bool _isTurning = false;
    protected Attack _recievedAttack;
    protected bool _superArmor = false;

    [Header("Events")]
    [FoldoutGroup("Events", false)]
    public UnityEvent OnAttackStart;
    [FoldoutGroup("Events", false)]
    public UnityEvent OnDeath;
    [FoldoutGroup("Events", false)]
    public UnityEvent OnReset;

    [Header("Debug")]
    [ReadOnly][ShowInInspector]
    protected Attack _currentAttack;
    protected Coroutine _attackRoutine;
    
    protected Dictionary<int, Attack> _attackMap = new Dictionary<int, Attack>();
    public Dictionary<int, Attack> AttackMap => _attackMap;
    public Rigidbody Rigidbody => _rigidBody;
    public bool IsTurning => _isTurning;
    public Attack CurrentAttack => _currentAttack;
    public FacingDirection FacingDirection => _facingDirection;
    public Attack RecievedAttack => _recievedAttack;
    
    public GameObject MeshContainer => _meshContainer;

    protected virtual void Awake()
    {
        _rigidBody.GetComponent<Rigidbody>();
        _rigidBody.useGravity = false;
        InitializeAttacks();
    }

    protected void Start()
    {
        if (_startingFacingDirection != FacingDirection.Back) return;
        _meshContainer.transform.rotation = Quaternion.Euler(new Vector3(0, 90f, 0));
        _facingDirection = FacingDirection.Back;
    }

    protected void Update()
    {
        if(poise < 100f)
            poise += Time.deltaTime * 5f;
        var forwardVelocity = Vector3.Dot(_meshContainer.transform.forward, _rigidBody.velocity);
        if (_rigidBody.velocity.magnitude > 0.01f && IsMoving)
        {
            _animator.SetBool("IsMoving", true);
            if (forwardVelocity < 0)
            {
                _animator.SetBool("IsMovingBackwards", true);
            }
            else
                _animator.SetBool("IsMovingBackwards", false);
        }
        else
        {
            _animator.SetBool("IsMoving", false);
        }
    }

    protected virtual void FixedUpdate()
    {
        ApplyGravity();
        Move();
    }

    protected virtual void InitializeAttacks()
    {
        if (_attackData == null)
        {
            Debug.LogWarning($"{this.gameObject.name}: Attack data set was empty.");
            return;
        }
        foreach(var data in _attackData.GetAttackDataList())
        {
            var hash = data.Name.GetHashCode();
            if (_attackMap.ContainsKey(hash)) continue;
            var attack = data.CreateAttack(_animator, _hitboxManager);
            _attackMap.Add(hash, attack);
        }
    }
    
    public void Dodge()
    {
        _animator.SetTrigger("Dodge");
        //Disable hitbox
    }
    
    public virtual void Attack(string attackName)
    {
        var hash = attackName.GetHashCode();
        if (!_attackMap.ContainsKey(hash))
        {
            Debug.LogWarning($"{this.gameObject.name}: Attack {attackName} does not exist.");
            return;
        }
        
        if (_currentAttack != null)
        {
            _currentAttack.Cancel(); // cancel the current attack
        }
        _currentAttack = _attackMap[hash];
        _superArmor = _currentAttack.SuperArmor;
        if(_weaponTrailFX != null)
            _weaponTrailFX.Play();
        OnAttackStart?.Invoke();
        _currentAttack.Execute();
        _animator.Play(_currentAttack.GetAnimationStateName());
        if (_attackRoutine != null) 
            StopCoroutine(_attackRoutine);
        _attackRoutine = StartCoroutine(AttackRoutine(_currentAttack));
    }

    public virtual void StopAttack()
    {
        if(_currentAttack == null) return;
        if(_attackRoutine != null)
            StopCoroutine(_attackRoutine);
        _currentAttack.Cancel();
        if(_weaponTrailFX != null)
            _weaponTrailFX.Stop();
        if(_currentAttack.SuperArmor)
            _superArmor = false;
        _currentAttack = null;
    }

    protected IEnumerator AttackRoutine(Attack attack)
    {
        while (attack.IsActive)
        {
            attack.Tick();
            yield return new WaitForEndOfFrame();
        }
        StopAttack();
    }
    public virtual void TakeDamage(Attack incomingAttack, Vector3 direction)
    {
        _recievedAttack = incomingAttack;
        if (_behaviorTree != null)
        {
            _behaviorTree.SendEvent("TakeDamage");
        }

        if (!_superArmor)
        {
            HitStopManager.Instance.HitStop(incomingAttack.HitStopDuration);
            PlayHitAnimation();
        }
        
        if(AudioManager.Instance != null)
            AudioManager.PlayOneShot(AudioManager.Instance.CombatEvents.Hit, transform.position);

        var damage = incomingAttack.Damage;
        if(_hitFX != null)
            _hitFX?.Play();
        DamageUIManager.Instance.SpawnDamagePopUp(transform.position + Vector3.up + transform.right, damage, DamagePopUp.DamageType.Default);
        _meshContainer.transform.localPosition = Vector3.zero;
        _meshContainer.transform.DOShakePosition(duration, shake, vibrato, 90f, false, true);
        
        health -= damage;
        var progress = health / MaxHealth;
        if(_healthMeter != null)
            _healthMeter.ChangeProgress(Mathf.Clamp01(progress));
        direction.y = 0f;
        var knockbackForce = direction.normalized * (incomingAttack.BaseKnockback * _rigidBody.mass);
        var knockupForce = Vector3.up * (incomingAttack.VerticalKnockup * _rigidBody.mass);
        _rigidBody.AddForce(knockbackForce + knockupForce, ForceMode.Impulse);
        if(health <= 0f)
            Die();
    }

    protected virtual void Move()
    {
        var magnitude = IsMoving ? 1f : 0f;
        var direction = _facingDirection == FacingDirection.Forward ? 1 : -1;
        var backward = IsMovingBackwards ? -1 : 1;
        
        var targetSpeed = magnitude * direction * backward * speed;
        
        var forwardVelocity = Vector3.Dot(transform.forward, _rigidBody.velocity);
        var speedDif =  (targetSpeed ) - forwardVelocity;
        var accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? _acceleration : _deceleration;
        var movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, _velocityPower) * Mathf.Sign(speedDif);
        _rigidBody.AddForce(transform.forward * (movement), ForceMode.Acceleration);
    }

    protected virtual void ApplyGravity()
    {
        if(!_applyGravity)
            return;
        var gravityForce = Vector3.up * (_gravity * _gravityScale);
        if (_rigidBody.velocity.y < 0)
            gravityForce *= _fallMultiplier;
        _rigidBody.AddForce(gravityForce, ForceMode.Acceleration);
    }
    
    public virtual void PlayHitAnimation()
    {
        var rand = UnityEngine.Random.Range(1, 4);
        switch (rand)
        {
            case 1:
                _animator.SetTrigger("Hurt1");
                break;
            case 2:
                _animator.SetTrigger("Hurt2");
                break;
            case 3:
                _animator.SetTrigger("Hurt3");
                break;
        }
    }
    
    public virtual void ChangeDirection(Vector2 direction)
    {
        _isTurning = true;
        if (direction.x < 0 && _facingDirection == FacingDirection.Forward)
        {
            _animator.SetTrigger("Turn");
            _facingDirection = FacingDirection.Back;
            _meshContainer.transform.DOLocalRotate(new Vector3(0, 180, 0), _rotationTime)
                .OnComplete(()=> _isTurning = false);
        }
        else if (direction.x > 0 && _facingDirection == FacingDirection.Back)
        {
            _animator.SetTrigger("Turn");
            _facingDirection = FacingDirection.Forward;
            _meshContainer.transform.DOLocalRotate(new Vector3(0, 0, 0), _rotationTime)
                .OnComplete(()=> _isTurning = false);
        }
        else
        {
            _isTurning = false;
        }
    }

    public virtual void ChangeDirection(FacingDirection direction)
    {
        var dirVector = direction switch
        {
            FacingDirection.Forward => Vector2.right,
            FacingDirection.Back => Vector2.left,
            _ => Vector2.zero
        };
        ChangeDirection(dirVector);
    }
    
    public void OnParried()
    {
        _behaviorTree.SendEvent("TakeDamage");
        _animator.SetTrigger("Parried");
        //StopAttack();
        Debug.Log("Parried");
    }

    public virtual void Die()
    {
        _animator.SetBool("Dead", true);
        if(_healthMeter != null)
            _healthMeter.FadeOut();
        //If currently attacking, stop attack.
        if(_currentAttack != null)
            StopAttack();
        //Disable AI
        if(_behaviorTree != null)
            _behaviorTree.DisableBehavior();
        //Disable Hurtboxes
        _hurtboxManager.SetAllHurtboxesActive(false);
        //Slow down time effect
        if(HitStopManager.Instance != null)
            HitStopManager.Instance.HitStop(0.25f);
        //Disable physics and collider.
        _collider.enabled = false;
        _rigidBody.isKinematic = true;
        
        OnDeath?.Invoke();
    }

    public virtual void Reset()
    {
        _animator.SetBool("Dead", false);
        _healthMeter.FadeIn();
        //Enable AI
        if(_behaviorTree != null)
            _behaviorTree.EnableBehavior();
        _hurtboxManager.SetAllHurtboxesActive(true);
        //Enable physics and collider.
        _collider.enabled = true;
        _rigidBody.isKinematic = false;
        health = MaxHealth;
        _healthMeter.ChangeProgress(1f);
        OnReset?.Invoke();
    }

    #region DEBUG MENU
#if UNITY_EDITOR
    [FoldoutGroup("Debug Menu", false)]
    [ValueDropdown("GetAttackNames")]
    [SerializeField] private string _debugAttackName; 
    [FoldoutGroup("Debug Menu", false)]
    [Button("Play Attack")]
    protected virtual void DebugAttack()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Cannot play attack in edit mode.");
            return;
        }
        Attack(_debugAttackName);
    }
    
    [FoldoutGroup("Debug Menu", false)]
    [Button("Turn", ButtonSizes.Small, ButtonStyle.CompactBox)]
    protected virtual void DebugTurn()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Cannot turn in edit mode.");
            return;
        }
        if(_facingDirection == FacingDirection.Forward)
            ChangeDirection(Vector2.left);
        else if(_facingDirection == FacingDirection.Back)
            ChangeDirection(Vector2.right);
    }

    [FoldoutGroup("Debug Menu", false)]
    [Button("Kill", ButtonSizes.Small, ButtonStyle.CompactBox)]
    protected virtual void DebugKill()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Cannot kill in edit mode.");
            return;
        }
        Die();
    }

    [FoldoutGroup("Debug Menu", false)]
    [Button("Reset", ButtonSizes.Small, ButtonStyle.CompactBox)]
    protected virtual void DebugReset()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Cannot reset in edit mode.");
            return;
        }
        Reset();
    }
    
    [FoldoutGroup("Debug Menu", false)]
    [Button("Dodge", ButtonSizes.Small, ButtonStyle.CompactBox)]
    protected virtual void DebugDodge(){
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Cannot dodge in edit mode.");
            return;
        }
        Dodge();
    }
    
    protected virtual IEnumerable GetAttackNames()
    {
        var attackNames = new string[_attackData.GetAttackDataList().Count];
        for (var i = 0; i < _attackData.GetAttackDataList().Count; i++)
        {
            attackNames[i] = _attackData.GetAttackDataList()[i].Name;
        }
        return attackNames;
    }
#endif
    #endregion
}
