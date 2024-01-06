using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the visual state of the settings UI.
/// </summary>
public class SettingsUIController : MonoBehaviour
{
    [FoldoutGroup("General Settings", expanded: false)]
    [SerializeField] DualButtonToggle _displayDamageValuesToggle;
    [FoldoutGroup("General Settings", expanded: false)]
    [SerializeField] DualButtonToggle _hitstopToggle;
    [FoldoutGroup("General Settings", expanded: false)]
    [SerializeField] DualButtonToggle _screenDamageFXToggle;

    [FoldoutGroup("Audio Settings", expanded: false)]
    [SerializeField] DualButtonToggle _muteAudioToggle;
    [FoldoutGroup("Audio Settings", expanded: false)]
    [SerializeField] SliderText _masterVolumeSlider;
    [FoldoutGroup("Audio Settings", expanded: false)]
    [SerializeField] SliderText _musicVolumeSlider;
    [FoldoutGroup("Audio Settings", expanded: false)]
    [SerializeField] SliderText _sfxVolumeSlider;
    
    /// <summary>
    /// Update the UI to match the settings data.
    /// </summary>
    /// <param name="settingsData"></param>
    public void UpdateUI(SettingsData settingsData)
    {
        _displayDamageValuesToggle.UpdateVisuals(settingsData.DisplayDamageValues);
        _hitstopToggle.UpdateVisuals(settingsData.HitstopEnabled);
        _screenDamageFXToggle.UpdateVisuals(settingsData.ScreenDamageFXEnabled);
        _muteAudioToggle.UpdateVisuals(settingsData.MuteAudio);
        _masterVolumeSlider.UpdateVisuals(settingsData.MasterVolume);
        _musicVolumeSlider.UpdateVisuals(settingsData.MusicVolume);
        _sfxVolumeSlider.UpdateVisuals(settingsData.SFXVolume);
    }

    #region BUTTON CALLBACKS
    public void ReturnToTitle()
    {
        SettingsManager.Instance.ToggleWindow(false);
        if(GameManager.Instance != null)
            GameManager.Instance.ReturnToTitleScreen();
    }

    public void LoadLastSave()
    {
        SettingsManager.Instance.ToggleWindow(false);
        if(GameManager.Instance != null)
            GameManager.Instance.LoadGame();
    }

    public void ExitGame()
    {
        SettingsManager.Instance.ToggleWindow(false);
        if(GameManager.Instance != null)
            GameManager.Instance.QuitGame();
    }
    #endregion
}
