using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerStartLocation : MonoBehaviour
{
    [InfoBox("Player will be moved here on scene start.")]
    [Header("Spline")]
    [Required]
    [SerializeField] private Spline _spline;
    [SerializeField] [Range(0, 1)] private float _pointOnSpline;
    [InfoBox("Toggle the usage of exit points. Useful for loading player into a specific spot once on game start.")]
    [SerializeField] bool _useExitPoint = false;
    [SerializeField] string _exitPointName = "Default";

    private void OnValidate()
    {
        if(_spline == null) return;
        if(_spline.AnchorPoints.Count < 3) return;
        var t = _pointOnSpline * ((float)_spline.AnchorPoints.Count - 2);
        var point = _spline.CalculatePoint(t);
        transform.position = point;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (_useExitPoint)
        {
            //If the exit point key does not match, return.
            if (PlayerPrefs.HasKey("LastExitPoint") && PlayerPrefs.GetString("LastExitPoint") != _exitPointName)
            {
                return;
            }
        }
        
        PlayerBaseController.Instance.transform.position = transform.position;
        var splineWalker = PlayerBaseController.Instance.GetComponent<SplineWalker>();
        splineWalker.SetSpline(_spline);
    }
}
