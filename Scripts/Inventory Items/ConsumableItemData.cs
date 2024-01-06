using System.Collections;
using System.Collections.Generic;
using Core.GameItems;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "ConsumableItem", menuName = "Items/Consumable Item")]
public class ConsumableItemData : ItemData
{
    [Header("Consumable Item Data")]
    [Tooltip("If true, the item will not be consumed when used.")]
    [SerializeField] private bool _reusable = false;
    public bool Reusable => _reusable;
    [LabelText("Events to Trigger On Use")]
    public List<VoidEventChannel> EventsToTrigger = new List<VoidEventChannel>();
}
