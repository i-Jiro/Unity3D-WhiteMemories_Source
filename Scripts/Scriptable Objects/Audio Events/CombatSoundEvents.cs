using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "CombatSoundEvents", menuName = "Audio Events/Combat Sound Events")]
public class CombatSoundEvents : ScriptableObject
{
    [field: SerializeField] public EventReference Whoosh { get; private set; }
    [field: SerializeField] public EventReference UnarmedWhoosh { get; private set; }
    [field: SerializeField] public EventReference Hit { get; private set; }
    [field: SerializeField] public EventReference GroundHit { get; private set; }
}
