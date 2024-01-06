using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Wrapper class for recovery based item data.
/// </summary>
[System.Serializable]
public class RecoveryItem : ConsumableItem
{
    public RecoveryItemData.RecoveryType RecoveryType => (DataReference as RecoveryItemData)?.Recovery ?? RecoveryItemData.RecoveryType.HEALTH;
    public float RecoveryAmount => (DataReference as RecoveryItemData)?.RecoveryAmount ?? 0;

    public RecoveryItem(ConsumableItemData dataReference, int quantity = 1) : base(dataReference, quantity) { }

    public override void Use()
    {
        base.Use();
        if(PlayerBaseController.Instance == null) return;
        switch (RecoveryType)
        {
            case RecoveryItemData.RecoveryType.HEALTH:
                PlayerBaseController.Instance.RecoverHealth(RecoveryAmount);
                break;
            case RecoveryItemData.RecoveryType.STAMINA:
                PlayerBaseController.Instance.RecoverStamina(RecoveryAmount);
                break;
            case RecoveryItemData.RecoveryType.EX:
                PlayerBaseController.Instance.RecoverEX(RecoveryAmount);
                break;
            case RecoveryItemData.RecoveryType.ALL:
                PlayerBaseController.Instance.RecoverHealth(RecoveryAmount);
                PlayerBaseController.Instance.RecoverStamina(RecoveryAmount);
                PlayerBaseController.Instance.RecoverEX(RecoveryAmount);
                break;
        }
    }
}
