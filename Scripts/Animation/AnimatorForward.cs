using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

//Applies root motion to parent container object.
public class AnimatorForward : MonoBehaviour
{
    [SerializeField]
    private GameObject _rootParent;
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void OnAnimatorMove()
    {
        _rootParent.transform.position += _animator.deltaPosition;
    }
}
