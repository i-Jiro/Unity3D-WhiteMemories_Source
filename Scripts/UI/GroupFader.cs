using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupFader : MonoBehaviour
{
    [SerializeField] private List<CanvasGroup> _groups = new List<CanvasGroup>();
    [SerializeField] private float _fadeDuration = 0.5f;
    [SerializeField] private float _delayBetweenFades = 0.1f;
    
    public void FadeIn()
    {
        StartCoroutine(FadeInCoroutine());
    }
    
    public void FadeOut()
    {
        StartCoroutine(FadeOutCoroutine());
    }
    
    private IEnumerator FadeInCoroutine()
    {
        foreach (var group in _groups)
        {
            group.alpha = 0f;
        }
        
        foreach (var group in _groups)
        {
            while (group.alpha < 1f)
            {
                group.alpha += Time.deltaTime / _fadeDuration;
                yield return null;
            }
            yield return new WaitForSeconds(_delayBetweenFades);
        }
    }
    
    private IEnumerator FadeOutCoroutine()
    {
        foreach (var group in _groups)
        {
            group.alpha = 1f;
        }
        
        foreach (var group in _groups)
        {
            while (group.alpha > 0f)
            {
                group.alpha -= Time.deltaTime / _fadeDuration;
                yield return null;
            }
            yield return new WaitForSeconds(_delayBetweenFades);
        }
    }
}
