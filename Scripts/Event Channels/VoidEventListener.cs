using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class VoidEventListener : MonoBehaviour
{
    [SerializeField] private VoidEventChannel _eventChannel;
    public UnityEvent OnEventRaised;

    private void Awake()
    {
        _eventChannel.OnEventRaised += Invoke;
    }

    private void Invoke()
    {
        OnEventRaised?.Invoke();
    }

    private void OnDestroy()
    {
        _eventChannel.OnEventRaised -= Invoke;
    }
}
