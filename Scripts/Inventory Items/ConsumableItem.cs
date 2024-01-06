using System.Collections;
using System.Collections.Generic;
using Core.GameItems;
using UnityEngine;

[System.Serializable]
public class ConsumableItem : Item
{
    public bool Reusable => (DataReference as ConsumableItemData)?.Reusable ?? false;
    public List<VoidEventChannel> EventsToTrigger => (DataReference as ConsumableItemData)?.EventsToTrigger;
    public ConsumableItem(ConsumableItemData dataReference, int quantity = 1) : base(dataReference, quantity)
    {
    }
    
    public virtual void Use()
    {
        foreach(var voidEvent in EventsToTrigger)
        {
            voidEvent.RaiseEvent();
        }
    }
}
