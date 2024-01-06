using System;
using System.Collections;
using System.Collections.Generic;
using FMOD;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using Sirenix.OdinInspector;
using Debug = UnityEngine.Debug;

/// <summary>
/// Manages all audio in the game through FMOD.
/// </summary>
/// 
public class AudioManager : MonoBehaviour
{
    private float _masterVolume = 1f;
    private float _musicVolume = 1f;
    private float _sfxVolume = 1f;
    public static AudioManager Instance { get; private set; }
    
    [FoldoutGroup("FMOD Audio Events")]
    [field: SerializeField] public GeneralSoundEvents Events { get; private set; }
     [FoldoutGroup("FMOD Audio Events")]
    [field: SerializeField] public UISoundEvents UIEvents { get; private set; }
     [FoldoutGroup("FMOD Audio Events")]
    [field: SerializeField] public CombatSoundEvents CombatEvents { get; private set; }
     [FoldoutGroup("FMOD Audio Events")]
    [field: SerializeField] public VoiceSoundEvents VoiceSoundEvents{ get; private set; }
    
    private List<EventInstance> _eventInstances = new List<EventInstance>();
    private EventInstance _BGMInstance;

    #region PUBLIC PROPERTIES
    public float MasterVolume
    {
        get => _masterVolume;
        set
        {
            _masterVolume = value;
            RuntimeManager.GetBus("bus:/").setVolume(_masterVolume);
        }
    }
    public float MusicVolume
    {
        get => _musicVolume;
        set
        {
            _musicVolume = value;
            RuntimeManager.GetBus("bus:/Music").setVolume(_musicVolume);
        }
    }
    public float SFXVolume
    {
        get => _sfxVolume;
        set
        {
            _sfxVolume = value;
            RuntimeManager.GetBus("bus:/SFX").setVolume(_sfxVolume);
        }
    }
    #endregion
    
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("AudioManager already exists. Deleting this instance.");
            Destroy(this);
        }
    }

    /// <summary>
    /// Plays One Shot sound at a world position.
    /// </summary>
    /// <param name="eventReference"></param>
    /// <param name="worldPosition"></param>
    public static void PlayOneShot(EventReference eventReference, Vector3 worldPosition = default)
    {
        RuntimeManager.PlayOneShot(eventReference, worldPosition);
    }

    /// <summary>
    /// Plays a one shot sound attached to a game object.
    /// </summary>
    /// <param name="eventReference"></param>
    /// <param name="gameObject"></param>
    public static void PlayOneShotAttached(EventReference eventReference, GameObject gameObject)
    {
        RuntimeManager.PlayOneShotAttached(eventReference, gameObject);
    }

    public EventInstance CreateEventInstance(EventReference eventReference)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
        _eventInstances.Add(eventInstance);
        return eventInstance;
    }

    /// <summary>
    /// Gets the GUID of the event instance.
    /// </summary>
    /// <param name="instance">Instance to get GUID from.</param>
    /// <param name="guid">GUID of event from instance. Empty GUID as fallback if it wasn't found.</param>
    /// <returns>Bool if GUID was attainable from instance.</returns>
    private static bool GetInstanceGUID(EventInstance instance, out GUID guid)
    {
        if(instance.getDescription(out var description) == RESULT.OK)
        {
            if (description.getID(out guid) == RESULT.OK)
            {
                return true;
            }
        }
        guid = new GUID();
        return false;
    }

    [Button("Play BGM", ButtonSizes.Small), GUIColor("Green")]
    public void PlayBGM(EventReference sound)
    {
        if (_BGMInstance.isValid())
        {
            //If the same music is already playing, don't do anything.
            if(GetInstanceGUID(_BGMInstance, out var guid) && guid == sound.Guid)
                return;
            //Otherwise, stop the current music.
            _BGMInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            _BGMInstance.release();
        }
        _BGMInstance = RuntimeManager.CreateInstance(sound);
        _BGMInstance.start();
    }
    
    [Button("Stop BGM", ButtonSizes.Small), GUIColor("Red")]
    public void StopBGM(bool immediate = false)
    {
        if (!_BGMInstance.isValid())
        {
            Debug.LogWarning("No BGM to stop.");
            return;
        }
        _BGMInstance.stop(immediate ? FMOD.Studio.STOP_MODE.IMMEDIATE : FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _BGMInstance.release();
    }

    [Button("Cleanup Event Instances", ButtonSizes.Medium), GUIColor("Red")]
    private void CleanUp()
    {
        foreach (var eventInstance in _eventInstances)
        {
            eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            eventInstance.release();
        }
        _eventInstances.Clear();
    }

    private void OnDestroy()
    {
        CleanUp();
    }
}
