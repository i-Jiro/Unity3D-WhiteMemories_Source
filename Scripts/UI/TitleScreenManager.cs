using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TitleScreenManager : MonoBehaviour
{
    [FormerlySerializedAs("_startingButton")] [SerializeField] private Button _ContinueButton;
    [SerializeField] private Button _NewGameButton;
    [SerializeField] private Canvas _titleScreenCanvas;
    
    private void OnEnable()
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.OnSettingsOpen += Hide;
            SettingsManager.Instance.OnSettingsClose += Show;
        }
    }

    private void OnDisable()
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.OnSettingsOpen -= Hide;
            SettingsManager.Instance.OnSettingsClose -= Show;
        }
    }

    private void Start()
    {
        Show();
    }

    public void OnPressNewGame()
    {
        if(GameManager.Instance != null)
            GameManager.Instance.StartNewGame();
    }
    
    public void OnPressContinue()
    {
        if(GameManager.Instance != null)
            GameManager.Instance.LoadGame();
    }
    
    public void OnPressSettings()
    {
        SettingsManager.Instance.ToggleWindow(true);
    }
    
    public void OnPressQuit()
    {
        if(GameManager.Instance != null)
            GameManager.Instance.QuitGame();   
    }

    private void Hide()
    {
        _titleScreenCanvas.gameObject.SetActive(false);
    }

    private void Show()
    {
        _titleScreenCanvas.gameObject.SetActive(true);
        if(GameDataFileHandler.HasPlayerSaveFile())
        {
            _ContinueButton.Select();
            _ContinueButton.interactable = true;
        }
        else
        {
            _NewGameButton.Select();
            //Hide continue button if there is no save file.
            _ContinueButton.gameObject.SetActive(false);
            _ContinueButton.interactable = false;
        }
    }
}
