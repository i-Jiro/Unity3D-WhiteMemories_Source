using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class PlayerMenuSideWindow : MonoBehaviour
{
    [SerializeField] private PlayerData _runtimePlayerData;
    [ChildGameObjectsOnly]
    [SerializeField] private UIMeterDelay _healthMeter;
    [ChildGameObjectsOnly]
    [SerializeField] private TextMeshProUGUI _healthText;
    [ChildGameObjectsOnly]
    [SerializeField] private UIMeterDelay _staminaMeter;
    [ChildGameObjectsOnly]
    [SerializeField] private TextMeshProUGUI _staminaText;
    [ChildGameObjectsOnly]
    [SerializeField] private UIMeterDelay _exMeter;
    [ChildGameObjectsOnly]
    [SerializeField] private TextMeshProUGUI _exText;
    
    //TODO: ADD EXP BAR

    private void OnEnable()
    {
        _runtimePlayerData.HealthChanged += UpdateHealthMeter;
        _runtimePlayerData.StaminaChanged += UpdateStaminaMeter;
        _runtimePlayerData.ExChanged += UpdateExMeter;
        UpdateUI(); //Update the UI when the menu activates.
    }
    
    private void OnDisable()
    {
        _runtimePlayerData.HealthChanged -= UpdateHealthMeter;
        _runtimePlayerData.StaminaChanged -= UpdateStaminaMeter;
        _runtimePlayerData.ExChanged -= UpdateExMeter;
    }

    private void UpdateUI()
    {
        UpdateHealthMeter(_runtimePlayerData.CurrentHealth, _runtimePlayerData.MaxHealth);
        UpdateStaminaMeter(_runtimePlayerData.CurrentStamina, _runtimePlayerData.MaxStamina);
        UpdateExMeter(_runtimePlayerData.CurrentEx, _runtimePlayerData.MaxEx);
    }
    
    private void UpdateHealthMeter(float currentHealth, float maxHealth)
    {
        _healthMeter.ChangeProgress(currentHealth/maxHealth);
        _healthText.text = $"{Mathf.RoundToInt(currentHealth)}/{Mathf.RoundToInt(maxHealth)}";
    }
    
    private void UpdateStaminaMeter(float currentStamina, float maxStamina)
    {
        var staminaPercent = currentStamina/maxStamina;
        _staminaMeter.ChangeProgress(staminaPercent);
        _staminaText.text = $"{Mathf.RoundToInt(staminaPercent*100f)}%";
    }
    
    private void UpdateExMeter(float currentEx, float maxEx)
    {
        var exPercent = currentEx/maxEx;
        _exMeter.ChangeProgress(exPercent);
        _exText.text = $"{Mathf.RoundToInt(exPercent*100f)}%";
    }
}
