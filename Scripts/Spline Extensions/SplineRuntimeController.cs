using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class SplineRuntimeController : MonoBehaviour
{
    [SerializeField] private SplineRuntimeSet _splineRuntimeSet;
    [SerializeField] private Spline _spline;

    private void Awake()
    {
        if(_spline == null)
            _spline = GetComponent<Spline>();
    }

    private void OnEnable()
    {
        _splineRuntimeSet.Add(_spline);
    }

    private void OnDisable()
    {
        _splineRuntimeSet.Remove(_spline);
    }
}
