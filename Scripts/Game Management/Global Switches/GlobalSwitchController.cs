using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Reacts to global switch changes.
/// </summary>
public class GlobalSwitchController : MonoBehaviour
{
    [SerializeField] private GlobalSwitchData _switchData;
    [InfoBox("Default state to react when the switch is off.")]
    public UnityEvent OnSwitchOffState;
    [InfoBox("State to react when the switch is on.")]
    public UnityEvent OnSwitchOnState;

    private void OnEnable()
    {
        _switchData.OnSwitchChanged += SwitchChanged;
        //Register the switch with the world manager if it is not already registered.
        //The switch now will be relevant to the world manager and will be saved and loaded with the game.
        if(WorldManager.Instance != null && !WorldManager.Instance.HasGlobalSwitch(_switchData))
            WorldManager.Instance.RegisterGlobalSwitch(_switchData);
    }
    
    private void OnDisable()
    {
        _switchData.OnSwitchChanged -= SwitchChanged;
    }
    
    /// <summary>
    /// Activate switch event triggers based on the current state of the switch data on scene start.
    /// </summary>
    private void Start()
    {
        if (_switchData == null || WorldManager.Instance == null)
        {
            Debug.LogError("Unable to respond to switch data. Either WorldManager was not found or SwitchData was not registered in component.");
            return;
        }
        // Check the current state of the switch and trigger the appropriate event. Will always return false if the switch is not registered with the world manager.
        var state = WorldManager.Instance.GetGlobalSwitchState(_switchData);
        InvokeSwitchEvent(state);
    }

    /// <summary>
    /// Event callback that is called when the switch is changed.
    /// This cover cases for switch changes that happens in the same scene.
    /// </summary>
    /// <param name="value">The state the switch has changed to.</param>
    private void SwitchChanged(bool value)
    {
        if (WorldManager.Instance == null || _switchData == null)
        {
            Debug.LogError("Unable to respond to switch data. Either WorldManager was not found or SwitchData was not registered in component.");
            return;
        }
        //Check if the switch is registered with the world manager before passing it's value.
        //Will always return false (Off-state) if the switch is not registered with the world manager.
        value = WorldManager.Instance.HasGlobalSwitch(_switchData) && value;
        InvokeSwitchEvent(value);
    }
    
    /// <summary>
    /// Invokes the appropriate event based on the switch state.
    /// </summary>
    /// <param name="value">State the switch is in.</param>
    private void InvokeSwitchEvent(bool value)
    {
        if (value)
        {
            OnSwitchOnState?.Invoke();
        }
        else
        {
            OnSwitchOffState?.Invoke();
        }
    }
}
