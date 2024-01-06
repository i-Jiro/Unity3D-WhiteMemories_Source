using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class WireframeDissolve : MonoBehaviour
{
    private Renderer _renderer;
    [SerializeField] private float _dissolveDuration = 2f;
    [SerializeField] private ParticleSystem _particleSystem;
    private Tween _dissolveTween;
    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        //Instantiate a personal instance of the material to change shader properties.
        _renderer.material = Instantiate(_renderer.material);
    }

    public void StartDissolve()
    {
        void SetParticlePlayState(bool value)
        {
            if (_particleSystem == null) return;
            if(value)
                _particleSystem.Play();
            else
                _particleSystem.Stop();
        }
        
        
        SetParticlePlayState(true);
        _dissolveTween = DOVirtual.Float(0f, 1f, _dissolveDuration, (x) => _renderer.material.SetFloat("_Dissolve", x));
        DOVirtual.DelayedCall(_dissolveDuration * 0.5f, () =>
        {
            SetParticlePlayState(false);
        });
    }

    public void SetDissolve(float value)
    {
        if(_dissolveTween != null)
            _dissolveTween.Kill();
        _renderer.material.SetFloat("_Dissolve", value);
    }
}
