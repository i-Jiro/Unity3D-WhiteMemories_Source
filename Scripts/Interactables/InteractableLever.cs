using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class InteractableLever : MonoBehaviour
{
    [Required]
    [SerializeField] private Animator _animator;
    private static readonly int Switch = Animator.StringToHash("Switch");
    public UnityEvent OnInteract;

    public void Interact()
    {
        Animate();
        OnInteract?.Invoke();
    }
    
    public void Animate()
    {
        _animator.SetTrigger(Switch);
    }
}
