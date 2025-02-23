using System;
using UnityEngine;

public abstract class BallAbility : MonoBehaviour
{
    protected Ball ball;
    protected IContextProvider context;
    
    public virtual void Initialize(Ball ownerBall, IContextProvider _context)
    {
        ball = ownerBall;
        context = _context;
        UnregisterFromEvents();
        RegisterToEvents();
    }
    
    public void OnEnable()
    {
        if (context == null)
        {
            return;
        }
        RegisterToEvents();
    }

    public void OnDisable()
    {
        if (context == null)
        {
            return;
        }
        UnregisterFromEvents();
    }

    public void RegisterToEvents()
    {
        context?.RegisterListener(GameEvent.BallShot, BallShot);
        context?.RegisterListener(GameEvent.NextShotCued, NextShotCued);
    }

    public void UnregisterFromEvents()
    {
        context?.UnregisterListener(GameEvent.BallShot, BallShot);
        context?.UnregisterListener(GameEvent.NextShotCued, NextShotCued);
    }

    public abstract void BallShot();
    public abstract void NextShotCued();
}
