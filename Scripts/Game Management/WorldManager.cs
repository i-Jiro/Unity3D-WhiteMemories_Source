using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Manages and handles the persistent state of the world.
/// Keeps track of persistent enemy deaths, global switches, etc.
/// </summary>
///
public class WorldManager : MonoBehaviour
{
    public static WorldManager Instance { get; private set; }
    [LabelText("Dead Enemies")]
    [ShowInInspector] private Dictionary<int, string> _deadEnemies = new Dictionary<int, string>();
    [LabelText("Permanently Killed Enemies")]
    [ShowInInspector] private Dictionary<int, string> _permaDeadEnemies = new Dictionary<int, string>();
    [LabelText("Active Global Switches")]
    [ShowInInspector] private Dictionary<int, GlobalSwitchData> _globalSwitches = new Dictionary<int, GlobalSwitchData>();
    [SerializeField] private ScriptableObjectReferenceCache switchSOCache;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("WorldManager already exists. Deleting this instance.");
            Destroy(this);
        }
    }

    private void Start()
    {
        switchSOCache.Initialize();
    }
    
    public void LoadWorldData()
    {
        ResetGlobalSwitches();
        ResetPermanentEnemyDeaths();
        //Load world data from file if it exists.
        if (!GameDataFileHandler.LoadWorldData(out var data, switchSOCache)) return;
        
        for(int i = 0; i < data.PermaDeadEnemyKeys.Count; i++)
        {
            _permaDeadEnemies.Add(data.PermaDeadEnemyKeys[i], data.PermaDeadEnemyName[i]);
        }
        for(int i = 0; i < data.GlobalSwitchKeys.Count; i++)
        {
            _globalSwitches.Add(data.GlobalSwitchKeys[i], data.GlobalSwitchData[i]);
            if(_globalSwitches.TryGetValue(data.GlobalSwitchKeys[i], out var switchData))
            {
                switchData.IsActive = data.GlobalSwitchValue[i];
            }
            else
            {
                Debug.LogError("Unable to load global switch data. Switch data not found. " + data.GlobalSwitchKeys[i]);
            }
        }
    }
    
    public void SaveWorldData()
    {
        var permaDeadEnemyIDs = new List<int>();
        var permaDeadEnemyNames = new List<string>();
        
        var globalSwitchIDs = new List<int>();
        var globalSwitchData = new List<GlobalSwitchData>();
        var globalSwitchValue = new List<bool>();
        
        foreach (var entry in _permaDeadEnemies)
        {
            permaDeadEnemyIDs.Add(entry.Key);
            permaDeadEnemyNames.Add(entry.Value);
        }
        
        foreach(var entry in _globalSwitches)
        {
            globalSwitchIDs.Add(entry.Key);
            globalSwitchData.Add(entry.Value);
            globalSwitchValue.Add(entry.Value.IsActive);
        }
        
        var data = new WorldData
        {
            PermaDeadEnemyKeys = permaDeadEnemyIDs,
            PermaDeadEnemyName = permaDeadEnemyNames,
            GlobalSwitchKeys = globalSwitchIDs,
            GlobalSwitchData = globalSwitchData,
            GlobalSwitchValue = globalSwitchValue
            
        };
        GameDataFileHandler.WriteWorldData(data, switchSOCache);
    }
    
    #region Persistent Enemy Death Management
    /// <summary>
    /// Generic enemy kill registration. Used for enemies that respawn.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="enemy"></param>
    public void RegisterEnemyDeath(int id, GameObject enemy)
    {
        if(_deadEnemies.ContainsKey(id)) return;
        _deadEnemies.Add(id, enemy.name);
    }
    
    public void UnregisterEnemyDeath(int id)
    {
        if(_deadEnemies.ContainsKey(id))
            _deadEnemies.Remove(id);
    }
    
    
    /// <summary>
    /// Clears the list of killed enemies. Used when the player dies or reaches a checkpoint.
    /// Does not clear the permanent enemy death list.
    /// </summary>
    [Button("Reset Enemy Deaths")]
    public void ResetEnemyDeaths()
    {
        _deadEnemies.Clear();
    }
    
    /// <summary>
    /// Boss enemy kill registration. Used for enemies that do not respawn.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="enemy"></param>
    public void RegisterPermanentEnemyDeath(int id, GameObject enemy)
    {
        if(_permaDeadEnemies.ContainsKey(id)) return;
        _permaDeadEnemies.Add(id, enemy.name);
    }
    
    public void UnregisterPermanentEnemyKill(int id)
    {
        if(_permaDeadEnemies.ContainsKey(id))
            _permaDeadEnemies.Remove(id);
    }
    
    /// <summary>
    /// Clears the permanent enemy death list.
    /// </summary>
    public void ResetPermanentEnemyDeaths()
    {
        _permaDeadEnemies.Clear();
    }
    
    public bool IsEnemyDead(int id)
    {
        return _deadEnemies.ContainsKey(id) || _permaDeadEnemies.ContainsKey(id);
    }
    #endregion

    #region Global Switches Management
    /// <summary>
    /// Registers a global switch in the world manager.
    /// Should be called when a global switch when it is relevant to the player at time of encounter.
    /// </summary>
    /// <param name="switchData"></param>
    /// <returns></returns>
    [Button("Register Global Switch")]
    public bool RegisterGlobalSwitch(GlobalSwitchData switchData)
    {
        var hash = switchData.Guid.GetHashCode();
        if (_globalSwitches.ContainsKey(hash))
        {
            Debug.LogWarning($"Global switch with ID {switchData.Guid} already registered." +
                             $"Switch data: {switchData}");
            return false;
        }
        _globalSwitches.Add(hash, switchData);
        return true;
    }
    
    /// <summary>
    /// De-registers a global switch in the world manager.
    /// </summary>
    /// <param name="switchData"></param>
    public void UnregisterGlobalSwitch(GlobalSwitchData switchData)
    {
        var hash = switchData.Guid.GetHashCode();
        if (!_globalSwitches.TryGetValue(hash, out var globalSwitch)) return;
        globalSwitch.IsActive = false;
        _globalSwitches.Remove(hash);
    }
    
    /// <summary>
    /// Sets the state of a global switch. Can be used to register a switch with a starting state.
    /// Will register the switch if it is not registered.
    /// </summary>
    /// <param name="switchData"></param>
    /// <param name="state"></param>
    public void SetGlobalSwitchState(GlobalSwitchData switchData, bool state)
    {
        var hash = switchData.Guid.GetHashCode();
        if (_globalSwitches.TryGetValue(hash, out var globalSwitch))
            globalSwitch.IsActive = state;
        else
        {
            //Register the switch if it is not registered and then try setting the state again.
            if(RegisterGlobalSwitch(switchData))
            {
                SetGlobalSwitchState(switchData, state);
            }
        }
    }
    
    /// <summary>
    /// Check if a global switch is registered.
    /// </summary>
    /// <param name="switchData"></param>
    /// <returns></returns>
    
    public bool HasGlobalSwitch(GlobalSwitchData switchData)
    {
        var hash = switchData.Guid.GetHashCode();
        return _globalSwitches.ContainsKey(hash);
    }
    
    /// <summary>
    /// Get the current state of a global switch.
    /// </summary>
    /// <param name="switchData"></param>
    /// <returns></returns>

    public bool GetGlobalSwitchState(GlobalSwitchData switchData)
    {
        var hash = switchData.Guid.GetHashCode();
        if (_globalSwitches.TryGetValue(hash, out var globalSwitch))
            return globalSwitch.IsActive;
        //Return false if the switch is not registered
        return false;
    }

    [Button("Reset Global Switches")]
    public void ResetGlobalSwitches()
    {
        foreach (var globalSwitch in _globalSwitches)
        {
            globalSwitch.Value.IsActive = false;
        }
        _globalSwitches.Clear();
    }

    #endregion
}
