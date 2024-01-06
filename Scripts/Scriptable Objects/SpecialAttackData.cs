using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "New Special Attack", menuName = "Attack/Special Attack Data", order = 3)]
public class SpecialAttackData : AttackData
{
    [PropertyOrder(-1)]
    [Title("Special Attack Properties")]
    [Tooltip("The sequence of actions that must be performed to trigger this special attack.")]
    [InfoBox("The sequence of actions that must be performed to trigger this special attack.\n Sequence needs to be at least 2 actions long and end with an attack button.")]
    [RequiredListLength(2,null)]
    public PlayerAction.ActionType[] Sequence;
}
