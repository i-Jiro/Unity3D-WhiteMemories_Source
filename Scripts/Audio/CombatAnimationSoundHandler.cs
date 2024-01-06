using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;


public class CombatAnimationSoundHandler : MonoBehaviour
{
    private AudioManager _audioManager;

    private void Start()
    {
        _audioManager = AudioManager.Instance;
    }

    public void PlayWhoosh()
    {
        AudioManager.PlayOneShot(_audioManager.CombatEvents.Whoosh, transform.position);
    }

    public void PlayUnarmedWhoosh()
    {
        AudioManager.PlayOneShot(_audioManager.CombatEvents.UnarmedWhoosh, transform.position);
    }
    
    public void PlayGroundHit()
    {
        AudioManager.PlayOneShot(_audioManager.CombatEvents.GroundHit, transform.position);
    }
}
