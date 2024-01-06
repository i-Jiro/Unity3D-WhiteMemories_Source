using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;

public class WorldIndicator : MonoBehaviour
{
    [FormerlySerializedAs("_indicator")] [SerializeField] private RawImage _indicatorImage;
    [SerializeField] private float _fadeDuration = 0.25f;
    [SerializeField] private AnimationCurve _bounceCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
    [SerializeField] private float _bounceHeight = 0.5f;
    private Camera _mainCamera;
    private float _yStart;
    private Tween _fadeTween;

    private void Awake()
    {
        _bounceCurve.postWrapMode = WrapMode.Loop;
        _yStart = transform.localPosition.y;
        //gameObject.SetActive(false);
    }

    private void Start()
    {
        _mainCamera = Camera.main;
        _indicatorImage.color = new Color(1, 1, 1, 0);
    }
    
    [Button]
    public void FadeIn()
    {
        gameObject.SetActive(true);
        if(_fadeTween != null)
            _fadeTween.Kill();
        _fadeTween = _indicatorImage.DOColor(new Color(1, 1, 1, 1), _fadeDuration);
    }
    
    [Button]
    public void FadeOut()
    {
        if(_fadeTween != null)
            _fadeTween.Kill();
        _fadeTween = _indicatorImage.DOColor(new Color(1,1,1,0), _fadeDuration)
            .OnComplete(()=>gameObject.SetActive(false));
    }

    //Billboard the indicator to the camera
    private void LateUpdate()
    {
        transform.LookAt(_mainCamera.transform);
        transform.Rotate(0, 180, 0);
        transform.localPosition = new Vector3(transform.localPosition.x,  _yStart + (_bounceCurve.Evaluate(Time.time) * _bounceHeight), transform.localPosition.z);
    }

    private void OnDestroy()
    {
        if(_fadeTween != null)
            _fadeTween.Kill();
    }
}
