using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIMeterDelay :UIMeter
{
    [SerializeField] protected Image _meterBackgroundImage;
    [SerializeField] protected Image _delayedMeterImage;
    [SerializeField] protected Color _recoverColor = new(0.69f, 0.97f, 0.65f);
    [SerializeField] protected Color _damageColor = new(0.98f, 0.46f, 0.46f);
    public float DelayHold = 0.5f;
    public float DelayDuration = 0.5f;
    private Tween _delayedMeterTween;
    [SerializeField] private bool _ignoreScaledTime = false;

    public override void ChangeProgress(float progress)
    {
        var oldProgress = _progress;
        var targetProgress = progress;

        if (_delayedMeterTween != null)
        {
            _delayedMeterTween.Kill();
        }
        
        if (oldProgress > targetProgress)
        {
            _delayedMeterImage.color = _damageColor;
            Decrease(oldProgress,targetProgress);
        }
        else
        {
            _delayedMeterImage.color = _recoverColor;
            Increase(oldProgress,targetProgress);
        }
        
        _progress = progress;
    }

    private void Decrease(float oldProgress, float targetProgress)
    {
        _delayedMeterImage.fillAmount = oldProgress;
        _meterImage.fillAmount = targetProgress;
        DOVirtual.DelayedCall(DelayHold, Tween)
            .SetUpdate(_ignoreScaledTime);

        void Tween()
        {
            _delayedMeterTween = DOVirtual.Float(oldProgress, targetProgress, DelayDuration, (value) =>
            {
                _delayedMeterImage.fillAmount = value;
            }).SetUpdate(_ignoreScaledTime);
        }
    }

    private void Increase(float oldProgress, float targetProgress)
    {
        _meterImage.fillAmount = oldProgress;
        _delayedMeterImage.fillAmount = targetProgress;
        DOVirtual.DelayedCall(DelayHold, Tween)
            .SetUpdate(_ignoreScaledTime);

        void Tween()
        {
            _delayedMeterTween = DOVirtual.Float(oldProgress, targetProgress, DelayDuration, (value) =>
            {
                _meterImage.fillAmount = value;
            }).SetUpdate(_ignoreScaledTime);
        }
    }

    public virtual void FadeIn()
    {
        _meterBackgroundImage?.DOFade(1f, 0.5f);
        _meterImage?.DOFade(1f, 0.5f);
        _delayedMeterImage?.DOFade(1f, 0.5f);
    }
    
    public virtual void FadeOut()
    {
        _meterBackgroundImage?.DOFade(0f, 0.5f);
        _meterImage?.DOFade(0f, 0.5f);
        _delayedMeterImage?.DOFade(0f, 0.5f);
    }
}
