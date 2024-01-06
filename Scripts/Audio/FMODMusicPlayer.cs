using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class FMODMusicPlayer : MonoBehaviour
{
    private EventInstance _musicInstance;
    [field: SerializeField] public EventReference MusicEvent;

    public void Play()
    {
        if(AudioManager.Instance != null)
            AudioManager.Instance.PlayBGM(MusicEvent);
    }
    
    public void Stop()
    {
        if(AudioManager.Instance != null)
            AudioManager.Instance.StopBGM();
    }
}
