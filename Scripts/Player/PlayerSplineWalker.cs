using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Helper script to adjust the forward rotation and position of the player to allow movement on a spline.
/// This does not handle movement, only position and rotation. Actual forward movement should be handled by a different script.
/// Assumption: The spline is on the xz plane and the character is applying forces along it's forward (x-axis) vector. Y axis is unaffected.
/// </summary>

public class PlayerSplineWalker : SplineWalker
{
    [Header("Player Event Channel")]
    [SerializeField] private PlayerEventChannel _playerEventChannel;
    protected virtual void OnEnable()
    {
        if(_playerEventChannel)
            _playerEventChannel.PlayerPhysicsMoveStart += ApplySplinePositionAndRotation;
    }

    protected virtual void OnDisable()
    {
        if(_playerEventChannel)
            _playerEventChannel.PlayerPhysicsMoveStart -= ApplySplinePositionAndRotation;
    }
}
