using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RecoveryItem", menuName = "Items/Recovery Item")]
public class RecoveryItemData : ConsumableItemData
{ 
    public enum RecoveryType { HEALTH, STAMINA, EX, ALL }
    [Header("Recovery Item Data")]
    [SerializeField] private RecoveryType _recoveryType;
    public RecoveryType Recovery => _recoveryType;
    
    [SerializeField] private int _recoveryAmount;
    public int RecoveryAmount => _recoveryAmount;
}
