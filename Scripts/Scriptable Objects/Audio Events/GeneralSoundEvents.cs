using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "GeneralSoundEvents", menuName = "Audio Events/General Sound Events")]
public class GeneralSoundEvents : ScriptableObject
{
    [field: SerializeField] public EventReference Footstep { get; private set; }
    [field: SerializeField] public EventReference Roll { get; private set; }
}

