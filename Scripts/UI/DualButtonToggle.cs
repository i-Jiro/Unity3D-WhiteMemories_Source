using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DualButtonToggle : MonoBehaviour
{
    [SerializeField] private Button _enableButton;
    [SerializeField] private Button _disableButton;
    [SerializeField] private Sprite _enabledSprite;
    [SerializeField] private Sprite _disabledSprite;

    public UnityEvent<bool> OnValueChanged;
    
    private void OnEnable()
    {
        _enableButton.onClick.AddListener(OnEnablePressed);
        _disableButton.onClick.AddListener(OnDisablePressed);
    }
    
    private void OnDisable()
    {
        _enableButton.onClick.RemoveListener(OnEnablePressed);
        _disableButton.onClick.RemoveListener(OnDisablePressed);
    }

    private void OnEnablePressed()
    {
        UpdateVisuals(true);
        OnValueChanged?.Invoke(true);
    }
    
    private void OnDisablePressed()
    {
        UpdateVisuals(false);
        OnValueChanged?.Invoke(false);
    }
    
    /// <summary>
    /// Set the visual state of the buttons.
    /// </summary>
    /// <param name="state">If true, enable button is highlighted. If false, disable is highlighted.</param>
    public void UpdateVisuals(bool state)
    {
        if (state)
        {
            _enableButton.image.sprite = _enabledSprite;
            _disableButton.image.sprite = _disabledSprite;
        }
        else
        {
            _enableButton.image.sprite = _disabledSprite;
            _disableButton.image.sprite = _enabledSprite;
        }
    }
}
