using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

public class CharacterVoiceHandler : MonoBehaviour
{
    [field: SerializeField] public EventReference VoiceHurt { get; private set; }
    [field: SerializeField] public EventReference VoiceAttack { get; private set; }
    [field: SerializeField] public EventReference VoiceDeath { get; private set; }

    public void PlayHurt()
    {
        AudioManager.PlayOneShotAttached(VoiceHurt, gameObject);
    }
    
    public void PlayAttack()
    {
        AudioManager.PlayOneShotAttached(VoiceAttack, gameObject);
    }
    
    public void PlayDeath()
    {
        AudioManager.PlayOneShotAttached(VoiceDeath, gameObject);
    }
}
