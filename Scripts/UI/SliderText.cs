using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class SliderText : MonoBehaviour
{
    [Required]
    [SerializeField] private TMPro.TextMeshProUGUI _text;
    [Required]
    [SerializeField] private Slider _slider;
    [SerializeField] private string _format = "{0}";
    
    private void OnEnable()
    {
        _slider.onValueChanged.AddListener(UpdateText);
    }

    private void OnDisable()
    {
        _slider.onValueChanged.RemoveListener(UpdateText);
    }

    /// <summary>
    /// Updates the text to match the given value when dragging the slider.
    /// </summary>
    /// <param name="value"></param>
    private void UpdateText(float value)
    {
        var roundedValue = Mathf.RoundToInt(value * 100f);
        _text.text = string.Format(_format, roundedValue);
    }
    
    /// <summary>
    /// Update the slider and text to match the given value.
    /// </summary>
    /// <param name="value"></param>
    public void UpdateVisuals(float value)
    {
        _slider.value = value;
        UpdateText(value);
    }
}
