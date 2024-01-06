using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerAction
{
    public enum ActionType
    {
        Down,
        Back,
        Forward,
        Up,
        DownBack,
        DownForward,
        Jump,
        LightAttack,
        HeavyAttack
    }
    
    [SerializeField] private ActionType _type;
    [SerializeField] private float _timePerformed;
    
    public ActionType Type => _type;
    public float TimePerformed => _timePerformed;
    
    public PlayerAction(ActionType actionType, float timePerformed)
    {
        _type = actionType;
        _timePerformed = timePerformed;
    }
}
