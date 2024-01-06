using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class PlayerAttackController : MonoBehaviour
{
    private static Dictionary<Vector2, PlayerAction.ActionType> _directionMap = new Dictionary<Vector2, PlayerAction.ActionType>()
    {
        { DirectionalVector.Down, PlayerAction.ActionType.Down },
        { DirectionalVector.Left, PlayerAction.ActionType.Back },
        { DirectionalVector.Right, PlayerAction.ActionType.Forward },
        { DirectionalVector.Up, PlayerAction.ActionType.Up },
        { DirectionalVector.DownRight, PlayerAction.ActionType.DownForward },
        { DirectionalVector.DownLeft, PlayerAction.ActionType.DownBack }
    };
    
    [SerializeField] private PlayerData _runtimePlayerData;
    [Header("Components Reference")]
    [SerializeField] private PlayerBaseController _playerBaseController;
    [SerializeField] private HitboxManager _hitboxManager;
    [SerializeField] private GameObject _playerMeshContainer; //Reference to the player mesh container to determine direction.
    [SerializeField] private ParticleSystem _weaponFX;
    [Header("Properties")]
    [Tooltip("Maximum time in seconds that an action can be buffered before expiration.")]
    [SerializeField] private float _maxBufferTime = 0.5f;
    [Tooltip("Time in seconds that the player is locked into a parry window.")]
    [SerializeField] private float _parryWindow = 0.15f;
    [FormerlySerializedAs("currentWeaponStance")]
    [Header("Attack Sets")]
    [SerializeField] private WeaponStanceData _currentWeaponStance;
    [SerializeField] private List<WeaponStanceData> _weaponStanceData;
    
    [Header("Event Channel")]
    [SerializeField] private PlayerEventChannel _playerEventChannel;
    [Header("Debug")]
    [Tooltip("Enable debug logging.")]
    [SerializeField] private bool _debugLog = false;
    [ReadOnly][ShowInInspector]
    private Attack currentAttack = null;
    [ReadOnly]
    [SerializeField] private List<PlayerAction> _playerActions = new List<PlayerAction>();
    
    [Header("Unity Events")]
    public UnityEvent OnAttackStarted; 

    private Dictionary<int, Attack> _attackMap = new Dictionary<int, Attack>();
    private bool _isAttacking = false;
    private bool _isParrying = false;
    private bool _canAttack = true;
    private Coroutine _attackRoutine = null;

    //Context data sent with attack events.
    public struct AttackEventContext
    {
        public Attack Attack;
        public string AnimationStateName;
        public Vector2 Direction;
    }

    private void Awake()
    {
        InitializeAttackSets();
    }

    private void OnEnable()
    {
        PlayerInputManager.Instance.LightAttackPressed += OnLightAttackPressed;
        PlayerInputManager.Instance.HeavyAttackPressed += OnHeavyAttackPressed;
        PlayerInputManager.Instance.MovePressed += OnMovePressed;
        PlayerInputManager.Instance.WeaponQuickSwitchPressed += OnWeaponQuickSwitchPressed;
        PlayerInputManager.Instance.WeaponSpecialActionPressed += OnWeaponSpecialActionPressed;

        if (_playerEventChannel != null)
        {
            _playerEventChannel.PlayerHitStunStarted += OnHitStunStarted;
            _playerEventChannel.PlayerHitStunEnded += OnHitStunEnded;
        }
    }

    private void OnDisable()
    {
        PlayerInputManager.Instance.LightAttackPressed -= OnLightAttackPressed;
        PlayerInputManager.Instance.HeavyAttackPressed -= OnHeavyAttackPressed;
        PlayerInputManager.Instance.MovePressed -= OnMovePressed;
        PlayerInputManager.Instance.WeaponQuickSwitchPressed -= OnWeaponQuickSwitchPressed;
        PlayerInputManager.Instance.WeaponSpecialActionPressed -= OnWeaponSpecialActionPressed;

        if (_playerEventChannel != null)
        {
            _playerEventChannel.PlayerHitStunStarted -= OnHitStunStarted;
            _playerEventChannel.PlayerHitStunEnded -= OnHitStunEnded;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Remove old actions that have expired based on buffer timer.
        if (_playerActions.Count == 0) return;
        foreach (var action in _playerActions.ToArray())
        {
            if(action.TimePerformed + _maxBufferTime < Time.time)
                _playerActions.Remove(action);
        }
    }

    //TODO: Temporary method for testing.
    public void CancelAttack()
    {
        if (currentAttack == null) return;
        if(_weaponFX != null)
            _weaponFX.Stop();
        currentAttack.Cancel();
        currentAttack = null;
    }

    //Set up the attack data
    private void InitializeAttackSets()
    {
        void AddToAttackMap(AttackData data, Animator animator)
        {
            var attack = data.CreateAttack(animator, _hitboxManager);
            var hash = data.Name.GetHashCode();
            _attackMap.Add(hash, attack);
            if(_debugLog)
                Debug.Log("Initialized Attack: " + data.Name); 
        }
        
        var animator = gameObject.GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator is null and could not be found. Cannot initialize attacks.");
            return;
        }

        foreach (var weapon in _weaponStanceData)
        {
            if (weapon.LightAttackDataSet != null)
            {
                foreach(AttackData data in weapon.LightAttackDataSet.GetAttackDataList())
                {
                    AddToAttackMap(data, animator);
                }
            }
            if(weapon.HeavyAttackDataSet != null)
            {
                foreach(AttackData data in weapon.HeavyAttackDataSet.GetAttackDataList())
                {
                    AddToAttackMap(data, animator);
                }
            }
            if (weapon.SpecialAttackDataSet != null)
            {
                foreach(AttackData data in weapon.SpecialAttackDataSet.GetAttackDataList())
                {
                    AddToAttackMap(data, animator);
                }
            }
        }
    }

    /// <summary>
    ///Change the current weapon stance to the one at the given index.
    /// </summary>
    /// <param name="index">value of -1 un-equips to no stance.</param>
    [Button("Change Weapon Stance")]
    public void ChangeWeaponStance(int index)
    {
        if (index == -1)
        {
            _currentWeaponStance.OnWeaponStanceRemovedChannel.RaiseEvent();
            _currentWeaponStance = null;
            return;
        }

        if (index >= 0 && index < _weaponStanceData.Count)
        {
            if(_currentWeaponStance != null)
                _currentWeaponStance.OnWeaponStanceRemovedChannel.RaiseEvent();
            _currentWeaponStance = _weaponStanceData[index];
            _currentWeaponStance.OnWeaponStanceEquippedChannel.RaiseEvent();
        }
        else
        {
            Debug.LogWarning("Index out of range for weapon stance data.");
        }
    }

    [Button("ParryTest")]
    private void Parry()
    {
        if (!_canAttack || _isAttacking || _isParrying) return;
        _isParrying = true;
        GetComponentInChildren<Animator>().SetTrigger("Parry");
        StartCoroutine(ParryRoutine());
    }
    

    private IEnumerator ParryRoutine()
    {
        yield return new WaitForEndOfFrame();
        var timer = 0f;
        var manager = GetComponentInChildren<HurtboxManager>();
        manager.SetAllHurtboxMode(HurtboxMode.PARRY);
        while (timer <= _parryWindow)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        manager.SetAllHurtboxMode(HurtboxMode.NORMAL);
        _isParrying = false;
    }
    

    private bool CheckForSpecialAttack(out SpecialAttackData specialFound)
    {
        //Comparison local method to check if the buffered actions match any sequences of a special attack at any point.
        bool CompareSequence(SpecialAttackData specialAttack)
        {
            var hasSequence = false;
            for (int i = 0; i <= _playerActions.Count - specialAttack.Sequence.Length; i++)
            {
                var match = !specialAttack.Sequence.Where((t, j) => _playerActions[i + j].Type != t).Any();
                if (!match) continue;
                hasSequence = true;
                break;
            }
            return hasSequence;
        }
        
        List<SpecialAttackData> possibleAttacks = new List<SpecialAttackData>();
        specialFound = null;
        if (_currentWeaponStance.SpecialAttackDataSet == null)
        {
            Debug.LogWarning("No special attack data set found for current weapon stance.");
            return false;
        }
        
        //Search through all special attacks and add any that are possible to a list based on number of inputs buffered.
        foreach (var attackData in _currentWeaponStance.SpecialAttackDataSet.GetAttackDataList())
        {
            var specialAttack = (SpecialAttackData)attackData;
            if(_playerActions.Count < specialAttack.Sequence.Length)
                continue;
            possibleAttacks.Add(specialAttack);
        }
        
        if (_debugLog)
        {
            var logNames = "";
            foreach (var attack in possibleAttacks)
            {
                logNames += attack.Name + ", ";
            }
            Debug.Log("Possible Specials: " + possibleAttacks.Count + $"\n {logNames}");
        }
        
        //No special attacks are possible, return false.
        if(possibleAttacks.Count == 0)
            return false;
        
        //Checks each possible attack.
        foreach (var specialAttack in possibleAttacks)
        {
            if (CompareSequence(specialAttack))
            {
                specialFound = specialAttack;
                break;
            }
        }
        return specialFound ? true : false;
    }

    private void PerformSpecialAttack(SpecialAttackData data)
    {
        if (currentAttack != null)
        {
            //TODO: Consider to buffer special attacks here for leniency if the inputs are early.
            //If the current attack is not in recovery frames, return.
            if (!currentAttack.InRecovery) return;
            currentAttack.Cancel();
        }
        //Execute special attack.
        var attack = _attackMap[data.Name.GetHashCode()];
        ExecuteAttack(attack);
    }
    
    private void PerformNormalAttack(AttackType inputAttackType)
    {
        Attack attack = null;
        int attackHash = 0;
        //If not already attacking, find the first attack in the gatling chain.
        if (currentAttack == null)
        {
            //Load the first attack in the gatling chain based on attack type.
            switch (inputAttackType)
            {
                case AttackType.Light:
                    if (_currentWeaponStance.LightAttackDataSet == null || _currentWeaponStance.LightAttackDataSet.GetAttackDataList().Count == 0)
                    {
                        Debug.LogError("Light attacks not set or is empty.");
                        break;
                    }
                    attackHash = _currentWeaponStance.LightAttackDataSet.GetAttackDataList()[0].Name.GetHashCode();
                    _attackMap.TryGetValue(attackHash, out attack);
                    break;
                case AttackType.Heavy:
                    if (_currentWeaponStance.HeavyAttackDataSet == null || _currentWeaponStance.HeavyAttackDataSet.GetAttackDataList().Count == 0)
                    {
                        Debug.LogError("Heavy attacks not set or is empty.");
                        break;
                    }
                    attackHash = _currentWeaponStance.HeavyAttackDataSet.GetAttackDataList()[0].Name.GetHashCode();
                    _attackMap.TryGetValue(attackHash, out attack);
                    break;
                default:
                    attack = null;
                    Debug.LogError($"Attack type not found. {inputAttackType}");
                    break;
            }
        } 
        //If already performing an attack, check and find for the next attack in the gatling chain.
        else if (currentAttack != null)
        {

            //Only chain when in recovery frames.
            if (!currentAttack.InRecovery && !currentAttack.GatlingOnlyOnHit)
            {
                return;
            }

            //Special case for attacks that can only be gatlinged on hit.
            if (currentAttack.GatlingOnlyOnHit)
            {
                if (!currentAttack.HadContact)
                    return;
            }
            
            //Find the next attack in the gatling chain in the current attack that matches corresponding attack type. 
            var attackData = currentAttack.Gatlings.FirstOrDefault(x => x.Type == inputAttackType);
            //Check if the gatling attack is valid with the current weapon stance.
            switch (inputAttackType)
            {
                case AttackType.Heavy:
                    if (!_currentWeaponStance.HeavyAttackDataSet.GetAttackDataList().Contains(attackData)) return;
                    break;
                case AttackType.Light:
                    if (!_currentWeaponStance.LightAttackDataSet.GetAttackDataList().Contains(attackData)) return;
                    break;
                default:
                    Debug.LogWarning("Attack type not found.");
                    break;
            }
            
            if(attackData == null) return;
            attackHash = attackData.Name.GetHashCode();
            _attackMap.TryGetValue(attackHash, out attack);
            currentAttack.Cancel();
            if(_debugLog)
                Debug.Log("Attack canceled.");
        }

        if (attack == null){ return;}
        // Execute the attack and play the animation.
        ExecuteAttack(attack);
    }
    
    private void ExecuteAttack(Attack attack)
    {
        OnAttackStarted?.Invoke();
        attack.Execute();
        if(_weaponFX != null)
            _weaponFX.Play();
        _playerEventChannel?.RaisePlayerAttacked(CreateAttackContext(attack));
        currentAttack = attack;
        if(_attackRoutine != null)
            StopCoroutine(_attackRoutine);
        _attackRoutine = StartCoroutine(AttackFrameUpdateRoutine(attack));
    }
    
    private AttackEventContext CreateAttackContext(Attack attack)
    {
        AttackEventContext ctx = new AttackEventContext
        {
            Attack = attack,
            AnimationStateName = attack.GetAnimationStateName(),
            Direction = Vector2.zero //TODO: Implement direction. Not sure if this is even needed.
        };
        return ctx;
    }

    //Frame by frame coroutine to update attack status.
    private IEnumerator AttackFrameUpdateRoutine(Attack attack)
    {
        _isAttacking = true;
        while (attack.IsActive)
        {
            attack.Tick();
            yield return new WaitForEndOfFrame();
        }
        currentAttack = null;
        _isAttacking = false;
        if(_weaponFX != null)
            _weaponFX.Stop();
        _playerEventChannel.RaisePlayerAttackEnded();
    }

    #region PLAYER EVENT CALLBACKS
    private void OnHitStunStarted()
    {
        _canAttack = false;
        CancelAttack();
    }
    
    private void OnHitStunEnded()
    {
        _canAttack = true;
    }
    #endregion
    
    
    #region INPUT CALLBACKS
    
    private void OnWeaponSpecialActionPressed()
    {
       if(_currentWeaponStance == null) return;
       switch (_currentWeaponStance.SpecialType)
       {
           case WeaponSpecialType.Attack:
               Debug.Log("Not implemented.");
               break;
           case WeaponSpecialType.Parry:
               Parry();
               break;
           case WeaponSpecialType.None:
               break;
           default:
               Debug.LogError("Invalid weapon special type.");
               break;
       }
    }
    
    private void OnWeaponQuickSwitchPressed()
    {
        if(_runtimePlayerData.CanWeaponSwitch == false) return;
        if(_currentWeaponStance == null) return;
        //If currently attacking and not in recovery frames, return.
        if (_isAttacking) return;

        //Quick and dirty way to switch weapon stances as there's only planned to be 2 weapon stances for GameJam.
        if(_currentWeaponStance == _weaponStanceData[0])
            ChangeWeaponStance(1);
        else
            ChangeWeaponStance(0);
    }
    
    private void OnLightAttackPressed()
    {
        if (_currentWeaponStance == null)
        {
            Debug.Log("No weapon stance equipped.");
            return;
        }
        _playerActions.Add(new PlayerAction(PlayerAction.ActionType.LightAttack, Time.time));
        if(!_canAttack || _isParrying) return;
        
        if (CheckForSpecialAttack(out var specialAttack))
        {
            if(_debugLog)
                Debug.Log($"Triggered Special Attack: {specialAttack.Name}");
            PerformSpecialAttack(specialAttack);
            return;
        }
        PerformNormalAttack(AttackType.Light);
    }
    
    private void OnHeavyAttackPressed()
    {
        if (_currentWeaponStance == null)
        {
            Debug.Log("No weapon stance equipped.");
            return;
        }
        _playerActions.Add(new PlayerAction(PlayerAction.ActionType.HeavyAttack, Time.time));
        if(!_canAttack || _isParrying) return;
        
        if (CheckForSpecialAttack(out var specialAttack))
        {
            if(_debugLog)
                Debug.Log($"Triggered Special Attack: {specialAttack.Name}");
            PerformSpecialAttack(specialAttack);
            return;
        }
        PerformNormalAttack(AttackType.Heavy);
    }
    
    private void OnMovePressed(Vector2 value)
    {
        PlayerAction.ActionType movementType;
        if (_playerBaseController.FacingDirection == FacingDirection.Forward) //If the player is facing right, use the input as is.
        {
            if (_directionMap.TryGetValue(value, out movementType))
            {
                _playerActions.Add(new PlayerAction(movementType, Time.time));
            }
            
        }
        else if (_playerBaseController.FacingDirection == FacingDirection.Back) //Invert the direction if the player is facing left.
        {
            var inverseDir = value;
            inverseDir.x *= -1;
            if (_directionMap.TryGetValue(inverseDir, out movementType))
            {
                _playerActions.Add(new PlayerAction(movementType, Time.time));
            }
        }
        
    }
    #endregion

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
        var attack = _attackMap[_debugAttackName.GetHashCode()];
        ExecuteAttack(attack);
    }
    
    protected virtual IEnumerable GetAttackNames()
    {
        var attackNames = new List<string>();
        if (_currentWeaponStance != null)
        {
            foreach (var attack in _currentWeaponStance.LightAttackDataSet.GetAttackDataList())
            {
                attackNames.Add(attack.Name);
            }
            foreach (var attack in _currentWeaponStance.HeavyAttackDataSet.GetAttackDataList())
            {
                attackNames.Add(attack.Name);
            }
            foreach (var attack in _currentWeaponStance.SpecialAttackDataSet.GetAttackDataList())
            {
                attackNames.Add(attack.Name);
            }
        }
        return attackNames;
    }
#endif
    #endregion
}
