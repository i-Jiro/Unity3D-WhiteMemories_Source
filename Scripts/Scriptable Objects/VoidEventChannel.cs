using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Void Event Channel")]
public class VoidEventChannel : ScriptableObject
{
    public delegate void EventHandler();

    public event EventHandler OnEventRaised;

    public void RaiseEvent()
    {
        OnEventRaised?.Invoke();
    }
}
