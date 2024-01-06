using System;
using System.Collections;
using Cinemachine;
using Sirenix.OdinInspector;
using UnityEngine;

public enum GameState
{
    TITLE,
    PLAYING,
    LOADING_GAME_DATA,
    PAUSED,
    GAME_OVER
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [ShowInInspector][ReadOnly]
    public GameState CurrentGameState { get; private set; } = GameState.TITLE;
    [Header("Player References")][FoldoutGroup("Player References", false)]
    public CinemachineVirtualCamera PlayerCamera;
    [FoldoutGroup("Player References", false)]
    public PlayerBaseController Player; 
    [Header("Player Data")][FoldoutGroup("Player Data", false)]
    [SerializeField] private PlayerData _RuntimePlayerData;
    [Header("Runtime Sets")][FoldoutGroup("Runtime Sets", false)]
    [SerializeField] private SplineRuntimeSet _SplineRuntimeSet;
    [SerializeField] private ScriptableObjectReferenceCache _itemSOCache;
        
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("GameManager already exists. Deleting this instance.");
            Destroy(this);
        }
    }

    private void Start()
    {
        Player.gameObject.SetActive(false);
        PlayerCamera.gameObject.SetActive(false);
        _itemSOCache.Initialize();
    }

    [Button("Start New Game")][ButtonGroup("Game Management")][GUIColor("Red")]
    public void StartNewGame()
    {
        CurrentGameState = GameState.PLAYING;
        SceneLoader.LoadScene("IntroScene");
        //Set playerpref to set player location in the scene with the PlayerStartLocation component.
        PlayerPrefs.SetString("LastExitPoint", "GAMESTART");
        Player.gameObject.SetActive(true);
        PlayerCamera.gameObject.SetActive(true);
        //Show player HUD
        if (StatusDisplayManager.Instance != null)
        {
            StatusDisplayManager.Instance.Hide(false);
            StatusDisplayManager.Instance.ForceUpdate();
        }
        //Load the first scene    
    }
    
    /// <summary>
    /// Loads the game data from the save file and set-ups the player.
    /// </summary>
    [Button("Load Game")][ButtonGroup("Game Management")][GUIColor("Yellow")]
    public void LoadGame()
    {
        if (CurrentGameState == GameState.LOADING_GAME_DATA && Application.isPlaying)
            return;
        //Loads the game data from the save file into the runtime player data.
        GameDataFileHandler.LoadPlayerSaveFile(out var data, _itemSOCache);
        _RuntimePlayerData.LoadSaveData(data);
        WorldManager.Instance.LoadWorldData();
        SceneLoader.LoadScene(_RuntimePlayerData.LastSavedSceneName);
        StartCoroutine(LoadGameRoutine());
    }
    
    /// <summary>
    /// Wait for the scene to load and then set-up the player.
    /// </summary>
    private IEnumerator LoadGameRoutine()
    {
        //Wait for the scene to start load.
        while (!SceneLoader.Instance.IsLoading) { yield return null; }

        CurrentGameState = GameState.LOADING_GAME_DATA;
        
        //Wait for the scene to finish loading.
        while(SceneLoader.Instance.IsLoading) { yield return null; }
        
        //Show player HUD
        if (StatusDisplayManager.Instance != null)
        {
            StatusDisplayManager.Instance.Hide(false);
            StatusDisplayManager.Instance.ForceUpdate();
        }
        
        //Clear the last exit point from PlayerPrefs.
        PlayerPrefs.SetString("LastExitPoint", "");
        UpdatePlayerPosition(_RuntimePlayerData.SavedPosition, _RuntimePlayerData.SavedSplineName);
        Player.gameObject.SetActive(true);
        PlayerCamera.gameObject.SetActive(true);
        CurrentGameState = GameState.PLAYING;
    }

    [Button("Save Game")][ButtonGroup("Game Management")][GUIColor("Green")]
    public void SaveGame()
    {
        if(CurrentGameState != GameState.PLAYING) return;
        Player.SavePosition(PositionType.SavePosition); //Record player position, and name of spline that the player is on.
        GameDataFileHandler.WritePlayerSaveFile(_RuntimePlayerData.CreateSaveData(), _itemSOCache);
        WorldManager.Instance.SaveWorldData();
    }

    public void SaveCheckPoint()
    {
        if(CurrentGameState != GameState.PLAYING) return;
        Player.SavePosition(PositionType.CheckpointPosition);
    }
    
    
    [Button("Return to Checkpoint")][ButtonGroup("Game Management")][GUIColor("Orange")]
    public void ResetToCheckPoint()
    {
        SceneLoader.LoadScene(_RuntimePlayerData.LastCheckpointSceneName);
        if(WorldManager.Instance != null)
            WorldManager.Instance.ResetEnemyDeaths();
        StartCoroutine(CheckpointRoutine());
    }
    
    private IEnumerator CheckpointRoutine()
    {
        while (!SceneLoader.Instance.IsLoading) { yield return null; }

        CurrentGameState = GameState.LOADING_GAME_DATA;
        
        //Wait for the scene to finish loading.
        while(SceneLoader.Instance.IsLoading) { yield return null; }
        PlayerPrefs.SetString("LastExitPoint", "");
        UpdatePlayerPosition(_RuntimePlayerData.LastCheckpointPosition, _RuntimePlayerData.LastCheckpointSplineName);
        CurrentGameState = GameState.PLAYING;
        

        yield return null;
    }
    
    /// <summary>
    /// Updates the player's position and spline.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="splineName"></param>
    private void UpdatePlayerPosition(Vector3 position, string splineName)
    {
        Player.transform.position = position;
        //Find the matching spline in the runtime set. 
        var spline = _SplineRuntimeSet.GetSplineByName(splineName);
        if(spline == null)
        {
            Debug.LogError("Could not find spline" + _RuntimePlayerData.SavedSplineName +
                           "\n Make sure to add it to the SplineRuntimeSet.");
        }
        Player.GetComponent<SplineWalker>().SetSpline(spline);
    }
    

    [Button("Title Screen")][ButtonGroup("Game Management")][GUIColor("Blue")]
    public void ReturnToTitleScreen()
    {
        CurrentGameState = GameState.TITLE;
        SceneLoader.LoadScene("TitleScreen");
        if (StatusDisplayManager.Instance != null)
        {
            StatusDisplayManager.Instance.Hide(true);
        }
        Player.gameObject.SetActive(false);
        PlayerCamera.gameObject.SetActive(false);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

