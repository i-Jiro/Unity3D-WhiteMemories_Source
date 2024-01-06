using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class AlertDisplay : MonoBehaviour
{
    public static AlertDisplay Instance;
    [ChildGameObjectsOnly]
    [SerializeField] private CanvasGroup _alertCanvasGroup;
    [SerializeField] private TextMeshProUGUI _alertText;
    [SerializeField] private float _fadeDuration = 0.5f;
    private Queue _messageQueue = new Queue();
    [HideInInspector]
    public bool IsDisplaying { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.Log("Alert display already exists. Destroying..");
            Destroy(this);
        }
    }

    private void Start()
    {
        _alertCanvasGroup.alpha = 0f;
    }

    /// <summary>
    /// Display a message given a string and duration. If already displaying a message, it will queue the incoming message.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="duration"></param>
    public void ShowMessage(string message, float duration)
    {
        if (IsDisplaying)
        {
            _messageQueue.Enqueue(new QueuedMessage(message,duration));
            return;
        }
        IsDisplaying = true;
        _alertText.text = message;
        _alertCanvasGroup.DOFade(1f, _fadeDuration)
            .OnComplete(() =>
            {
                DOVirtual.DelayedCall(duration, () => _alertCanvasGroup.DOFade(0f, _fadeDuration) .OnComplete(() =>
                {
                    IsDisplaying = false;
                    if (_messageQueue.Count <= 0) return;
                    var nextAlert = _messageQueue.Dequeue() as QueuedMessage;
                    if (nextAlert != null) ShowMessage(nextAlert.Message, nextAlert.Duration);
                }));
                //When message fades out check if there's any alerts that are queued.
            });
    }

    private class QueuedMessage
    {
        public string Message;
        public float Duration;
        public QueuedMessage(string message, float duration)
        {
            Message = message;
            Duration = duration;
        }
    }
}
