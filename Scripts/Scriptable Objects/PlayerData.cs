using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Windows;

/// <summary>
/// Runtime information about the player. There should only be ONE instance of this asset in the project.
/// </summary>
/// 
//[CreateAssetMenu(fileName = "New Player Data", menuName = "Player/Player Data", order = 2)]
public class PlayerData : SerializedScriptableObject
{
    [Title("Scene Data")]
    public string CurrentSceneName;

    [Title("Saved Position Data")]
    public string LastSavedSceneName;
    public string SavedSplineName;
    public Vector3 SavedPosition;
    
    [Title("Last Checkpoint")]
    public string LastCheckpointSceneName;
    public string LastCheckpointSplineName;
    public Vector3 LastCheckpointPosition;
    
    [Title("Base Stats")]
    [SerializeField] private float _maxHealth;
    public float MaxHealth => _maxHealth;
    [SerializeField] private float _maxEx;
    public float MaxEx => _maxEx;
    [SerializeField] private float _maxStamina;
    public float MaxStamina => _maxStamina;
    [SerializeField] private float _staminaRegenRate;
    public float StaminaRegenRate => _staminaRegenRate;

    [Title("Current Stats")]
    [ProgressBar(0,"MaxHealth",1f,0.278f,0.341f)]
    [SerializeField] private float _currentHealth;
    public float CurrentHealth
    {
        get => _currentHealth;
        set
        {
            _currentHealth = Mathf.Clamp(value, 0, MaxHealth);
            HealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }
    }

    [ProgressBar(0, "@MaxEx", 0.18f, 0.835f, 0.451f)]
    [SerializeField] private float _currentEx;
    public float CurrentEx
    {
        get => _currentEx;
        set
        {
            _currentEx = value;
            ExChanged?.Invoke(CurrentEx, MaxEx);
        }
    }
    [ProgressBar(0,"@MaxStamina", 0.18f,0.835f,0.451f)]
    [SerializeField] private float _currentStamina;
    public float CurrentStamina
    {
        get => _currentStamina;
        set
        {
            _currentStamina = value;
            StaminaChanged?.Invoke(CurrentStamina, MaxStamina);
        }
    }

    [Title("Base Cost")]
    [LabelText("Stamina Cost for Roll")]
    [SerializeField] private float _rollCost;
    public float RollCost => _rollCost;

    [Title("Flags")] public bool CanWeaponSwitch = false;

    [Title("Inventory Data")]
    [InfoBox("DO NOT add/remove items directly from this list or dictionary. Use the AddItem and RemoveItem methods instead under Debug.", InfoMessageType.Warning)]
    [NonSerialized, OdinSerialize]
    public Inventory Inventory = new Inventory();
    
    public delegate void HealthChangedEventHandler(float health, float maxHealth);
    public delegate void ExChangedEventHandler(float ex, float maxEx);
    public delegate void StaminaChangedEventHandler(float stamina, float maxStamina);
    
    public event HealthChangedEventHandler HealthChanged;
    public event ExChangedEventHandler ExChanged;
    public event StaminaChangedEventHandler StaminaChanged;

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            HealthChanged?.Invoke(CurrentHealth,MaxHealth);
            ExChanged?.Invoke(CurrentEx,MaxEx);
            StaminaChanged?.Invoke(CurrentStamina,MaxStamina);
        }
    }

    /// <summary>
    /// Returns a wrapper object containing all the data that needs to be saved.
    /// </summary>
    /// <returns></returns>
    public PlayerSaveWrapper CreateSaveData()
    {
        PlayerSaveWrapper wrapper = new PlayerSaveWrapper();
        wrapper.LastSavedSceneName = LastSavedSceneName;
        wrapper.SavedSplineName = SavedSplineName;
        wrapper.SavedPosition = SavedPosition;
        wrapper.MaxHealth = MaxHealth;
        wrapper.MaxEx = MaxEx;
        wrapper.MaxStamina = MaxStamina;
        wrapper.CurrentHealth = CurrentHealth;
        wrapper.CurrentEx = CurrentEx;
        wrapper.CurrentStamina = CurrentStamina;
        foreach(var item in Inventory.Items)
        {
            wrapper.ItemsInInventory.Add(item.DataReference);
            wrapper.ItemStacks.Add(item.CurrentStack);
        }
        foreach (var item in Inventory.QuickSlots)
        {
            if(item == null)
                wrapper.ItemsInQuickSlot.Add(null);
            else
                wrapper.ItemsInQuickSlot.Add(item.DataReference);
        }
        return wrapper;
    }

    /// <summary>
    /// Loads the data from the wrapper object into the player data.
    /// </summary>
    /// <param name="wrapper"></param>
    public void LoadSaveData(PlayerSaveWrapper wrapper)
    {
        LastSavedSceneName = wrapper.LastSavedSceneName;
        SavedSplineName = wrapper.SavedSplineName;
        SavedPosition = wrapper.SavedPosition;
        _maxHealth = wrapper.MaxHealth;
        _maxEx = wrapper.MaxEx;
        _maxStamina = wrapper.MaxStamina;
        _currentHealth = wrapper.CurrentHealth;
        _currentEx = wrapper.CurrentEx;
        _currentStamina = wrapper.CurrentStamina;
        //Recreate the inventory
        Inventory = new Inventory();
        for (int i = 0; i < wrapper.ItemsInInventory.Count; i++)
        {
            Inventory.AddItem(wrapper.ItemsInInventory[i], wrapper.ItemStacks[i]);
        }
        //Assign the quick slots
        for(int i = 0; i < wrapper.ItemsInQuickSlot.Count; i++)
        {
            if (wrapper.ItemsInQuickSlot[i] == null) continue; //Slot is empty.
            var item = Inventory.GetItem(wrapper.ItemsInQuickSlot[i]);
            Inventory.SetQuickSlot(i, item);
        }
    }
}

public enum PositionType
{
    SavePosition,
    CheckpointPosition
}
