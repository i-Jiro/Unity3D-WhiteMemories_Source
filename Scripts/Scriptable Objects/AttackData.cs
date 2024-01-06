using System;
using System.Collections;
using System.Collections.Generic;
using Core.Animation;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Attack Data", menuName = "Attack/Attack Data", order = 2)]
public class AttackData : ScriptableObject
{
    [Tooltip("A unique name for this attack. Used to identify the attack in code.")]
    [SerializeField] private string _name;
    [SerializeField] private AttackType _type;
    [SerializeField] private float _baseDamage;
    [SerializeField] private bool _superArmor;
    [Header("Hitstun and Hitstop")]
    [SerializeField] private float _hitstunDuration;
    [SerializeField] private float _hitstopDuration;
    [Header("Knockback")]
    [SerializeField] private float _baseKnockback;
    [SerializeField] private float _verticalKnockup;
    [LabelText("Unique Hitbox Identifier")]
    [SerializeField] private int _hitboxUid;
    [SerializeField] List<VoidEventChannel> _onHitEvents = new List<VoidEventChannel>();
    [Tooltip("Attacks that can be chained into from this attack. Leave empty if this attack cannot be chained.")]
    [InfoBox("Attacks that can be chained into from this attack. Leave empty if this attack cannot be chained.")]
    public List<AttackData> Gatlings = new List<AttackData>();
    [SerializeField] private bool _gatlingOnlyOnHit = false;
    [Title("Frame Data")]
    [ReadOnly][SerializeField]
    private int _totalFrames = 0;
    private int _activeFrames = 0;
    private int _recoveryFrames = 0;
    [Tooltip("Animation clip to check the frame data.")]
    [Required("Animation clip required!")]
    [InlineEditor(InlineEditorModes.LargePreview,
        PreviewWidth = 300, 
        DrawGUI = true )]
    public AnimationClip Clip;
    [Tooltip("The name of the animation state in the animator.")]
    [Required("Animation state name required! Should match the state name in the animator.")]
    public string AnimationStateName;
    [Tooltip("The layer number of the animation state in the animator.")]
    public int AnimationLayerNumber;
    [Tooltip("The frame ranges that will trigger hitboxes.")]
    [InfoBox("The frame ranges that will trigger hitboxes.")]
    [SerializeField] public List<ActiveFrameRange> ActiveFrameRanges = new List<ActiveFrameRange>();
    
    public String Name => _name;
    public AttackType Type => _type;
    public float BaseDamage => _baseDamage;
    public float BaseKnockback => _baseKnockback;
    public float VerticalKnockup => _verticalKnockup;
    public int HitboxUid => _hitboxUid;
    public bool SuperArmor => _superArmor;
    public float HitstunDuration => _hitstunDuration;
    public float HitstopDuration => _hitstopDuration;
    public bool GatlingOnlyOnHit => _gatlingOnlyOnHit;

    private void OnValidate()
    {
        if (Clip != null)
        {
            _totalFrames = Mathf.RoundToInt(Clip.length * Clip.frameRate);
        }
        foreach (var range in ActiveFrameRanges)
        {
            range.TotalFrames = _totalFrames;
            range.FrameRange.x = Mathf.RoundToInt(range.FrameRange.x);
            range.FrameRange.y = Mathf.RoundToInt(range.FrameRange.y);
        }
    }
    
    public void RaiseOnHitEvents()
    {
        foreach (var onHitEvent in _onHitEvents)
        {
            onHitEvent.RaiseEvent();
        }
    }

    public Attack CreateAttack(Animator animator, HitboxManager hitboxManager)
    {
        //Stores data relating about the frames in the animation clip.
        var animationFrameData = new AnimationFrameData(Clip, animator, AnimationStateName, AnimationLayerNumber);
        animationFrameData.SetTotalFrame(_totalFrames);
        //Checks information about the frames in the animation clip.
        var frameChecker = new FrameChecker(this.ActiveFrameRanges, animationFrameData);
        //Stores data and game logic relating to the attack.
        var attack = new Attack(this, frameChecker, hitboxManager);
        return attack;
    }
}
