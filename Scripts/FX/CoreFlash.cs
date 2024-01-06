using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;

public class CoreFlash : MonoBehaviour
{
    [InfoBox("Should be used with the 'SimpleEmission' shader.")]
    [SerializeField] private LensFlareComponentSRP _lensFlare;
    [SerializeField] float _emissionIntensity = 1f;
    [Range(0.1f,1f)]
    [SerializeField] float _transitionTime = 0.5f;
    [SerializeField] private float _lensFlareCut = 0.75f;
    [SerializeField] private float _lensflareIntensity = 2f;
    [SerializeField] private float _fadeDuration = 1f;
    private TrailRenderer _trailRenderer;
    private Renderer _renderer;
    
    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _renderer.material = Instantiate(_renderer.material);
        if (!_trailRenderer) return;
        _trailRenderer = GetComponent<TrailRenderer>();
        _trailRenderer.material = _renderer.material;
    }

    [Button]
    public void FlashIn()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) return;
#endif
        var material =  _renderer.material;
        var startColor = material.GetColor("_EmissionColor");
        var endColor = startColor * _emissionIntensity;
        if(_lensFlare)
            DOVirtual.Float(0f, _lensflareIntensity, _transitionTime,
                (value) =>
                {
                    _lensFlare.intensity = value;
                });
        
        DOVirtual.Color(startColor,endColor, _transitionTime,
                (value) =>
                {
                    material.SetColor("_EmissionColor", value);
                    _renderer.UpdateGIMaterials();
                })
            .OnComplete(() => FlashOut(material, startColor, endColor));
    }

    private void FlashOut(Material material, Color startColor, Color endColor)
    {
        if(_lensFlare)
            DOVirtual.Float(_lensflareIntensity, 0f, _transitionTime * _lensFlareCut,
            (value) =>
            {
                _lensFlare.intensity = value;
            });
        
        DOVirtual.Color(endColor, startColor, _transitionTime,
            (value) =>
            {
                material.SetColor("_EmissionColor", value);
                _renderer.UpdateGIMaterials();
            });
    }

    public void FadeOut()
    {
        DOVirtual.Float(0f, 1f, _fadeDuration, (value)=> _renderer.material.SetFloat("_Transparency", value));
    }

    public void SetTransparency(float value)
    {
        _renderer.material.SetFloat("_Transparency", value);
    }
}
