using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using URPGlitch.Runtime.AnalogGlitch;
using URPGlitch.Runtime.DigitalGlitch;

public class ScreenGlitchController : MonoBehaviour
{
    [SerializeField] private Volume _volume;
    [SerializeField] private float _glitchDuration = 0.1f;
    [Range(0f,1f)]
    [SerializeField] private float _scanlineStrength = 0.5f;
    [Range(0f,1f)]
    [SerializeField] private float _colorDriftStrength = 0.5f;
    [Header("Event Channel To Listen For")]
    [SerializeField] private VoidEventChannel _eventChannel;
    private Tween _scanlineTween;
    private Tween _colorDriftTween;
    
    private void OnEnable()
    {
        if(_eventChannel != null)
            _eventChannel.OnEventRaised += Glitch;
    }
    
    private void OnDisable()
    {
        if(_eventChannel != null)
            _eventChannel.OnEventRaised -= Glitch;
    }

    private void Awake()
    {
        if (_volume == null)
        {
            _volume = GetComponent<Volume>();
        }
    }

    public void Glitch()
    {
        //Checks if damaged screen effect is disabled in settings.
        if(SettingsManager.Instance != null && !SettingsManager.Instance.ScreenDamageFXEnabled) return;
        
        if (!_volume.profile.TryGet<AnalogGlitchVolume>(out var glitchVolume)) return;
        
        if(_scanlineTween != null && _scanlineTween.IsActive())
            _scanlineTween.Kill();
        if(_colorDriftTween != null && _colorDriftTween.IsActive())
            _colorDriftTween.Kill();
        
        _scanlineTween =
            DOVirtual.Float(0,_scanlineStrength, _glitchDuration, (value)=> glitchVolume.scanLineJitter.value = value)
                .SetUpdate(true) // Unscaled time
                .OnComplete(()=> glitchVolume.scanLineJitter.value = 0);
        _colorDriftTween =
            DOVirtual.Float(0f, _colorDriftStrength, _glitchDuration, (value)=> glitchVolume.colorDrift.value = value)
                .SetUpdate(true)
                .OnComplete(()=> glitchVolume.colorDrift.value = 0);
    }
}
