using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusDisplayManager : MonoBehaviour
{
    public static StatusDisplayManager Instance;
    public Image CharacterPortrait;
    public GameObject StaminaWarning;
    [Header("Quick Slots")]
    public QuickSlotUI[] QuickSlots; 
    [Header("Meter Components")]
    [Required]
    public UIMeter HealthMeter;
    [Required]
    public UIMeter ExMeter;
    [Required]
    public UIMeter StaminaMeter;
    [Header("Text Components")]
    [Required]
    public TextMeshProUGUI HealthText;
    [Header("Player Data Reference")]
    [SerializeField] private PlayerData _playerData;

    private void OnEnable()
    {
        _playerData.HealthChanged += OnHealthChanged;
        _playerData.ExChanged += OnExChanged;
        _playerData.StaminaChanged += OnStaminaChanged;
        _playerData.Inventory.InventoryChanged += OnInventoryChanged;
    }

    private void OnDisable()
    {
        _playerData.HealthChanged -= OnHealthChanged;
        _playerData.ExChanged -= OnExChanged;
        _playerData.StaminaChanged -= OnStaminaChanged;
        _playerData.Inventory.InventoryChanged -= OnInventoryChanged;
    }
    

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("StatusDisplayManager already exists. Deleting this instance.");
            Destroy(transform.root.gameObject);
        }
        Hide(true);
    }
    
    public void OnInventoryChanged()
    {
        UpdateQuickSlots();
    }
    
    public void OnHealthChanged(float health, float maxHealth)
    {
        var percent = health / maxHealth;
        var healthFloor = Mathf.Floor(health);
        HealthMeter.ChangeProgress(percent);
        HealthText.text = $"{healthFloor}/{maxHealth}";
    }
    
    public void OnExChanged(float ex, float maxEx)
    {
        var percent = ex / maxEx;
        ExMeter.ChangeProgress(percent);
    }
    
    public void OnStaminaChanged(float stamina, float maxStamina)
    {
        var percent = stamina / maxStamina;
        StaminaMeter.ChangeProgress(percent);
        if (stamina <= _playerData.RollCost)
        {
            StaminaWarning.SetActive(true);
        }
        else
        {
            StaminaWarning.SetActive(false);
        }
    }

    /// <summary>
    /// Force the status display to update for a single tick.
    /// </summary>
    public void ForceUpdate()
    {
        OnHealthChanged(_playerData.CurrentHealth, _playerData.MaxHealth);
        OnExChanged(_playerData.CurrentEx, _playerData.MaxEx);
        OnStaminaChanged(_playerData.CurrentStamina, _playerData.MaxStamina);
        UpdateQuickSlots();
    }

    public void UpdateQuickSlots()
    {
        for (var i = 0; i < QuickSlots.Length; i++)
        {
            var item = _playerData.Inventory.QuickSlots[i];
            QuickSlots[i].UpdateUI(item);
        }
    }

    /// <summary>
    /// Set the visibility of the status display.
    /// </summary>
    /// <param name="state">True hides the UI, false is visible.</param>
    public void Hide(bool state)
    {
        HealthMeter.gameObject.SetActive(!state);
        ExMeter.gameObject.SetActive(!state);
        StaminaMeter.gameObject.SetActive(!state);
        HealthText.gameObject.SetActive(!state);
        CharacterPortrait.gameObject.SetActive(!state);
        foreach(var quickSlot in QuickSlots)
        {
            quickSlot.gameObject.SetActive(!state);
        }
    }
}
