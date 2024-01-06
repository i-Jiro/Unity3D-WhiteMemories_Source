using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "UI_SoundEvents", menuName = "Audio Events/UI Sound Events")]
public class UISoundEvents : ScriptableObject
{
    [field:SerializeField] public EventReference UIMove { get; private set; }
    [field:SerializeField] public EventReference UIOpen { get; private set; }
    [field:SerializeField] public EventReference UIClose { get; private set; }
}
