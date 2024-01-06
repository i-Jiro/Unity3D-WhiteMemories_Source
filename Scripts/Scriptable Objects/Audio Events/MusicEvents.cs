using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Music", menuName = "Audio Events/Music Events")]
public class MusicEvents : ScriptableObject
{
    [LabelText("Overworld Theme BGM")]
    [field:SerializeField] public EventReference OverWorldThemeBGM { get; private set; }
}
