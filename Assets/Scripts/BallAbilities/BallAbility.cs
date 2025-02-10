using System;
using UnityEngine;

public abstract class BallAbility : MonoBehaviour
{
    protected Ball ball;

    private void Awake()
    {
        ball = GetComponent<Ball>();
        if (!GameManager.IsGameplayActive)
        {
            enabled = false;
        }
    }
}
