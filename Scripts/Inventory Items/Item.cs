using System.Collections;
using System.Collections.Generic;
using Core.GameItems;
using Sirenix.OdinInspector;
using Unity.Collections;
using UnityEngine;

namespace Core.GameItems
{
    /// <summary>
    /// Wrapper class for ItemData. This is the class that will be used in the inventory system.
    /// </summary>
    [System.Serializable]
    public class Item
    {
        [SerializeField] [Sirenix.OdinInspector.ReadOnly] private ItemData _dataReference;
        [field: SerializeField] public int CurrentStack { get; private set; }

        public string ID => _dataReference.Guid;
        public string DisplayName => _dataReference.DisplayName;
        public string Description => _dataReference.Description;
        public Sprite Icon => _dataReference.Icon;
        public bool Stackable => _dataReference.Stackable;
        public int MaxStack => _dataReference.MaxStack;
        public bool Discardable => _dataReference.Discardable;
        public ItemData DataReference => _dataReference;

        public Item(ItemData dataReference, int quantity = 1)
        {
            _dataReference = dataReference;
            CurrentStack = quantity;
        }
        
        public virtual void AddStack(int amount)
        {
            if (!Stackable) return;
            CurrentStack += amount;
            if (CurrentStack > MaxStack)
            {
                CurrentStack = MaxStack;
            }
        }
        
        public virtual void RemoveStack(int amount)
        {
            CurrentStack -= amount;
            if (CurrentStack < 0)
            {
                CurrentStack = 0;
            }
        }
    }
}
