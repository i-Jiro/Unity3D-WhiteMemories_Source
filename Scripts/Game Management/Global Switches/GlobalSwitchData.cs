using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "GlobalSwitchData", menuName = "Global Switch Data")]
public class GlobalSwitchData : ScriptableObject, ISerializeReferenceByCustomGuid
{
    [SerializeField] private string _guid;
    public string Guid => _guid;

    [SerializeField] private bool _isActive = false;
    public bool IsActive
    {
        get => _isActive;
        set
        {
            _isActive = value;
            OnSwitchChanged?.Invoke(_isActive);
        }
    }

    /// <summary>
    /// Event that is invoked when the switch is changed.
    /// </summary>
    public delegate void SwitchChanged(bool isActive);
    public event SwitchChanged OnSwitchChanged;

    private void OnValidate()
    {
        if(Application.isPlaying)
            OnSwitchChanged?.Invoke(_isActive);
    }
    
    public void SetSwitchState(bool isActive)
    {
        IsActive = isActive;
    }
}
