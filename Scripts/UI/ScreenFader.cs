using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance;
    public Ease Ease = Ease.Linear;
    public Image image;
    public bool IsFading = false;
    private Tween _fadeTween;

    public void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("ScreenFader already exists. Deleting this instance.");
            Destroy(this);
        }
    }

    public void FadeIn(float duration)
    {
        if (_fadeTween != null)
        {
            _fadeTween.Kill();
        }
        IsFading = true;
        _fadeTween = DOVirtual.Float(0, 1, duration, SetImageAlpha)
            .SetEase(Ease)
            .OnComplete(() => IsFading = false);
    }

    public void FadeOut(float duration)
    {
        if (_fadeTween != null)
        {
            _fadeTween.Kill();
        }
        IsFading = true;
        _fadeTween = DOVirtual.Float(1, 0, duration, SetImageAlpha)
            .SetEase(Ease)
            .OnComplete(() => IsFading = false);
    }

    public void SetImageAlpha(float value)
    {
        var color = image.color;
        color.a = value;
        image.color = color;
    }
}
