using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using Core.Animation;
using Sirenix.OdinInspector;

[System.Serializable]
public class ActiveFrameRange
{
    [HideInInspector]
    public int TotalFrames;
    [MinMaxSlider(0, "@TotalFrames",true)]
    public Vector2 FrameRange;
    [HideInInspector] public bool CheckedStartFrame;
    [HideInInspector] public bool CheckedEndFrame;
    [HideInInspector] public bool Completed;
}

[System.Serializable]
public class FrameChecker
{
    [ReadOnly][Tooltip("Will auto-fill on inserting an animation clip.")]
    public int TotalFrames;
    
    private IFrameCheckHandler _frameCheckHandler;
    private AnimationFrameData animationFrameData;
    private List<ActiveFrameRange> _activeFrameRanges;
    private int _inactiveFrameCount = 0;
    private bool _isFirstFrame;

    private bool _isLastFrame;
    private bool _initialized = false;
    
    public AnimationFrameData AnimationFrameData => animationFrameData;
    
    public FrameChecker(List<ActiveFrameRange> activeFrameRanges, AnimationFrameData animationFrameData)
    {
        //Store directly the reference for live-editing.
        _activeFrameRanges = activeFrameRanges;
        this.animationFrameData = animationFrameData;
        _initialized = true;
    }

    public void SetHandler(IFrameCheckHandler handler)
    {
        _frameCheckHandler = handler;
    }
    
    //Reset flags. Call when the action starts, or when the animation ends.
    public void InitializeCheck()
    {
        _isFirstFrame = true;
        foreach (var range in _activeFrameRanges)
        {
            range.CheckedStartFrame = false;
            range.CheckedEndFrame = false;
            range.Completed = false;
        }
        _isLastFrame = false;
        _inactiveFrameCount = 0;
    }
    
    //Called in an update method.
    public void CheckFrames()
    {
        if(!_initialized){return;}
        if(_isLastFrame){return;}
        if(_isFirstFrame)
        {
            _isFirstFrame = false;
            _frameCheckHandler.OnFirstFrame();
        }
        
        //Waits until the animation is playing in the animator.
        if (!animationFrameData.IsActive())
        {
            //Failsafe if the targeted animation is not playing for 5 frames consecutively.
            //Band-aid solution. Most likely relating to animation transitions not properly reaching reaching final frame.
            _inactiveFrameCount++;
            if (_inactiveFrameCount != 5) return;
            /*
            Debug.LogWarning($"FrameChecker: Failsafe. Forcefully ending checks on" +
                             $" {animationFrameData.AnimationStateName}"); */
            _frameCheckHandler.OnLastFrame();
            _isLastFrame = true;
            return;
        }
        _inactiveFrameCount = 0; //Reset failsafe counter

        foreach (var range in _activeFrameRanges)
        {
            if(range.Completed){;continue;}
            if (!range.CheckedStartFrame && animationFrameData.BiggerOrEqualThanFrame((int)range.FrameRange.x))
            {
                range.CheckedStartFrame = true;
                _frameCheckHandler.OnHitFrameStart();
            }
            else if (!range.CheckedEndFrame && animationFrameData.BiggerOrEqualThanFrame((int)range.FrameRange.y))
            {
                range.CheckedEndFrame = range.Completed = true;
                _frameCheckHandler.OnHitFrameEnd();
            }
        }

        if (_isLastFrame || !animationFrameData.IsOnLastFrame()) return;
        _isLastFrame = true;
        _frameCheckHandler.OnLastFrame();
    }
    
    public void SetActiveFrameRanges(List<ActiveFrameRange> activeFrameRanges)
    {
        _activeFrameRanges = activeFrameRanges;
    }

    public void OnValidate()
    {
        //Converts float to int.
        foreach (var range in _activeFrameRanges)
        {
            range.TotalFrames = this.TotalFrames;
            range.FrameRange.x = Mathf.RoundToInt(range.FrameRange.x);
            range.FrameRange.y = Mathf.RoundToInt(range.FrameRange.y);
        }
    }

}
