using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtboxManager : MonoBehaviour
{
    [SerializeField] private List<HurtboxController> _hurtboxes = new List<HurtboxController>();
    public List<HurtboxController> Hurtboxes => _hurtboxes;

    private void Start()
    {
        foreach (var hurtbox in _hurtboxes)
        {
            hurtbox.SetOwner(transform.root.gameObject);
        }
    }
    
    public void SetAllHurtboxesActive(bool active)
    {
        foreach (var hurtbox in _hurtboxes)
        {
            hurtbox.gameObject.SetActive(active);
        }
    }

    public void SetAllHurtboxMode(HurtboxMode mode)
    {
        foreach (var hurtbox in _hurtboxes)
        {
            hurtbox.Mode = mode;
        }
    }
}
