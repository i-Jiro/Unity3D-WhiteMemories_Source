using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private MMFeedbacks _cameraShakeFeedback;
    [Header("Event Channel To Listen For")]
    [SerializeField] private VoidEventChannel _eventChannel;
    
    private void OnEnable()
    {
        _eventChannel.OnEventRaised += Shake;
    }

    private void OnDisable()
    {
        _eventChannel.OnEventRaised -= Shake;
    }
    
    private void Shake()
    {
        _cameraShakeFeedback.Initialization();
        _cameraShakeFeedback.PlayFeedbacks();
    }
}
