using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class MovementAnimationSoundHandler : MonoBehaviour
{
    public void PlayFootstep()
    {
        AudioManager.PlayOneShot(AudioManager.Instance.Events.Footstep, transform.position);
    }

    public void PlayRoll()
    {
        AudioManager.PlayOneShotAttached(AudioManager.Instance.Events.Roll, transform.root.gameObject);
    }
}
