using System;
using System.Collections;
using System.Collections.Generic;
using Core.GameItems;
using UnityEngine;
using UnityEngine.Events;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private PlayerData _runtimePlayerData;
    [SerializeField] private ItemData RestorativeItem;
    [Tooltip("VFX to play when checkpoint is activated")]
    public UnityEvent OnActivate;
    public UnityEvent OnInteract;
    public bool SaveGameOnActivate = true;
    public bool IsActivated { get; private set; }
    
    private void Awake()
    {
        IsActivated = false;
    }

    public void Save()
    {
        if (!IsActivated)
        {
            OnActivate?.Invoke();
            IsActivated = true;
        }
        AddRestorativeItem();
        _runtimePlayerData.CurrentHealth = _runtimePlayerData.MaxHealth;
        
        if (GameManager.Instance == null)
        {
            Debug.LogError("No GameManager found in scene! Unable to save.");
        }
        else
        {
            GameManager.Instance.SaveCheckPoint();
            if(SaveGameOnActivate)
                GameManager.Instance.SaveGame();
        }
        
        WorldManager.Instance.ResetEnemyDeaths();
        OnInteract?.Invoke();
    }
    
    private void AddRestorativeItem()
    {
        if(RestorativeItem == null) { Debug.LogError("Item data not set.");return;}
        Item item;
        if (_runtimePlayerData.Inventory.HasItem(RestorativeItem))
        {
            item = _runtimePlayerData.Inventory.GetItem(RestorativeItem);
            item.AddStack(item.MaxStack - item.CurrentStack);
            return;
        }
        _runtimePlayerData.Inventory.AddItem(RestorativeItem, RestorativeItem.MaxStack);
    }
}
