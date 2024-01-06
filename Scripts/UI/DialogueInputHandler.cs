using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class DialogueInputHandler : MonoBehaviour
{
    [SerializeField] private DialogueRunner _dialogueRunner;
    [SerializeField] private LineView _lineView;
    public void OnEnable()
    {
        if(PlayerInputManager.Instance)
            PlayerInputManager.Instance.ConfirmPressed += OnConfirmPressed;
    }
    
    public void OnDisable()
    {
        if(PlayerInputManager.Instance)
            PlayerInputManager.Instance.ConfirmPressed -= OnConfirmPressed;
    }

    private void OnConfirmPressed()
    {
        if (!_dialogueRunner.IsDialogueRunning) return;
        _lineView.UserRequestedViewAdvancement();
    }
}
