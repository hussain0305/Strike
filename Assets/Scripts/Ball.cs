using System;
using System.Collections;
using System.Collections.Generic;
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

    private Vector3 teePosition;
    private List<Vector3> trajectoryPoints;
    private List<Vector3> capturedTrajectoryPoints;
    private IContextProvider context;
    
    [HideInInspector]
    public bool collidedWithSomething = false;

    private void OnEnable()
    {
        EventBus.Subscribe<ResetPreviewEvent>(ResetBall);
        EventBus.Subscribe<NextShotCuedEvent>(ResetBall);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<ResetPreviewEvent>(ResetBall);
        EventBus.Unsubscribe<NextShotCuedEvent>(ResetBall);
    }

    public void Initialize(IContextProvider _context)
    {
        context = _context;
        foreach (BallAbility ability in GetComponentsInChildren<BallAbility>())
        {
            ability.Initialize(this, context);
        }
        teePosition = transform.position;
    }
    
    public void Shoot()
    {
        trajectoryPoints = context.GetTrajectory();
        rb.isKinematic = true;
        StartCoroutine(FollowTrajectory());
        if (GameStateManager.Instance.CurrentGameState == GameStateManager.GameState.InGame)
        {
            StartCoroutine(CaptureTrajectory());
        }
    }

    IEnumerator FollowTrajectory()
    {
        if (trajectoryPoints == null || trajectoryPoints.Count == 0)
            yield break;

        float timeStep = 0.1f;

        for (int i = 0; i < trajectoryPoints.Count - 1; i++)
        {
            Vector3 currentPoint = trajectoryPoints[i];
            Vector3 nextPoint = trajectoryPoints[i + 1];

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

        rb.isKinematic = false;

        if (trajectoryPoints.Count >= 2)
        {
            Vector3 finalVelocity = (trajectoryPoints[^1] - trajectoryPoints[^2]) / timeStep;
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

    public void ResetBall(ResetPreviewEvent e)
    {
        ResetBall();
    }
    
    public void ResetBall(NextShotCuedEvent e)
    {
        ResetBall();
    }
    
    public void ResetBall()
    {
        StopAllCoroutines();
        RoundDataManager.Instance?.FinishLoggingShotInfo(capturedTrajectoryPoints);
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        ball.position = teePosition;
        ball.rotation = Quaternion.identity;
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
}

/*
public float speedMultiplier = 1.0f; // Adjustable speed factor
IEnumerator FollowTrajectory_ConstantSpeed()
{
    if (trajectoryPoints == null || trajectoryPoints.Count < 2)
        yield break;

    // Compute cumulative squared distances to avoid costly sqrt operations
    List<float> cumulativeSqrDistances = new List<float> { 0f };
    float totalSqrDistance = 0f;

    for (int i = 1; i < trajectoryPoints.Count; i++)
    {
        totalSqrDistance += (trajectoryPoints[i] - trajectoryPoints[i - 1]).sqrMagnitude;
        cumulativeSqrDistances.Add(totalSqrDistance);
    }

    float baseSpeed = totalSqrDistance / (trajectoryPoints.Count * 0.1f); // Base speed
    float adjustedSpeed = baseSpeed * speedMultiplier; // Apply user-defined speed factor
    float travelSqrDistance = 0f;
    int index = 0;

    while (travelSqrDistance < totalSqrDistance)
    {
        // Find the segment in which the current distance falls
        while (index < cumulativeSqrDistances.Count - 1 && travelSqrDistance > cumulativeSqrDistances[index + 1])
        {
            index++;
        }

        if (index >= trajectoryPoints.Count - 1)
            break;

        float segmentStart = cumulativeSqrDistances[index];
        float segmentEnd = cumulativeSqrDistances[index + 1];
        float segmentFraction = Mathf.InverseLerp(segmentStart, segmentEnd, travelSqrDistance);

        // Move the ball smoothly along the segment
        ball.position = Vector3.Lerp(trajectoryPoints[index], trajectoryPoints[index + 1], segmentFraction);

        // Stop if we hit something or touch the ground
        if (collidedWithSomething || ball.position.y <= groundLevel)
        {
            rb.isKinematic = false;
            Vector3 finalVelocity = (trajectoryPoints[index + 1] - trajectoryPoints[index]) / 0.1f;
            rb.velocity = finalVelocity;
            GameManager.BallState = BallState.InPhysicsMotion;
            yield break;
        }

        // Advance along the trajectory using squared distance for efficiency
        travelSqrDistance += adjustedSpeed * Time.deltaTime;
        yield return null;
    }

    rb.isKinematic = false;

    // Set final velocity when transitioning to physics motion
    if (trajectoryPoints.Count >= 2)
    {
        Vector3 finalVelocity = (trajectoryPoints[^1] - trajectoryPoints[^2]) / 0.1f;
        rb.velocity = finalVelocity;
    }

    GameManager.BallState = BallState.InPhysicsMotion;
}
*/