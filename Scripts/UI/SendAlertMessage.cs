using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendAlertMessage : MonoBehaviour
{
    [SerializeField] private List<string> _messages;
    [SerializeField] private float _duration;
    
    public void SendMessage()
    {
        if (AlertDisplay.Instance == null)
        {
            Debug.LogError("Alert Display HUD was not found. Unable to send alert.");
            return;
        }

        foreach (var message in _messages)
        {
            AlertDisplay.Instance.ShowMessage(message, _duration);
        }
    }
}
