using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ComboCounter : MonoBehaviour
{
    public float MaxComboTime = 4f;
    public float FadeSpeed = 0.10f;
    [SerializeField] private Image _comboBar;
    [SerializeField] private TextMeshProUGUI _comboText;
    [SerializeField] private CanvasGroup _comboCanvasGroup;
    [SerializeField] private MMF_Player _mmfPlayer;
    private float _comboTimer;
    private int _comboCount;
    private bool _isCounting = false;
    private Vector3 _originalPos;

    private void Start()
    {
        _comboCanvasGroup.alpha = 0f;
        _originalPos = _comboText.transform.position;
    }

    public void AddCombo()
    {
        _comboCount++;
        _comboTimer = MaxComboTime;
        _comboText.text = _comboCount.ToString();
        //Shake combo text
        _comboText.transform.DOShakePosition(0.25f, 10f, 10, 90f, false, true);
        if (!_isCounting)
        {
            _comboCanvasGroup.DOFade(1f, FadeSpeed);
            _isCounting = true;
            StartCoroutine(ComboTimer());
        }
    }
    
    private IEnumerator ComboTimer()
    {
        while (_comboTimer > 0)
        {
            _comboTimer -= Time.deltaTime;
            _comboBar.fillAmount = _comboTimer / MaxComboTime;
            yield return null;
        }
        ResetCounter();
    }

    public void ResetCounter()
    {
        if(!_isCounting) return;
        _comboCanvasGroup.DOFade(0f, FadeSpeed)
            .OnComplete(() =>
            {
                _comboCount = 0;
                _comboText.text = _comboCount.ToString();
                _isCounting = false;
                _comboText.transform.position = _originalPos;
            });
    }
}
