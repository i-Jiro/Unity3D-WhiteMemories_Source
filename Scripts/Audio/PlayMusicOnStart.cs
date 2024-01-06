using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlayMusicOnStart : MonoBehaviour
{
    [InfoBox("If true, the music will stop when this object is destroyed. Music will carry over if false.")]
    public bool StopOnDestroy = true;
    
    private EventInstance _musicInstance;
    [field: SerializeField] public EventReference MusicEvent;

    [Header("Void Event Channel")]
    [Tooltip("If true, this will wait for the load event to play the music.")]
    public bool WaitForLoadEvent;
    [SerializeField] private VoidEventChannel _LoadEventChannel;

    private void OnEnable()
    {
        if(_LoadEventChannel != null)
            _LoadEventChannel.OnEventRaised += PlayMusic;
    }

    private void OnDisable()
    {
        if(_LoadEventChannel)
            _LoadEventChannel.OnEventRaised -= PlayMusic;
    }


    // Start is called before the first frame update
    void Start()
    {
        if(!WaitForLoadEvent)
            PlayMusic();
    }
    
    public void PlayMusic()
    {
        if(AudioManager.Instance != null)
            AudioManager.Instance.PlayBGM(MusicEvent);
    }
    
    public void StopMusic()
    {
        if(AudioManager.Instance != null)
            AudioManager.Instance.StopBGM();
    }
    
    private void OnDestroy()
    {
        if(StopOnDestroy)
            StopMusic();
    }
}
