using System;
using System.Collections;
using Core.GameItems;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public enum FacingDirection
{
    Forward,
    Back
}

public class PlayerBaseController : MonoBehaviour, IDamageable
{
    public static PlayerBaseController Instance;
    
    [Header("Components")]
    [Required]
    [SerializeField] private Rigidbody _rigidBody;
    [SerializeField] private GameObject _mesh;
    [SerializeField] private Animator _animator;
    [Header("Properties")]
    [ReadOnly][ShowInInspector]
    private FacingDirection _facingDirection = FacingDirection.Forward;
    [SerializeField] private float _rotationTime = 0.25f;
    [SerializeField] private float _stunDuration = 0.5f;
    [LabelText("Knockback Multiplier")]
    [SerializeField] private float _knockBackMulti = 1f;
    [Header("Player Event Channel")]
    [SerializeField] private PlayerEventChannel _playerEventChannel;
    [Tooltip("Event channel to raise when the player is hit.")]
    [SerializeField] private VoidEventChannel _onHitEventChannel;
    [Header("Player Data")]
    [SerializeField] private PlayerData _playerData;

    [Header("Unity Event")] public UnityEvent OnPlayerHurt;

    public FacingDirection FacingDirection => _facingDirection;

    private Vector3 _inputDirection;
    private bool _isFocusing = false;
    private Coroutine _stunRoutine;
    private bool _isDead;
    
    private void Awake()
    {
        _isDead = false;
        if(Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        
        if(_rigidBody == null)
            _rigidBody = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        PlayerInputManager.Instance.MovePressed += OnMovePressed;
        PlayerInputManager.Instance.MoveCanceled += OnMoveReleased;
        PlayerInputManager.Instance.FocusPressed += OnFocusPressed;
        PlayerInputManager.Instance.FocusReleased += OnFocusReleased;
        PlayerInputManager.Instance.QuickSlotTriggerPressed += OnQuickSlotTriggerPressed;
        PlayerInputManager.Instance.QuickSlotTriggerReleased += OnQuickSlotTriggerReleased;
        PlayerInputManager.Instance.QuickSlot1Pressed += OnQuickSlot1Pressed;
        PlayerInputManager.Instance.QuickSlot2Pressed += OnQuickSlot2Pressed;

    }

    private void OnDisable()
    {
        PlayerInputManager.Instance.MovePressed -= OnMovePressed;
        PlayerInputManager.Instance.MoveCanceled -= OnMoveReleased;
        PlayerInputManager.Instance.FocusPressed -= OnFocusPressed;
        PlayerInputManager.Instance.FocusReleased -= OnFocusReleased;
        PlayerInputManager.Instance.QuickSlotTriggerPressed -= OnQuickSlotTriggerPressed;
        PlayerInputManager.Instance.QuickSlotTriggerReleased -= OnQuickSlotTriggerReleased;
        PlayerInputManager.Instance.QuickSlot1Pressed -= OnQuickSlot1Pressed;
        PlayerInputManager.Instance.QuickSlot2Pressed -= OnQuickSlot2Pressed;
    }

    private void Update()
    {
        if(MoveHeld && !_isFocusing) // TODO: Move into a coroutine later.
            ChangeDirection(_inputDirection);
        RegenerateStamina();
    }
    
    public void AddItemToInventory(ItemData itemData, int quantity = 1)
    {
        if(AlertDisplay.Instance != null)
            AlertDisplay.Instance.ShowMessage($"{itemData.DisplayName} x{quantity} added to inventory.", 3f);
        _playerData.Inventory.AddItem(itemData, quantity);
    }
    
    [Button("Use Quick Slot")]
    private void UseQuickSlot(int index)
    {
        if (index < 0 || index >= _playerData.Inventory.QuickSlots.Length) return;
        if (_playerData.Inventory.QuickSlots[index] == null) return;
        var item = _playerData.Inventory.QuickSlots[index];
        switch (item)
        {
            case RecoveryItem recovery:
                recovery.Use();
                break;
            case ConsumableItem consumable:
                consumable.Use();
                break;
            default:
                Debug.LogError("Invalid item type in quick slot.");
                return;
        }

        if (((ConsumableItem)item).Reusable) return;
        // Remove item from inventory if it's not reusable.
        _playerData.Inventory.RemoveItem(item.DataReference);
    }

    /// <summary>
    /// Save the player's position to the player data.
    /// </summary>
    /// <param name="type"></param>
    public void SavePosition(PositionType type)
    {
        switch (type)
        {
            case PositionType.SavePosition:
                _playerData.SavedPosition = transform.position;
                _playerData.SavedSplineName = GetComponent<SplineWalker>().Spline.name;
                _playerData.LastSavedSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                break;
            case PositionType.CheckpointPosition:
                _playerData.LastCheckpointPosition = transform.position;
                _playerData.LastCheckpointSplineName = GetComponent<SplineWalker>().Spline.name;
                _playerData.LastCheckpointSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                break;
            default:
                Debug.LogError("Invalid position type.");
                break;
        }
    }
    
    
    public void TakeDamage(Attack incomingAttack, Vector3 direction)
    {
        if(_isDead) return;
        OnPlayerHurt?.Invoke();
        _playerData.CurrentHealth -= incomingAttack.Damage;
        DamageUIManager.Instance.SpawnDamagePopUp(transform.position + Vector3.up + transform.right, incomingAttack.Damage, DamagePopUp.DamageType.Default);
        
        if(AudioManager.Instance != null)
            AudioManager.PlayOneShot(AudioManager.Instance.CombatEvents.Hit, transform.position);
        
        HitStun();
        HitStopManager.Instance.HitStop(incomingAttack.HitStopDuration);
        if(_onHitEventChannel != null)
            _onHitEventChannel.RaiseEvent();

        direction.y = 0f;
        var horizontalForce = direction.normalized * (incomingAttack.BaseKnockback * _rigidBody.mass);
        var verticalForce = Vector3.up * (incomingAttack.VerticalKnockup * _rigidBody.mass);
        _rigidBody.AddForce((horizontalForce + verticalForce) * _knockBackMulti, ForceMode.Impulse);
        if (_playerData.CurrentHealth <= 0)
            StartCoroutine(DeathRoutine());
    }

    private void ChangeDirection(Vector2 direction)
    {
        var forwardVelocity = Vector3.Dot(transform.forward,_rigidBody.velocity.normalized);
        if (direction.x < 0 && _facingDirection == FacingDirection.Forward && forwardVelocity < 0)
        {
            _facingDirection = FacingDirection.Back;
            _mesh.transform.DOLocalRotate(new Vector3(0, 180, 0), _rotationTime);
        }
        else if (direction.x > 0 && _facingDirection == FacingDirection.Back && forwardVelocity > 0)
        {
            _facingDirection = FacingDirection.Forward;
            _mesh.transform.DOLocalRotate(new Vector3(0, 0, 0), _rotationTime);
            
        }
    }

    private void HitStun()
    {
        if(_stunRoutine != null)
            StopCoroutine(_stunRoutine);
        _stunRoutine = StartCoroutine(StunRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        _animator.SetBool("IsDead", true);
        PlayerInputManager.Instance.SetInWorldControlState(false);
        _isDead = true;
        yield return new WaitForSeconds(3f);
        GameManager.Instance.ResetToCheckPoint();
        yield return new WaitForSeconds(2f);
        _animator.SetBool("IsDead", false);
        _isDead = false;
        PlayerInputManager.Instance.SetInWorldControlState(true);
        //Reset to checkpoint and reset health.
        _playerData.CurrentHealth = _playerData.MaxHealth;
        _playerData.CurrentEx = _playerData.MaxEx;
        _playerData.CurrentStamina = _playerData.MaxStamina;
        StatusDisplayManager.Instance.ForceUpdate();
    }
    
    private IEnumerator StunRoutine()
    {
        _playerEventChannel.RaisePlayerHitStunStart();
        var timer = 0f;
        while (timer <= _stunDuration)
        {
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        _playerEventChannel.RaisePlayerHitStunEnd();
    }
    
    private void RegenerateStamina()
    {
        if (_playerData.MaxStamina <= _playerData.CurrentStamina) return;
        _playerData.CurrentStamina += _playerData.StaminaRegenRate * Time.deltaTime;
        //stamina = Mathf.Clamp(stamina, 0f, _playerData.MaxStamina);
    }
    
    public void RecoverHealth(float amount)
    {
        _playerData.CurrentHealth += amount;
        //_playerData.CurrentHealth = Mathf.Clamp(_playerData.CurrentHealth, 0f, _playerData.MaxHealth);
    }
    
    public void RecoverStamina(float amount)
    {
        _playerData.CurrentStamina += amount;
        _playerData.CurrentStamina = Mathf.Clamp(_playerData.CurrentStamina, 0f, _playerData.MaxStamina);
    }

    public void RecoverEX(float amount)
    {
        _playerData.CurrentEx += amount;
        _playerData.CurrentEx = Mathf.Clamp(_playerData.CurrentEx, 0f, _playerData.MaxEx);
    }
    
    public void RecoverHealthPercentage(float percentage)
    {
        _playerData.CurrentHealth += _playerData.MaxHealth * percentage;
        _playerData.CurrentHealth = Mathf.Clamp(_playerData.CurrentHealth, 0f, _playerData.MaxHealth);
    }

    public void HidePlayerMesh(bool value)
    {
        _mesh.SetActive(!value);
    }

    #region PLAYER INPUT CALLBACKS
    private bool MoveHeld = false;
    private void OnQuickSlot1Pressed()
    {
        UseQuickSlot(0);
    }
    
    private void OnQuickSlot2Pressed()
    {
        UseQuickSlot(1);
    }
    
    private void OnMovePressed(Vector2 inputDir)
    {
        MoveHeld = true;
        _inputDirection = inputDir;
        if (!_isQuickSlotTriggerHeld) return;
        switch (inputDir.y)
        {
            case > 0:
                UseQuickSlot(0);
                break;
            case < 0:
                UseQuickSlot(1);
                break;
        }
    }

    private void OnMoveReleased()
    {
        MoveHeld = false;
        _inputDirection = Vector3.zero;
    }
    
    private void OnFocusPressed()
    {
        _isFocusing = true;
        _playerEventChannel.RaisePlayerFocusStart();
    }
    
    private void OnFocusReleased()
    {
        _isFocusing = false;
        _playerEventChannel.RaisePlayerFocusEnd();
    }

    private bool _isQuickSlotTriggerHeld = false;
    private void OnQuickSlotTriggerPressed()
    {
        _isQuickSlotTriggerHeld = true;    
    }
    
    private void OnQuickSlotTriggerReleased()
    {
        _isQuickSlotTriggerHeld = false;
    }
    
    #endregion
}
