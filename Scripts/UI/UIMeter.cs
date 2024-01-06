using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMeter : MonoBehaviour
{
    [SerializeField] protected Image _meterImage;
    protected float _progress = 1f;

    public virtual void ChangeProgress(float progress)
    {
        _progress = progress;
        _meterImage.fillAmount = _progress;
    }
}
