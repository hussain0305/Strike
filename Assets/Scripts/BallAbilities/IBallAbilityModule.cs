using UnityEngine;

public enum AbilityAxis {
    Spawn,
    Trajectory,
    Collision,
    MidFlight,
    Lifetime,
    OnHitEffect,
    AimModifier,
}

public interface IBallAbilityModule
{
    AbilityAxis Axis { get; }
    void Initialize(Ball ownerBall, IContextProvider context);
    void OnBallShot(BallShotEvent e);
    void OnProjectilesSpawned(ProjectilesSpawnedEvent e);
    void OnNextShotCued(NextShotCuedEvent e);
    void OnHitSomething(BallHitSomethingEvent e);
    void Cleanup();
}