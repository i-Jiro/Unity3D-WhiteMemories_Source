using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Core.GameItems;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

/// <summary>
/// Data container for the inventory system. Holds a list of items and a dictionary for faster lookup.
/// </summary>
[System.Serializable]
public class Inventory
{
    [field:SerializeField] public List<Item> Items { get; private set; } = new List<Item>(); //List of all items in the inventory.
    [OdinSerialize] private Dictionary<int, Item> _itemDictionary = new Dictionary<int, Item>(); //Dictionary for faster lookup for specific item.

    public Item[] QuickSlots;
    
    public delegate void OnInventoryChangedEventHandler();
    public event OnInventoryChangedEventHandler InventoryChanged;
    
    public Inventory()
    {
        Items = new List<Item>();
        _itemDictionary = new Dictionary<int, Item>();
        QuickSlots = new Item[2];
    }
    
    /// <summary>
    /// Sets an item in the quick slot.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="item"></param>
    public void SetQuickSlot(int index, Item item)
    {
        if (index < 0 || index >= QuickSlots.Length)
        {
            Debug.LogError("Trying to set an invalid index in the quick slots. Aborting.");return;
        }
        QuickSlots[index] = item;
        InventoryChanged?.Invoke();
    }
    

    /// <summary>
    /// Adds item to the inventory. If the item is stackable, it will add to the stack
    /// </summary>
    /// <param name="itemData">Item data reference to add item.</param>
    /// <param name="quantity">Amount to add.</param>
    [Button("Add Item"),][FoldoutGroup("Debug")]
    public void AddItem(ItemData itemData, int quantity = 1)
    {
        if(itemData == null) return;
        int hash = itemData.Guid.GetHashCode();
        //Check if the item is already in the inventory.
        if (_itemDictionary.TryGetValue(hash, out var item))
        {
            //Add to the stack if the item is stackable.
            item.AddStack(quantity);
        }
        else
        {
            Item newItem = CreateItem(itemData, quantity);

            _itemDictionary.Add(hash, newItem);
            Items.Add(newItem);
        }
        
        InventoryChanged?.Invoke();
    }

    /// <summary>
    /// Removes item from the inventory. If the item is stackable, it will remove from the stack
    /// </summary>
    /// <param name="itemData">Item data reference to remove item.</param>
    /// <param name="quantity">Amount to remove.</param>
    [Button("Remove Item")][FoldoutGroup("Debug")]
    public void RemoveItem(ItemData itemData, int quantity = 1)
    {
        if(itemData == null) return;
        int hash = itemData.Guid.GetHashCode();
        if (_itemDictionary.TryGetValue(hash, out var item))
        {
            item.RemoveStack(quantity);
            //Remove the item from the dictionary if it's stack is 0 or less.
            if(item.CurrentStack <= 0)
            {
                _itemDictionary.Remove(hash);
                Items.Remove(item);
                //Remove from quick slots if it's there.
                for (int i = 0; i < QuickSlots.Length; i++)
                {
                    if (QuickSlots[i] == item)
                    {
                        QuickSlots[i] = null;
                    }
                }
            }
        }

        InventoryChanged?.Invoke();
    }

    /// <summary>
    /// Get the item from the inventory using it's data reference.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public Item GetItem(ItemData item)
    {
        int hash = item.Guid.GetHashCode();
        if (_itemDictionary.ContainsKey(hash))
        {
            return _itemDictionary[hash];
        }

        return null;
    }

        /// <summary>
    /// Returns if the inventory contains the item.
    /// </summary>
    /// <param name="itemData">The item to look for.</param>
    /// <returns>True if there is an instance of the item, false otherwise.</returns>
    public bool HasItem(ItemData itemData)
    {
        if(itemData == null) return false;
        int hash = itemData.Guid.GetHashCode();
        return _itemDictionary.ContainsKey(hash);
    }
    
    /// <summary>
    /// Creates an item from the item data and it's class type.
    /// </summary>
    /// <param name="itemData"></param>
    /// <param name="quantity"></param>
    /// <returns></returns>
    private Item CreateItem(ItemData itemData, int quantity)
    {
        Item newItem;
        switch (itemData)
        {
            case ConsumableItemData data:
                if (data is RecoveryItemData)
                {
                    newItem = new RecoveryItem(data as RecoveryItemData, quantity);
                }
                else
                    newItem = new ConsumableItem(data, quantity);
                break;
            default:
                newItem = new Item(itemData, quantity);
                break;
        }
        return newItem;
    }

    /// <summary>
    /// Clears the inventory of all items.
    /// </summary>
    [Button("Clear Inventory")][FoldoutGroup("Debug")][GUIColor("Red")]
    private void ClearInventory()
    {
        _itemDictionary.Clear();
        Items.Clear();
        QuickSlots = new Item[2];
        InventoryChanged?.Invoke();
    }
}
