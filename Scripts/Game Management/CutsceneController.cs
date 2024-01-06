using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

public class CutsceneController : MonoBehaviour
{
    [SerializeField] private PlayableDirector _director;
    [SerializeField] private bool PlayOnStart = false;
    [SerializeField] private bool HidePlayer = true;
    [SerializeField] private bool DisableControls = true;
    [SerializeField] private bool HideUI = true;
    [SerializeField] private TextMeshProUGUI _skipText;
    public UnityEvent OnCutsceneFinished;
    private Coroutine _cutsceneCoroutine;
    private int _skipPressCount = 0;

    private void OnEnable()
    {
        if(PlayerInputManager.Instance)
            PlayerInputManager.Instance.ConfirmPressed += OnConfirmPressed;
    }

    private void OnDisable()
    {
        if(PlayerInputManager.Instance)
            PlayerInputManager.Instance.ConfirmPressed -= OnConfirmPressed;
    }

    private void Start()
    {
        if (PlayOnStart) { PlayCutscene(); }
        _skipText.gameObject.SetActive(false);
    }
    
    public void PlayCutscene()
    {
        if (_director.state == PlayState.Playing) return;
        if(HidePlayer)
            GameManager.Instance.Player.HidePlayerMesh(true);
        if(DisableControls || HidePlayer)
            PlayerInputManager.Instance.SetInWorldControlState(false);
        if(HideUI)
            StatusDisplayManager.Instance.Hide(true);
        _director.Play();
        _cutsceneCoroutine = StartCoroutine(WaitForCutsceneToFinish());
    }
    
    private IEnumerator WaitForCutsceneToFinish()
    {
        yield return new WaitForEndOfFrame();
        while (_director.state != PlayState.Playing)
        {
            //Wait for the cutscene to start playing before checking it's finished.
            yield return null;
        }
        while (_director.state != PlayState.Paused)
        {
            //Wait for the cutscene to finish playing.
            yield return null;
        }
        if(HidePlayer)
            GameManager.Instance.Player.HidePlayerMesh(false);
        if(DisableControls || HidePlayer)
            PlayerInputManager.Instance.SetInWorldControlState(true);
        if(HideUI)
            StatusDisplayManager.Instance.Hide(false);
        OnCutsceneFinished?.Invoke();
    }
    
    private void OnConfirmPressed()
    {
        if (_director.state != PlayState.Playing) return;
        _skipPressCount++;
        _skipText.gameObject.SetActive(true);
        if (_skipPressCount >= 2)
        {
            _director.time = _director.duration;
            _skipPressCount = 0;
            _skipText.gameObject.SetActive(false);
        }
    }
}
