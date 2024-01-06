using System.Collections;
using System.Collections.Generic;
using Core.GameItems;
using Sirenix.Serialization;
using UnityEngine;

/// <summary>
/// Data container for the player's save data.
/// </summary>

[System.Serializable]
public class PlayerSaveWrapper
{
    public string LastSavedSceneName;
    public string SavedSplineName;
    public Vector3 SavedPosition;
    
    public float MaxHealth;
    public float MaxEx;
    public float MaxStamina;
    
    public float CurrentHealth;
    public float CurrentEx;
    public float CurrentStamina;
    
    public List<ItemData> ItemsInInventory;
    public List<int> ItemStacks;
    public List<ItemData> ItemsInQuickSlot;
    
    public PlayerSaveWrapper()
    {
        LastSavedSceneName = "";
        SavedSplineName = "";
        SavedPosition = Vector3.zero;
        MaxHealth = 0;
        MaxEx = 0;
        MaxStamina = 0;
        CurrentHealth = 0;
        CurrentEx = 0;
        CurrentStamina = 0;
        ItemsInInventory = new List<ItemData>();
        ItemStacks = new List<int>();
        ItemsInQuickSlot = new List<ItemData>();
    }
}
