using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Core.Animation;

public enum AttackType
{
    Light,
    Heavy,
}

[System.Serializable]
public class Attack : IFrameCheckHandler, IHitboxResponder
{
    private AttackData _data;
    
    private AnimationFrameData animationFrameData;
    private FrameChecker _frameChecker;

    private bool _isActive = false;
    private bool _inRecovery = false;
    private HitboxManager _hitboxManager;
    private HitboxController _hitbox;
    private bool _hadContact = false;

    public bool IsActive => _isActive;
    public bool InRecovery => _inRecovery;
    public bool HadContact => _hadContact;
    public FrameChecker FrameChecker => _frameChecker;
    public AnimationFrameData AnimationFrameData => animationFrameData;
    
    public List<AttackData> Gatlings => _data.Gatlings;
    public AttackType Type => _data.Type;
    public string Name => _data.Name;
    public float Damage => _data.BaseDamage;
    public bool SuperArmor => _data.SuperArmor;
    public int HitboxUid => _data.HitboxUid;
    public float HitstunDuration => _data.HitstunDuration;
    public float HitStopDuration => _data.HitstopDuration;
    public float BaseKnockback => _data.BaseKnockback;
    public float VerticalKnockup => _data.VerticalKnockup;
    public bool GatlingOnlyOnHit => _data.GatlingOnlyOnHit;


    public Attack(AttackData data, FrameChecker frameChecker, HitboxManager hitboxManager)
    {
        _data = data;
        _frameChecker = frameChecker;
        _hitboxManager = hitboxManager;
        _frameChecker.SetHandler(this);
    }
    
    public void Execute()
    {
        //Debug.Log("Execute attack.");
        _frameChecker.InitializeCheck();
        _hitbox = _hitboxManager.GetHitbox(HitboxUid);
        _hitbox.SetResponder(this);
        _isActive = true;
        _hadContact = false;
    }

    public void Tick()
    {
        if(_isActive == false){return;}
        _frameChecker.CheckFrames();
        _hitbox.Tick();
    }

    //Force stop the attack.
    public void Cancel()
    {
        _isActive = false;
        _hitbox.StopCheckingCollision();
    }
    
    public string GetAnimationStateName()
    {
        return _frameChecker.AnimationFrameData.AnimationStateName;
    }

    #region FRAME CHECKER CALLBACKS
    public void OnFirstFrame()
    {
        _inRecovery = false;
    }

    public void OnHitFrameStart()
    {
        _inRecovery = false;
        _hitbox.StartCheckingCollision();
        //Debug.Log("Hit frame start.");
    }

    public void OnHitFrameEnd()
    {
        _hitbox.StopCheckingCollision();
        //Debug.Log("Hit frame end.");
        _inRecovery = true;
    }

    public void OnLastFrame()
    {
        //Debug.Log("Last Frame");
        _inRecovery = false;
        _isActive = false;
    }
    #endregion
    
    //CALLBACK from HitboxController when making contact with another collider.
    public void CollisionedWith(Collider other, Vector3 direction)
    {
        _hadContact = true;
        _data.RaiseOnHitEvents();
        var root = other.transform.root.GetComponent<IDamageable>();
        root.TakeDamage(this, direction);
    }
}
