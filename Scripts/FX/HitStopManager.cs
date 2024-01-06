using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core.Easing;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class HitStopManager : MonoBehaviour
{
    public static HitStopManager Instance;
    public float HitStopScale = 0.15f;
    public float RampTime = 0.0075f;
    [SerializeField] private Ease _rampEase = Ease.InOutSine;
    
    private float _timer;
    private bool _hitStopActive;
    private Coroutine _stopRoutine;
    private Tweener _rampUpTween;
    private Tweener _rampDownTween;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("HitStopManager already exists. Deleting this instance.");
            Destroy(this);
        }
    }

    public void HitStop(float duration)
    {
        //If hitstop is disabled in settings, don't do anything.
        if(SettingsManager.Instance != null && !SettingsManager.Instance.HitstopEnabled)
            return;
        
        _hitStopActive = true;
        if(_stopRoutine != null)
            StopCoroutine(_stopRoutine);
        _rampUpTween?.Kill();
        _rampDownTween?.Kill();
        Time.timeScale = 1f;
        _rampDownTween =
        DOVirtual.Float(1f, HitStopScale, RampTime, (x) => Time.timeScale = x)
            .SetEase(_rampEase)
            .OnComplete(() => _stopRoutine = StartCoroutine(StopRoutine(duration)))
            .SetUpdate(true);
    }

    private IEnumerator StopRoutine(float duration)
    {
        Time.timeScale = HitStopScale;
        _hitStopActive = true;
        _timer = 0f;
        while (_timer <= duration)
        {
            _timer += Time.unscaledDeltaTime;
            yield return new WaitForEndOfFrame();
        }
        _hitStopActive = false;
        RampUp();
    }

    private void RampUp()
    {
        _rampUpTween?.Kill();
        _rampUpTween =
        DOVirtual.Float(Time.timeScale, 1f, RampTime, (x) => Time.timeScale = x)
            .SetEase(_rampEase)
            .SetUpdate(true);
    }

    public bool IsHitStopActive()
    {
        return _hitStopActive;
    }
}
