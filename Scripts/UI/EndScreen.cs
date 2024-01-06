using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EndScreen : MonoBehaviour
{
   [SerializeField] private CanvasGroup _endScreenCanvasGroup;
   [SerializeField] private float delayStart = 5f;

   private void Start()
   {
      _endScreenCanvasGroup.alpha = 0f;
   }

   public void Activate()
   {
      StartCoroutine(DelayedStart());
   }
   
   private IEnumerator DelayedStart()
   {
      yield return new WaitForSeconds(delayStart);
      _endScreenCanvasGroup.DOFade(1f, 3f);
      DOVirtual.DelayedCall(10f, () =>
      {
         GameManager.Instance.ReturnToTitleScreen();
      });
   }
}
