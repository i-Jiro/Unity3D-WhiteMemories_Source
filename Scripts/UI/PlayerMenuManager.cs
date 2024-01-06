using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerMenuManager : MonoBehaviour
{
    public static PlayerMenuManager Instance { get; private set; }
    public bool IsMenuOpen { get; private set; } = false;
    [ChildGameObjectsOnly]
    [SerializeField] private GameObject _menuCanvas;
    [ChildGameObjectsOnly]
    [SerializeField] private Core.UI.TabButton _startingTab;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("PlayerMenuManager already exists. Deleting this instance.");
            Destroy(this);
        }
    }

    private void OnEnable()
    {
        if (PlayerInputManager.Instance == null) return;
        PlayerInputManager.Instance.StartMenuPressed += OpenMenu;
        PlayerInputManager.Instance.CancelPressed += CloseMenu;
    }
    
    private void OnDisable()
    {
        if(PlayerInputManager.Instance == null) return;
        PlayerInputManager.Instance.StartMenuPressed -= OpenMenu;
        PlayerInputManager.Instance.CancelPressed -= CloseMenu;
    }

    public void OpenMenu()
    {
        if(GameManager.Instance.CurrentGameState != GameState.PLAYING) return;
        if(PlayerInputManager.Instance != null)
            PlayerInputManager.Instance.SetInWorldControlState(false);
        IsMenuOpen = true;
        _menuCanvas.SetActive(true);
        _startingTab.Select();
        Time.timeScale = 0.0f;
    }
    
    public void CloseMenu()
    {
        if(PlayerInputManager.Instance != null)
            PlayerInputManager.Instance.SetInWorldControlState(true);
        if(IsMenuOpen == false) return;
        _menuCanvas.SetActive(false);
        IsMenuOpen = false;
        Time.timeScale = 1f;
    }
}
