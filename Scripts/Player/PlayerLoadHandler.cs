using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class PlayerLoadHandler : MonoBehaviour
{
    [Header("Data")]
    public PlayerData RuntimePlayerData;
   
    [Header("Void Event Channels To Listen To")]
    [FormerlySerializedAs("_OnSceneLoading")] public VoidEventChannel _OnSceneLoadStart;
    [FormerlySerializedAs("_OnSceneLoaded")] public VoidEventChannel _OnSceneComplete;
    public UnityEvent OnLoadStart;
    public UnityEvent OnLoadEnd;
    public void OnEnable()
    {
        _OnSceneLoadStart.OnEventRaised += OnSceneLoadStart;
        _OnSceneComplete.OnEventRaised += OnSceneComplete;
    }
    public void OnDisable()
    {
        _OnSceneLoadStart.OnEventRaised -= OnSceneLoadStart;
        _OnSceneComplete.OnEventRaised -= OnSceneComplete;
    }
    
    public void OnSceneLoadStart()
    {
        OnLoadStart?.Invoke();
    }
    
    public void OnSceneComplete()
    {
        OnLoadEnd?.Invoke();
        //Record scene-name only if the game is in the playing state.
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.CurrentGameState == GameState.PLAYING)
        {
            if(RuntimePlayerData != null) //Weird error if there isn't a null check here.
                RuntimePlayerData.CurrentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        }
    }
}
