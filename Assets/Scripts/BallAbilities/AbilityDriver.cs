using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilityDriver : MonoBehaviour
{
    public IContextProvider context;
    [HideInInspector]
    public Ball ball;
    public List<IBallAbilityModule> modules = new List<IBallAbilityModule>();
    public List<IBallAbilityUpdateableModule> updateModules = new List<IBallAbilityUpdateableModule>();
    
    private bool configured = false;
    
    public void Configure(Ball ownerBall, IContextProvider _context, List<IBallAbilityModule> _modules)
    {
        ball = ownerBall;
        context = _context;
        modules = _modules ?? new List<IBallAbilityModule>();
        updateModules = modules.OfType<IBallAbilityUpdateableModule>().ToList();
        
        foreach (var m in modules)
        {
            m.Initialize(ball, context);
        }
        
        configured = true;
    }

    void OnEnable()
    {
        EventBus.Subscribe<BallShotEvent>(OnBallShot);
        EventBus.Subscribe<ProjectilesSpawnedEvent>(OnProjectilesSpawned);
        EventBus.Subscribe<NextShotCuedEvent>(OnNextShotCued);
        EventBus.Subscribe<BallHitSomethingEvent>(OnBallHitSomething);
    }
    void OnDisable()
    {
        EventBus.Unsubscribe<BallShotEvent>(OnBallShot);
        EventBus.Unsubscribe<ProjectilesSpawnedEvent>(OnProjectilesSpawned);
        EventBus.Unsubscribe<NextShotCuedEvent>(OnNextShotCued);
        EventBus.Unsubscribe<BallHitSomethingEvent>(OnBallHitSomething);
    }

    private void Update()
    {
        if (!configured)
            return;
        
        foreach (var u in updateModules)
            u.OnUpdate();
    }

    private void OnBallShot(BallShotEvent e)
    {
        foreach (IBallAbilityModule module in modules)
        {
            module.OnBallShot(e);
        }
    }

    private void OnProjectilesSpawned(ProjectilesSpawnedEvent e)
    {
        foreach (IBallAbilityModule module in modules)
        {
            module.OnProjectilesSpawned(e);
        }
    }

    private void OnNextShotCued(NextShotCuedEvent e)
    {
        foreach (IBallAbilityModule module in modules)
        {
            module.OnNextShotCued(e);
        }
    }
    
    private void OnBallHitSomething(BallHitSomethingEvent e)
    {
        foreach (IBallAbilityModule module in modules)
        {
            module.OnHitSomething(e);
        }
    }

    private void OnDestroy()
    {
        if (modules != null)
        {
            foreach (var m in modules)
            {
                m.Cleanup();
            }
        }
    }
}
