using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Core.Animation
{
    [System.Serializable]
    public class AnimationFrameData
    {
        private Animator _animator;
        public AnimationClip Clip;
        public string AnimationStateName;
        public int AnimationLayerNumber;

        private int _totalFrames = 0;
        private int _animationFullNameHash;

        public int TotalFrames => _totalFrames;
        
        public AnimationFrameData(AnimationClip clip, Animator animator, string animationStateName, int animationLayerNumber)
        {
            Clip = clip;
            _animator = animator;
            AnimationStateName = animationStateName;
            AnimationLayerNumber = animationLayerNumber;
            Initialize();
        }

        public void Initialize()
        {
            if (!_animator.isActiveAndEnabled) return;
            //TODO: Hardcoded the substate name for testing animation workflow. Change this later to be changeable variable.
            string name = _animator.GetLayerName(AnimationLayerNumber) + "." + "Attacks"+ "." + AnimationStateName;
            _animationFullNameHash = Animator.StringToHash(name);
        }

        public void SetTotalFrame(int frameNumber)
        {
            _totalFrames = frameNumber;
        }

        public bool IsActive()
        {
            return _animator.GetCurrentAnimatorStateInfo(AnimationLayerNumber).fullPathHash == _animationFullNameHash;
        }

        private double PercentageOnFrame(int frameNumber)
        {
            return (double)frameNumber / (double)_totalFrames;
        }

        public bool BiggerOrEqualThanFrame(int frameNumber)
        {
            double percentage = _animator.GetCurrentAnimatorStateInfo(AnimationLayerNumber).normalizedTime;
            return (percentage >= PercentageOnFrame(frameNumber));
        }

        public bool IsOnLastFrame()
        {
            double percentage = _animator.GetCurrentAnimatorStateInfo(AnimationLayerNumber).normalizedTime;
            return (percentage > PercentageOnFrame(_totalFrames - 1));
        }

        public void SetAnimator(Animator animator)
        {
            _animator = animator;
        }

    }
}
