using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.UI;
using Sirenix.OdinInspector;
using UnityEngine.EventSystems;

[DefaultExecutionOrder(0)]
public class SettingsManager : MonoBehaviour
{
    [SerializeField] private Canvas _settingsCanvas;
    [SerializeField] private TabGroup _tabGroup;
    [SerializeField] private Core.UI.TabButton _startingTab;
    [LabelText("Settings UI Controller")]
    [SerializeField] private SettingsUIController _uiController;
    private SettingsData _settingsData;
    public static SettingsManager Instance;
    public bool IsSettingsOpen { get; private set; } = false;
    
    private bool _isDirty = false;

    public delegate void OnSettingsOpenEventHandler();
    public event OnSettingsOpenEventHandler OnSettingsOpen;
    
    public delegate void OnSettingsCloseEventHandler();
    public event OnSettingsCloseEventHandler OnSettingsClose;
    
    //Read-only getters for settings data.
    public bool DisplayDamageValues => _settingsData.DisplayDamageValues;
    public bool HitstopEnabled => _settingsData.HitstopEnabled;
    public bool ScreenDamageFXEnabled => _settingsData.ScreenDamageFXEnabled;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("SettingsManager already exists. Deleting this instance.");
            Destroy(this);
            return;
        }

        Initialize();
    }

    private void Start()
    {
        _uiController.UpdateUI(_settingsData);
        ApplyVolumeSettings();
    }

    private void Initialize()
    {
        //Load settings data from file if it exists.
        if (GameDataFileHandler.LoadSettingsData(out var data))
        {
            _settingsData = data;
        }
        else
        {
            //If no settings data file exists, create a new one and save it.
            _settingsData = new SettingsData();
            GameDataFileHandler.WriteSettingsData(_settingsData);
        }
    }

    private void OnEnable()
    {
        if (PlayerInputManager.Instance != null)
        {
            PlayerInputManager.Instance.CancelPressed += OnCancelPressed;
            PlayerInputManager.Instance.NextTabPressed += OnNextTabPressed;
            PlayerInputManager.Instance.PreviousTabPressed += OnPreviousTabPressed;
            PlayerInputManager.Instance.SettingsPressed += OnSettingsPressed;
        }
    }

    private void OnDisable()
    {
        if (PlayerInputManager.Instance != null)
        {
            PlayerInputManager.Instance.CancelPressed -= OnCancelPressed;
            PlayerInputManager.Instance.NextTabPressed -= OnNextTabPressed;
            PlayerInputManager.Instance.PreviousTabPressed -= OnPreviousTabPressed;
            PlayerInputManager.Instance.SettingsPressed -= OnSettingsPressed;
        }
    }

    /// <summary>
    /// Change the UI visibility of the settings window.
    /// </summary>
    /// <param name="state">True to show, False to hide.</param>
    public void ToggleWindow(bool state)
    {
        //_settingsCanvas.enabled = state;
        switch (state)
        {
            case true:
                Show();
                break;
            case false:
                Hide();
                break;
        }
    }

    private void ApplyVolumeSettings()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogError("AudioManager is null. Cannot apply volume settings.");
            return;
        }
        AudioManager.Instance.MasterVolume = _settingsData.MuteAudio ? 0f : _settingsData.MasterVolume;
        AudioManager.Instance.MusicVolume = _settingsData.MusicVolume;
        AudioManager.Instance.SFXVolume = _settingsData.SFXVolume;
        //AudioManager.Instance.VoiceVolume = _settingsData.VoiceVolume;
        //AudioManager.Instance.AmbienceVolume = _settingsData.AmbienceVolume;
    }

    [Button("Show Settings")][ButtonGroup("Settings")][GUIColor("Green")]
    private void Show()
    {
        OnSettingsOpen?.Invoke();
        if (PlayerInputManager.Instance != null)
        {
            PlayerMenuManager.Instance.CloseMenu();
            PlayerInputManager.Instance.SetInWorldControlState(false); //Disable player in-game input while settings are open.
        }
        Time.timeScale = 0f;
        AudioManager.PlayOneShot(AudioManager.Instance.UIEvents.UIOpen, Vector3.zero);
        _settingsCanvas.gameObject.SetActive(true);
        _startingTab.Select();
        IsSettingsOpen = true;
    }
    
    [Button("Hide Settings")][ButtonGroup("Settings")][GUIColor("Red")]
    private void Hide()
    {
        Time.timeScale = 1f;
        OnSettingsClose?.Invoke();
        AudioManager.PlayOneShot(AudioManager.Instance.UIEvents.UIClose, Vector3.zero);
        if(PlayerInputManager.Instance != null)
             PlayerInputManager.Instance.SetInWorldControlState(true); //Re-enable player in-game input.
        _settingsCanvas.gameObject.SetActive(false);
        // If settings have been changed, save them.
        if (_isDirty)
        {
            GameDataFileHandler.WriteSettingsData(_settingsData);
            _isDirty = false;
            //TODO: Confirmation popup window before closing the settings window if settings have been changed.
        } 
            
        IsSettingsOpen = false;
    }
    

    #region INPUT CALLBACKS
    private void OnSettingsPressed()
    {
        if (IsSettingsOpen)
            ToggleWindow(false);
        else
            ToggleWindow(true);
    }
    
    private void OnCancelPressed()
    {
        if (IsSettingsOpen)
        {
            ToggleWindow(false);
        }
    }
    
    private void OnNextTabPressed()
    {
        if (IsSettingsOpen)
        {
            _tabGroup.GetNextTab().Select();
        }
    }
    
    private void OnPreviousTabPressed()
    {
        if (IsSettingsOpen)
        {
            _tabGroup.GetPreviousTab().Select();
        }
    }
    #endregion

    #region SETTINGS SETTERS
    public void UpdateMasterVolume(float value)
    {
        _settingsData.MasterVolume = value;
        AudioManager.Instance.MasterVolume = _settingsData.MuteAudio ? 0f : value;
        _isDirty = true;
    }
    
    public void UpdateMusicVolume(float value)
    {
        _settingsData.MusicVolume = value;
        AudioManager.Instance.MusicVolume = value;
        _isDirty = true;
    }
    
    public void UpdateSFXVolume(float value)
    {
        _settingsData.SFXVolume = value;
        AudioManager.Instance.SFXVolume = value;
        _isDirty = true;
    }
    
    public void UpdateVoiceVolume(float value)
    {
        _settingsData.VoiceVolume = value;
        _isDirty = true;
    }

    public void UpdateDamageValueToggle(bool value)
    {
        _settingsData.DisplayDamageValues = value;
        _isDirty = true;
    }
    
    public void UpdateHitStopToggle(bool value)
    {
        _settingsData.HitstopEnabled = value;
        _isDirty = true;
    }
    
    public void UpdateScreenDamageToggle(bool value)
    {
        _settingsData.ScreenDamageFXEnabled = value;
        _isDirty = true;
    }
    
    public void UpdateMuteAudioToggle(bool value)
    {
        _settingsData.MuteAudio = value;
        AudioManager.Instance.MasterVolume = value ? 0f : _settingsData.MasterVolume;
        _isDirty = true;
    }

    #endregion
}
