using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BallHitSomethingEvent
{
    public Collision Collision { get; }
    public HashSet<PFXType> PfxTypes { get; }

    public BallHitSomethingEvent(Collision collision, HashSet<PFXType> pfxTypes)
    {
        Collision = collision;
        PfxTypes = pfxTypes;
    }
}

public class Ball : MonoBehaviour
{
    public Transform ball;
    public Rigidbody rb;

    public float spinEffect = 5f;
    public float curveClamp = 5f;
    public float dipClamp = 5f;
    public float groundLevel = 0.0f;

    protected IContextProvider context;
    protected ITrajectoryCalculator trajectoryCalculator;
    public ITrajectoryModifier trajectoryModifier;

    protected List<Vector3> trajectoryPoints;
    protected List<List<Vector3>> finalTrajectory;
    protected List<Vector3> capturedTrajectoryPoints;

    protected int trajectoryDefinition = 10;
    protected float gravity;
    protected Tee tee;
    protected Vector3 startPosition;
    
    [HideInInspector]
    public bool collidedWithSomething = false;

    private AbilityDriver abilityDriver;
    protected AbilityDriver AbilityDriver => abilityDriver ??= (GetComponent<AbilityDriver>() ?? gameObject.AddComponent<AbilityDriver>());
    
    private void OnEnable()
    {
        EventBus.Subscribe<ResetPreviewEvent>(ResetBall);
        EventBus.Subscribe<NextShotCuedEvent>(ResetBall);
        EventBus.Subscribe<ShotCompleteEvent>(ShotCompleted);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<ResetPreviewEvent>(ResetBall);
        EventBus.Unsubscribe<NextShotCuedEvent>(ResetBall);
        EventBus.Unsubscribe<ShotCompleteEvent>(ShotCompleted);
    }

    public void Initialize(IContextProvider _context, ITrajectoryModifier _trajectoryModifier, List<IBallAbilityModule> additionalModules = null)
    {
        context = _context;
        trajectoryModifier = _trajectoryModifier;
        
        trajectoryDefinition = context.GetTrajectoryDefinition();
        tee = context.GetTee();
        startPosition = tee.ballPosition.position;
        ball.rotation = Quaternion.Euler(GlobalConsts.BallTeeRotation);
        gravity = context.GetGravity();
        gameObject.AddComponent<PortalTraveler>();
        InitAbilityDriver(additionalModules);
        InitTrajectoryCalcualtor();
    }
    
    public void Shoot()
    {
        trajectoryPoints = trajectoryCalculator.CalculateTrajectory(startPosition);
        finalTrajectory = trajectoryModifier.ModifyTrajectory(trajectoryPoints);

        rb.isKinematic = true;
        StartCoroutine(FollowTrajectory());
        if (GameStateManager.Instance.CurrentGameState == GameState.InGame)
        {
            StartCoroutine(CaptureTrajectory());
        }
    }

    public List<Vector3> CalculateTrajectory()
    {
        if (trajectoryCalculator == null)
            return null;
        
        return trajectoryCalculator.CalculateTrajectory(startPosition);
    }

    private IEnumerator FollowTrajectory()
    {
        if (finalTrajectory == null || finalTrajectory.Count == 0)
            yield break;

        float timeStep = 0.1f;

        foreach (List<Vector3> trajectoryPart in finalTrajectory)
        {
            for (int i = 0; i < trajectoryPart.Count - 1; i++)
            {
                Vector3 currentPoint = trajectoryPart[i];
                Vector3 nextPoint = trajectoryPart[i + 1];

                if (collidedWithSomething || currentPoint.y <= groundLevel || nextPoint.y <= groundLevel)
                {
                    rb.isKinematic = false;
                    Vector3 finalVelocity = (nextPoint - currentPoint) / timeStep;
                    rb.linearVelocity = finalVelocity;
                    context.SetBallState(BallState.InPhysicsMotion);
                    yield break;
                }

                float elapsedTime = 0f;
                while (elapsedTime < timeStep)
                {
                    elapsedTime += Time.deltaTime;
                    ball.position = Vector3.Lerp(currentPoint, nextPoint, elapsedTime / timeStep);
                    yield return null;
                }
            }
        }

        rb.isKinematic = false;

        if (finalTrajectory.Count > 0 && finalTrajectory[^1].Count >= 2)
        {
            List<Vector3> lastPartOfTrajectory = finalTrajectory[^1];
            Vector3 finalVelocity = (lastPartOfTrajectory[^1] - lastPartOfTrajectory[^2]) / timeStep;
            rb.linearVelocity = finalVelocity;
        }

        context.SetBallState(BallState.InPhysicsMotion);
    }

    public IEnumerator CaptureTrajectory()
    {
        float timeRemaining = 10;
        capturedTrajectoryPoints = new List<Vector3>();
        while (timeRemaining > 0)
        {
            capturedTrajectoryPoints.Add(transform.position);
            timeRemaining -= 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void ShotCompleted(ShotCompleteEvent e)
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        RoundDataManager.Instance?.FinishLoggingShotInfo(capturedTrajectoryPoints);
    }
    
    public void ResetBall<T>(T e)
    {
        ResetBall();
    }
    
    public void ResetBall()
    {
        StopAllCoroutines();
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        ball.position = startPosition;
        ball.rotation = Quaternion.Euler(GlobalConsts.BallTeeRotation);
        collidedWithSomething = false;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (((1 << other.gameObject.layer) & Global.levelSurfaces) != 0)
        {
            EventBus.Publish(new BallHitSomethingEvent(other, new HashSet<PFXType> {PFXType.FlatHitEffect}));
        }
        else if(other.gameObject.GetComponent<Collectible>())
        {
            EventBus.Publish(new BallHitSomethingEvent(other, new HashSet<PFXType> {PFXType.FlatHitEffect, PFXType.HitPFX3D}));
        }
    }
    
    public virtual void InitAbilityDriver(List<IBallAbilityModule> additionalModules)
    {
        AbilityDriver.Configure(this, context, additionalModules);
    }
    
    public virtual void InitTrajectoryCalcualtor()
    {
        bool hasSniper = AbilityDriver.modules.Any(m => m is SniperModule);
        trajectoryCalculator = hasSniper ? new NonGravitationalTrajectory() : new GravitationalTrajectory();
        trajectoryCalculator.Initialize(context, this, gravity, trajectoryDefinition);
    }
}