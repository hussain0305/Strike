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
        EventBus.Subscribe<BallShotEvent>(BallShot);
        EventBus.Subscribe<NextShotCuedEvent>(NextShotCued);
    }

    public void UnregisterFromEvents()
    {
        EventBus.Unsubscribe<BallShotEvent>(BallShot);
        EventBus.Unsubscribe<NextShotCuedEvent>(NextShotCued);
    }

    public abstract void BallShot(BallShotEvent e);
    public abstract void NextShotCued(NextShotCuedEvent e);
}
