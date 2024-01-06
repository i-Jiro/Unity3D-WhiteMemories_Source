using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSLimit : MonoBehaviour
{
    public int targetFPS = 60;
    void Start()
    {
        Application.targetFrameRate = targetFPS;
    }

    private void OnValidate()
    {
        Application.targetFrameRate = targetFPS;
    }
}
