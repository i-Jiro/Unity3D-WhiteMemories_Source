using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SettingsData
{
    public bool DisplayDamageValues = true;
    public bool HitstopEnabled = true;
    public bool ScreenDamageFXEnabled = true;
    public bool MuteAudio = false;
    public float MasterVolume = 1f;
    public float MusicVolume = 1f;
    public float SFXVolume = 1f;
    public float VoiceVolume = 1f;
}
