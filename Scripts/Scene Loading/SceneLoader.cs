using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Handles loading scenes and invoking events when loading starts and completes.
/// </summary>
public class SceneLoader : MonoBehaviour
{
    [SerializeField][Required]
    private string _sceneToLoadOnStart;
    [Header("Load Screen")]
    [SerializeField] GameObject _loadScreen;
    [SerializeField] Image _loadProgressBar;
    
    [Tooltip("The amount of time to wait before loading the scene.")]
    public float ArtificialLoadTime = 2f;

    [Header("Event Channels To Invoke")]
    [SerializeField] private VoidEventChannel _OnPreLoad;
    [SerializeField] private VoidEventChannel _OnLoadStart;
    [SerializeField] private VoidEventChannel _OnLoadComplete;
    
    [Header("Debug")]
    [SerializeField] private bool _debugLog;
    
    public static SceneLoader Instance;
    [ReadOnly][ShowInInspector]
    private bool _isLoading;
    private AsyncOperation _loadOperation;
    public bool IsLoading => _isLoading;
    
    public void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("SceneLoader already exists. Deleting this instance.");
            Destroy(this);
        }
    }

    public void Start()
    {
        LoadScene(_sceneToLoadOnStart);
    }
    
    /// <summary>
    /// Loads the scene with the given name in the build setting.
    /// </summary>
    /// <param name="sceneName"></param>
    /// Name of scene to load.
    public static void LoadScene(string sceneName)
    {
        if(Instance._isLoading)
        {
            if(Instance._debugLog)
                Debug.LogWarning("SceneLoader is already loading a scene.");
            return;
        }
        Instance._OnPreLoad?.RaiseEvent();
        Instance.StartCoroutine(Instance.LoadRoutine(sceneName));
    }

    private IEnumerator LoadRoutine(string sceneName)
    {
        List<AsyncOperation> _operations = new List<AsyncOperation>();
        //Wait for the screen to fade out if fade is playing.
        while(ScreenFader.Instance.IsFading)
        {
            yield return new WaitForEndOfFrame();
        }
        Instance._OnLoadStart?.RaiseEvent();
        Instance._loadScreen.SetActive(true);
        yield return new WaitForEndOfFrame();
        
        Scene currentScene = SceneManager.GetActiveScene();
        _isLoading = true;
        //Unload current scene if it's not the master scene.
        if(currentScene.name != "MasterScene")
        {
            //SceneManager.UnloadSceneAsync(currentScene);
            _operations.Add(SceneManager.UnloadSceneAsync(currentScene));
            if(_debugLog)
                Debug.Log($"Unloading {currentScene.name}");
        }
        yield return null;
        
        //Additively add the scene.
        _loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        _loadOperation.allowSceneActivation = false; //Don't activate the scene until it's fully loaded.
        _operations.Add(_loadOperation);

        //Wait until the load is finished.
        //NOTE: Unity considers a scene loaded when it's 90% loaded.
        _loadProgressBar.fillAmount = 0.1f;
        while (_loadOperation.progress < 0.9f)
        {
            yield return new WaitForSeconds(0.1f);
            _loadProgressBar.fillAmount = _loadOperation.progress + 0.1f;
            yield return new WaitForEndOfFrame();
        }
        /*
        float totalSceneProgress = 0f;
        for(int i = 0; i < _operations.Count; i++)
        {
            while(!_operations[i].isDone)
            {
                totalSceneProgress = 0f;
                foreach(var operation in _operations)
                {
                    totalSceneProgress += operation.progress;
                }
                
                totalSceneProgress = (totalSceneProgress/_operations.Count);
                _loadProgressBar.fillAmount = totalSceneProgress;
                yield return new WaitForEndOfFrame();
            }
        }
        Debug.Log(totalSceneProgress);
        */
        
        //Activate scene once it's loaded.
        _loadOperation.allowSceneActivation = true;
        yield return new WaitForSeconds(ArtificialLoadTime);

        //Wait until the operation is fully done.
        while(!_loadOperation.isDone)
        {
            yield return new WaitForEndOfFrame();
        }
        
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
        yield return new WaitForEndOfFrame();
        _isLoading = false;
        _OnLoadComplete?.RaiseEvent();
        _loadScreen.SetActive(false);
        if(_debugLog)
            Debug.Log($"Finished loading {sceneName}.");
    }
}
