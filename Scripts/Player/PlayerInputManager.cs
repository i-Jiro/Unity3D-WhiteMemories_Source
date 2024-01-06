using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles all player input and passes it to the player controller or any subscribers using Unity's new input system.
/// Can be easily replaced with a different input system if needed.
/// </summary>

[DefaultExecutionOrder(-1)]
public class PlayerInputManager : MonoBehaviour
{
    public static PlayerInputManager Instance;
    
    [Header("Debug")]
    public bool debugLog = false;
    [SerializeField] private bool _worldControlOnStart;
    [SerializeField] private bool _menuControlOnStart;
    
    private PlayerInputAction _playerInputAction;

    //Events for player controller and UI to hook into.
    #region EVENT
    public delegate void MovementEventHandler(Vector2 value);
    public event MovementEventHandler MovePressed;
    
    public delegate void MovementReleasedEventHandler();
    public event MovementReleasedEventHandler MoveCanceled;
    
    public delegate void JumpEventHandler();
    public event JumpEventHandler JumpPressed;
    
    public delegate void JumpReleasedEventHandler();
    public event JumpReleasedEventHandler JumpReleased;
    
    public delegate void LightAttackEventHandler();
    public event LightAttackEventHandler LightAttackPressed;
    
    public delegate void LightAttackReleasedEventHandler();
    public event LightAttackReleasedEventHandler LightAttackReleased;

    public delegate void HeavyAttackEventHandler();
    public event HeavyAttackEventHandler HeavyAttackPressed;
    
    public delegate void HeavyAttackReleasedEventHandler();
    public event HeavyAttackReleasedEventHandler HeavyAttackReleased;
    
    public delegate void InteractPressedEventHandler();
    public event InteractPressedEventHandler InteractPressed;
    
    public delegate void InteractReleasedEventHandler();
    public event InteractReleasedEventHandler InteractReleased;
    
    public delegate void DodgePressedEventHandler();
    public event DodgePressedEventHandler DodgePressed;
    
    public delegate void DodgeReleasedEventHandler();
    public event DodgeReleasedEventHandler DodgeReleased;
    
    public delegate void FocusPressedEventHandler();
    public event FocusPressedEventHandler FocusPressed;
    
    public delegate void FocusReleasedEventHandler();
    public event FocusReleasedEventHandler FocusReleased;
    public delegate void ConfirmPressedEventHandler();
    public event ConfirmPressedEventHandler ConfirmPressed;
    
    public delegate void ConfirmReleasedEventHandler();
    public event ConfirmReleasedEventHandler ConfirmReleased;
    
    public delegate void CancelPressedEventHandler();
    public event CancelPressedEventHandler CancelPressed;
    
    public delegate void CancelReleasedEventHandler();
    public event CancelReleasedEventHandler CancelReleased;
    
    public delegate void NextTabPressedEventHandler();
    public event NextTabPressedEventHandler NextTabPressed;
    
    public delegate void PreviousTabPressedEventHandler();
    public event PreviousTabPressedEventHandler PreviousTabPressed;

    public delegate void QuickSlotTriggerPressedEventHandler();
    public event QuickSlotTriggerPressedEventHandler QuickSlotTriggerPressed;
    public delegate void QuickSlotTriggerReleasedEventHandler();
    public event QuickSlotTriggerReleasedEventHandler QuickSlotTriggerReleased;
    
    public delegate void SettingsPressedEventHandler();
    public event SettingsPressedEventHandler SettingsPressed;
    public delegate void SettingsReleasedEventHandler();
    public event SettingsReleasedEventHandler SettingsReleased;
    
    public delegate void StartMenuPressedEventHandler();
    public event StartMenuPressedEventHandler StartMenuPressed;
    
    public delegate void WeaponSwapPressedEventHandler();
    public event WeaponSwapPressedEventHandler WeaponQuickSwitchPressed;
    
    public delegate void WeaponSpecialActionPressedEventHandler();
    public event WeaponSpecialActionPressedEventHandler WeaponSpecialActionPressed;
    
    public delegate void QuickSlot1PressedEventHandler();
    public event QuickSlot1PressedEventHandler QuickSlot1Pressed;
    
    public delegate void QuickSlot2PressedEventHandler();
    public event QuickSlot2PressedEventHandler QuickSlot2Pressed;


    #endregion
    
    private void Awake(){
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
        _playerInputAction = new PlayerInputAction();
        
        _playerInputAction.InWorld.Movement.performed += MovePress;
        _playerInputAction.InWorld.Movement.canceled += MoveRelease;
        _playerInputAction.InWorld.Jump.performed += JumpPress;
        _playerInputAction.InWorld.Jump.canceled += JumpRelease;
        _playerInputAction.InWorld.LightAttack.performed += LightAttackPress;
        _playerInputAction.InWorld.LightAttack.canceled += LightAttackRelease;
        _playerInputAction.InWorld.HeavyAttack.performed += HeavyAttackPress;
        _playerInputAction.InWorld.HeavyAttack.canceled += HeavyAttackRelease;
        _playerInputAction.InWorld.Interact.performed += InteractPress;
        _playerInputAction.InWorld.Interact.canceled += InteractRelease;
        _playerInputAction.InWorld.Dodge.performed += DodgePress;
        _playerInputAction.InWorld.Dodge.canceled += DodgeRelease;
        _playerInputAction.InWorld.Focus.performed += FocusPress;
        _playerInputAction.InWorld.Focus.canceled += FocusRelease;
        _playerInputAction.InWorld.QuickSlotTrigger.performed += QuickSlotTriggerPress;
        _playerInputAction.InWorld.QuickSlotTrigger.canceled += QuickSlotTriggerRelease;
        _playerInputAction.InWorld.Settings.performed += SettingsPress;
        _playerInputAction.InWorld.Settings.canceled += SettingsRelease;
        _playerInputAction.InWorld.StartMenu.performed += StartMenuPress;
        _playerInputAction.InWorld.WeaponQuickSwitch.performed += WeaponQuickSwitchPress;
        _playerInputAction.InWorld.WeaponSpecialAction.performed += WeaponSpecialActionPress;
        _playerInputAction.InWorld.QuickSlot1.performed += QuickSlot1Press;
        _playerInputAction.InWorld.QuickSlot2.performed += QuickSlot2Press;


        _playerInputAction.InMenu.Confirm.performed += ConfirmPress;
        _playerInputAction.InMenu.Confirm.canceled += ConfirmRelease;
        _playerInputAction.InMenu.Cancel.performed += CancelPress;
        _playerInputAction.InMenu.Cancel.canceled += CancelRelease;
        _playerInputAction.InMenu.NextTab.performed += NextTabPress;
        _playerInputAction.InMenu.PreviousTab.performed += PreviousTabPress;
    }
    

    private void Start()
    {
        SetInWorldControlState(_worldControlOnStart);
        SetInMenuWorldControlState(_menuControlOnStart);
    }

    #region Input Methods
    private void WeaponSpecialActionPress(InputAction.CallbackContext ctx)
    {
        WeaponSpecialActionPressed?.Invoke();
        if(debugLog)
            Debug.Log("Weapon special action pressed.\n" + $"Time: {ctx.time}");
    }

    private void QuickSlot1Press(InputAction.CallbackContext ctx)
    {
        QuickSlot1Pressed?.Invoke();
    }
    
    private void QuickSlot2Press(InputAction.CallbackContext ctx)
    {
        QuickSlot2Pressed?.Invoke();
    }

    private void WeaponQuickSwitchPress(InputAction.CallbackContext ctx)
    {
        WeaponQuickSwitchPressed?.Invoke();
        if(debugLog)
            Debug.Log("Weapon quick switch pressed.\n" + $"Time: {ctx.time}");
    }
    private void StartMenuPress(InputAction.CallbackContext ctx)
    {
        StartMenuPressed?.Invoke();
        if(debugLog)
            Debug.Log("Start menu pressed.\n" + $"Time: {ctx.time}");
    }

    private void SettingsPress(InputAction.CallbackContext ctx)
    {
        SettingsPressed?.Invoke();
        if(debugLog)
            Debug.Log("Quick slot trigger pressed.\n" + $"Time: {ctx.time}");
    }
    
    private void SettingsRelease(InputAction.CallbackContext ctx)
    {
        SettingsReleased?.Invoke();
        if(debugLog)
            Debug.Log("Quick slot trigger released.\n" + $"Time: {ctx.time}");
    }
    
    private void QuickSlotTriggerPress(InputAction.CallbackContext ctx)
    {
        QuickSlotTriggerPressed?.Invoke();
        if(debugLog)
            Debug.Log("Quick slot trigger pressed.\n" + $"Time: {ctx.time}");
    }
    
    private void QuickSlotTriggerRelease(InputAction.CallbackContext ctx)
    {
        QuickSlotTriggerReleased?.Invoke();
        if(debugLog)
            Debug.Log("Quick slot trigger released.\n" + $"Time: {ctx.time}");
    }
    
    private void NextTabPress(InputAction.CallbackContext obj)
    {
        NextTabPressed?.Invoke();
        if(debugLog)
            Debug.Log("Next tab pressed.\n" + $"Time: {obj.time}");
    }
    
    private void PreviousTabPress(InputAction.CallbackContext obj)
    {
        PreviousTabPressed?.Invoke();
        if(debugLog)
            Debug.Log("Previous tab pressed.\n" + $"Time: {obj.time}");
    }
    
    private void ConfirmPress(InputAction.CallbackContext obj)
    {
        ConfirmPressed?.Invoke();
        if(debugLog)
            Debug.Log("Confirm pressed.\n" + $"Time: {obj.time}");
    }
    
    private void ConfirmRelease(InputAction.CallbackContext obj)
    {
        ConfirmReleased?.Invoke();
        if(debugLog)
            Debug.Log("Confirm released.\n" + $"Time: {obj.time}");
    }
    
    private void CancelPress(InputAction.CallbackContext obj)
    {
        CancelPressed?.Invoke();
        if(debugLog)
            Debug.Log("Cancel pressed.\n" + $"Time: {obj.time}");
    }
    
    private void CancelRelease(InputAction.CallbackContext obj)
    {
        CancelReleased?.Invoke();
        if(debugLog)
            Debug.Log("Cancel released.\n" + $"Time: {obj.time}");
    }
    
    private void FocusPress(InputAction.CallbackContext ctx)
    {
        FocusPressed?.Invoke();
        if(debugLog)
            Debug.Log("Focus pressed.\n" + $"Time: {ctx.time}");
    }
    
    private void FocusRelease(InputAction.CallbackContext ctx)
    {
        FocusReleased?.Invoke();
        if(debugLog)
            Debug.Log("Focus released.\n" + $"Time: {ctx.time}");
    }

    
    private void InteractPress(InputAction.CallbackContext ctx)
    {
        InteractPressed?.Invoke();
        if(debugLog)
            Debug.Log("Interact pressed.\n" + $"Time: {ctx.time}");
    }
    
    private void InteractRelease(InputAction.CallbackContext ctx)
    {
        if(debugLog)
            Debug.Log("Interact released.\n" + $"Time: {ctx.time}");
    }
    
    //Context expected is Vector2
    private void MovePress(InputAction.CallbackContext context)
    {
        Vector2 direction = context.ReadValue<Vector2>();
        MovePressed?.Invoke(direction);
        if(debugLog)
            Debug.Log("Move pressed.\n" + $"Input Direction: {direction}\n" + $"Time: {context.time}");
    }

    //Called when movement is canceled by either by letting go or pressing another movement key.
    private void MoveRelease(InputAction.CallbackContext context)
    {
        MoveCanceled?.Invoke();
        if(debugLog)
            Debug.Log("Move released.\n" + $"Time: {context.time}");
    }
    
    private void JumpPress(InputAction.CallbackContext context)
    {
        JumpPressed?.Invoke();
        if(debugLog)
            Debug.Log("Jump pressed.\n" + $"Time: {context.time}");
    }

    private void JumpRelease(InputAction.CallbackContext context)
    {
        JumpReleased?.Invoke();
        if(debugLog)
            Debug.Log("Jump released.\n" + $"Time: {context.time}");
    }
    
    private void LightAttackPress(InputAction.CallbackContext context)
    {
        LightAttackPressed?.Invoke();
        if (debugLog)
            Debug.Log("Light Attack pressed.\n" + $"Time: {context.time}");
    }
    
    private void LightAttackRelease(InputAction.CallbackContext context)
    {
        LightAttackReleased?.Invoke();
        if (debugLog)
            Debug.Log("Light Attack released.\n" + $"Time: {context.time}");
    }
    
    private void HeavyAttackPress(InputAction.CallbackContext context)
    {
        HeavyAttackPressed?.Invoke();
        if (debugLog)
            Debug.Log("Heavy Attack press.\n" + $"Time: {context.time}");
    }
    
    private void HeavyAttackRelease(InputAction.CallbackContext context)
    {
        HeavyAttackReleased?.Invoke();
        if (debugLog)
            Debug.Log("Heavy Attack released.\n" + $"Time: {context.time}");
    }
    
    private void DodgePress(InputAction.CallbackContext context)
    {
        DodgePressed?.Invoke();
        if (debugLog)
            Debug.Log("Dodge pressed.\n" + $"Time: {context.time}");
    }
    
    private void DodgeRelease(InputAction.CallbackContext context)
    {
        DodgeReleased?.Invoke();
        if (debugLog)
            Debug.Log("Dodge released.\n" + $"Time: {context.time}");
    }

    
    #endregion
    // End of Input Methods

    //Enable/Disable input actions for in-game actions.
    public void SetInWorldControlState(bool value)
    {
        switch (value)
        {
            case true:
                _playerInputAction.InWorld.Enable();
                break;
            case false:
                _playerInputAction.InWorld.Disable();
                break;
        }
    }

    //Enable/Disable input actions for in-menu actions.
    public void SetInMenuWorldControlState(bool value)
    {
        switch (value)
        {
            case true:
                _playerInputAction.InMenu.Enable();
                break;
            case false:
                _playerInputAction.InMenu.Disable();
                break;
        }
    }
    
    

    private void OnDestroy()
    {
        _playerInputAction.InWorld.Movement.performed -= MovePress;
        _playerInputAction.InWorld.Movement.canceled -= MoveRelease;
        _playerInputAction.InWorld.Jump.performed -= JumpPress;
        _playerInputAction.InWorld.Jump.canceled -= JumpRelease;
        _playerInputAction.InWorld.LightAttack.performed -= LightAttackPress;
        _playerInputAction.InWorld.LightAttack.canceled -= LightAttackRelease;
        _playerInputAction.InWorld.HeavyAttack.performed -= HeavyAttackPress;
        _playerInputAction.InWorld.HeavyAttack.canceled -= HeavyAttackRelease;
        _playerInputAction.InWorld.Dodge.performed -= DodgePress;
        _playerInputAction.InWorld.Dodge.canceled -= DodgeRelease;
        _playerInputAction.InWorld.Focus.performed -= FocusPress;
        _playerInputAction.InWorld.Focus.canceled -= FocusRelease;
        _playerInputAction.InWorld.Interact.performed -= InteractPress;
        _playerInputAction.InWorld.Interact.canceled -= InteractRelease;
        _playerInputAction.InWorld.QuickSlot1.performed -= QuickSlot1Press;
        _playerInputAction.InWorld.QuickSlot2.performed -= QuickSlot2Press;
    }
}
