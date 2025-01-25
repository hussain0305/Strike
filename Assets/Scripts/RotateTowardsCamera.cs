using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateTowardsCamera : RotateTowardsTarget
{
    private void Awake()
    {
        target = Camera.main.transform;
    }
}
