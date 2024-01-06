using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentDeathController : MonoBehaviour
{
    [InfoBox("Will generate a unique ID for this object based on its name, position, rotation and the scene that it is on awake.")]
    [ReadOnly] [ShowInInspector] private int _uniqueID;
    [SerializeField] private bool _isPermanent = false;
    
    private void Awake()
    {
        GenerateID();
    }

    /// <summary>
    /// Check if this enemy is dead in the world manager and disable it if it is.
    /// </summary>
    private void Start()
    {
        if (WorldManager.Instance == null)
        {
            Debug.LogWarning("WorldManager not found. Persistent death could not be checked.");
            return;
        }
        gameObject.SetActive(!WorldManager.Instance.IsEnemyDead(_uniqueID));
    }

    /// <summary>
    /// Creates a unique ID for this object based on its name, position, rotation and the scene that it is on.
    /// </summary>
    [Button("Preview UID")]
    private void GenerateID()
    {
        var context = SceneManager.GetActiveScene().name + gameObject.name + transform.position + transform.rotation;
        _uniqueID = context.GetHashCode();
    }

    /// <summary>
    /// Callback for when this enemy dies. Registers the death in the world manager.
    /// </summary>
    public void RegisterDeath()
    {
        if (WorldManager.Instance == null)
        {
            Debug.LogWarning("WorldManager not found. Persistent death not registered.");
            return;
        }
        if (_isPermanent)
        {
            WorldManager.Instance.RegisterPermanentEnemyDeath(_uniqueID, this.gameObject);
        }
        else
        {
            WorldManager.Instance.RegisterEnemyDeath(_uniqueID, this.gameObject);
        }
    }
}
