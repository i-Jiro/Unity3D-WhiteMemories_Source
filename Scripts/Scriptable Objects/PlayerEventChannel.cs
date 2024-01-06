using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Player Event Channel", menuName = "Player/Player Event Channel", order = 1)]
public class PlayerEventChannel : ScriptableObject
{
    public delegate void PlayerAttackEventHandler(PlayerAttackController.AttackEventContext context);
    public event PlayerAttackEventHandler PlayerAttackStarted;
    public delegate void PlayerAttackEndEventHandler();
    public event PlayerAttackEndEventHandler PlayerAttackEnded;
    public delegate void PlayerPhysicsMoveStartEventHandler();
    public event PlayerPhysicsMoveStartEventHandler PlayerPhysicsMoveStart;
    public delegate void PlayerDodgeEventHandler();
    public event PlayerDodgeEventHandler PlayerDodged;
    public delegate void PlayerFocusStartEventHandler();
    public event PlayerFocusStartEventHandler PlayerFocusStarted;
    public delegate void PlayerFocusEndEventHandler();
    public event PlayerFocusEndEventHandler PlayerFocusEnded;
    public delegate void PlayerHitStunStartEventHandler();
    public event PlayerHitStunStartEventHandler PlayerHitStunStarted;
    public delegate void PlayerHitStunEndEventHandler();
    public event PlayerHitStunEndEventHandler PlayerHitStunEnded;
    
    public void RaisePlayerHitStunStart()
    {
        PlayerHitStunStarted?.Invoke();
    }
    
    public void RaisePlayerHitStunEnd()
    {
        PlayerHitStunEnded?.Invoke();
    }
    
    public void RaisePlayerAttacked(PlayerAttackController.AttackEventContext context)
    {
        PlayerAttackStarted?.Invoke(context);
    }
    
    public void RaisePlayerAttackEnded()
    {
        PlayerAttackEnded?.Invoke();
    }
    
    public void RaisePlayerDodged()
    {
        PlayerDodged?.Invoke();
    }
    
    public void RaisePlayerFocusStart()
    {
        PlayerFocusStarted?.Invoke();
    }
    
    public void RaisePlayerFocusEnd()
    {
        PlayerFocusEnded?.Invoke();
    }
    
    public void RaisePhysicsMoveStart()
    {
        PlayerPhysicsMoveStart?.Invoke();
    }
}
