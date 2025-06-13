using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class BallHitSomethingEvent
{
    public Vector3 ImpactVelocity;
    public Collision Collision { get; }
    public HashSet<PFXType> PfxTypes { get; }

    public BallHitSomethingEvent(Collision collision, HashSet<PFXType> pfxTypes, Vector3 impactVelocity)
    {
        Collision = collision;
        PfxTypes = pfxTypes;
        ImpactVelocity = impactVelocity;
    }
}

public class Ball : MonoBehaviour, ISwitchTrigger
{
    public Transform ball;
    public Rigidbody rb;

    public float spinEffect = 5f;
    public float curveClamp = 5f;
    public float dipClamp = 5f;
    public float groundLevel = 0.0f;
    
    public AudioSource wooshSFX;
    public AudioSource bounceSFX;

    protected IContextProvider context;
    protected ITrajectoryCalculator trajectoryCalculator;
    public ITrajectoryModifier trajectoryModifier;

    protected List<Vector3> trajectoryPoints;
    protected List<List<Vector3>> finalTrajectory;
    protected List<Vector3> capturedTrajectoryPoints;
    public List<Vector3> CapturedTrajectoryPoints => capturedTrajectoryPoints;

    protected int trajectoryDefinition = 10;
    protected float gravity;
    protected Tee tee;
    protected Vector3 startPosition;
    
    [HideInInspector]
    public bool collidedWithSomething = false;
    [HideInInspector]
    public bool shouldResumePhysics = true;
    [HideInInspector]
    public bool haltBall = false;

    public BallState BallState
    {
        get
        {
            if(context != null)
                return context.GetBallState();

            return BallState.OnTee;
        }
    }

    public bool IsInControlledMotion => BallState == BallState.InControlledMotion;
    public bool IsInPhysicsMotion => BallState == BallState.InPhysicsMotion;

    private AbilityDriver abilityDriver;
    private CollisionForce collisionForce;
    protected AbilityDriver AbilityDriver => abilityDriver ??= (GetComponent<AbilityDriver>() ?? gameObject.AddComponent<AbilityDriver>());
    private Vector3 lastKnownVelocity;
    public Vector3 LastKnownVelocity => lastKnownVelocity;
    
    private GameStateManager gameStateManager;
    
    [InjectOptional]
    private RoundDataManager roundDataManager;
    [InjectOptional]
    private GameManager gameManager;
    public GameManager GameManager => gameManager;
    
    [Inject]
    public void Construct(GameStateManager _gameStateManager)
    {
        gameStateManager = _gameStateManager;
    }

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

    public void Initialize(IContextProvider _context, ITrajectoryModifier _trajectoryModifier, BallProperties ballProperties, List<IBallAbilityModule> additionalModules = null)
    {
        context = _context;
        trajectoryModifier = _trajectoryModifier;

        spinEffect = ballProperties.spin;
        curveClamp = spinEffect;
        dipClamp = spinEffect;
        rb.mass = ballProperties.weight;
        
        trajectoryDefinition = context.GetTrajectoryDefinition();
        tee = context.GetTee();
        startPosition = tee.ballPosition.position;
        ball.rotation = Quaternion.Euler(GlobalConsts.BallTeeRotation);
        gravity = context.GetGravity();
        gameObject.AddComponent<PortalTraveler>();
        collisionForce = GetComponent<CollisionForce>();
        collisionForce?.Initialize(this, _context is InGameContext);
        InitAbilityDriver(additionalModules);
        InitTrajectoryCalcualtor();
        AdjustEffectsIfNeeded();
    }
    
    public void Shoot()
    {
        trajectoryPoints = trajectoryCalculator.CalculateTrajectory(startPosition);
        finalTrajectory = trajectoryModifier.ModifyTrajectory(trajectoryPoints);

        rb.isKinematic = true;
        StartCoroutine(FollowTrajectory());
        if (gameStateManager.CurrentGameState == GameState.InGame)
        {
            StartCoroutine(CaptureTrajectory());
        }
        
        wooshSFX.enabled = true;
        wooshSFX.Play();
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
                while (haltBall)
                {
                    yield return null;
                }
                Vector3 currentPoint = trajectoryPart[i];
                Vector3 nextPoint = trajectoryPart[i + 1];
                lastKnownVelocity = (nextPoint - currentPoint) / timeStep;

                if (collidedWithSomething || currentPoint.y <= groundLevel || nextPoint.y <= groundLevel)
                {
                    if (shouldResumePhysics)
                    {
                        rb.isKinematic = false;
                        lastKnownVelocity = (nextPoint - currentPoint) / timeStep;
                        rb.linearVelocity = lastKnownVelocity;
                        context.SetBallState(BallState.InPhysicsMotion);
                    }
                    yield break;
                }

                float elapsedTime = 0f;
                while (elapsedTime < timeStep)
                {
                    elapsedTime += Time.deltaTime;
                    ball.position = Vector3.Lerp(currentPoint, nextPoint, elapsedTime / timeStep);
                    ball.transform.Rotate(Vector3.up, 540 * Time.deltaTime, Space.Self);
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
        shouldResumePhysics = true;
        
        wooshSFX.Stop();
        wooshSFX.enabled = false;
    }

    private void OnCollisionEnter(Collision other)
    {
        Collectible collectible = other.gameObject.GetComponent<Collectible>();
        if (((1 << other.gameObject.layer) & Global.LevelSurfaces) != 0)
        {
            BallBounced();
            EventBus.Publish(new BallHitSomethingEvent(other, new HashSet<PFXType> {PFXType.FlatHitEffect}, lastKnownVelocity));
        }
        else if(collectible)
        {
            // collisionForce?.AddForceOnCollectibleHit(other.gameObject.GetComponent<Rigidbody>(), other.GetContact(0).point, lastKnownVelocity);
            EventBus.Publish(new BallHitSomethingEvent(other, new HashSet<PFXType> {PFXType.FlatHitEffect, 
                collectible.type == CollectibleType.Danger ? PFXType.HitPFXDanger : PFXType.HitPFXCollectible}, lastKnownVelocity));
        }       
        else if (((1 << other.gameObject.layer) & Global.StickySurfaces) != 0)
        {
            BallBounced();
            collidedWithSomething = true;
            EventBus.Publish(new BallHitSomethingEvent(other, new HashSet<PFXType> {PFXType.FlatHitEffect, PFXType.HitPFXObstacle}, lastKnownVelocity));
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

    public void AdjustEffectsIfNeeded()
    {
        if (context is MenuContext menuCtx)
        {
            GetComponentInChildren<TrailRenderer>()?.gameObject.SetActive(false);
            GetComponentInChildren<ParticleSystem>()?.gameObject.SetActive(false);
        }
    }

    public void BallBounced()
    {
        bounceSFX.Play();
    }
}