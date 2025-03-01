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

    private List<Vector3> trajectoryPoints;
    private List<Vector3> capturedTrajectoryPoints;
    private IContextProvider context;

    private int trajectoryDefinition = 10;
    private float gravity;
    private Tee tee;
    private Vector3 startPosition;
    
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

        trajectoryDefinition = context.GetTrajectoryDefinition();
        tee = context.GetTee();
        startPosition = tee.ballPosition.position;
        gravity = context.GetGravity();
    }
    
    public void Shoot()
    {
        trajectoryPoints = CalculateTrajectory();
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

    public void ResetBall<T>(T e)
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
        ball.position = startPosition;
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
    
    public List<Vector3> CalculateTrajectory()
    {
        List<Vector3> trajectoryPoints = new List<Vector3>();
        float timeStep = 0.1f;

        Vector2 spin = context.GetSpinVector();
        float launchForce = context.GetLaunchForce() / rb.mass;
        Vector3 initialVelocity = context.GetLaunchAngle() * Vector3.forward * launchForce;

        float sideSpin = spin.x;
        float topSpin = spin.y;

        float curveScale = Mathf.Clamp(spinEffect * Mathf.Abs(sideSpin), 0, curveClamp);
        float dipScale = -Mathf.Clamp(spinEffect * Mathf.Abs(topSpin), 0, dipClamp);

        float curveDuration = 2.0f;

        bool isCurving = true;
        Vector3 previousPoint;
        Vector3 currentPoint = Vector3.zero;
        Vector3 lastVelocity;
        float x = 0, y = 0, z = 0;

        for (int i = 0; i < trajectoryDefinition; i++)
        {
            float t = i * timeStep;
            previousPoint = currentPoint;
            currentPoint = new Vector3(x, y, z);
            lastVelocity = (currentPoint - previousPoint) / timeStep;

            if (isCurving)
            {
                x = startPosition.x + initialVelocity.x * t;
                z = startPosition.z + initialVelocity.z * t;
                y = startPosition.y + initialVelocity.y * t - 0.5f * gravity * t * t;

                float curveFactor = Mathf.Sin(t * Mathf.PI / curveDuration) * curveScale * Mathf.Sign(sideSpin);
                float dipFactor = Mathf.Sin(t * Mathf.PI / curveDuration) * dipScale * Mathf.Sign(topSpin);

                x += curveFactor;
                y += dipFactor;

                if (t >= curveDuration)
                {
                    isCurving = false;
                }
            }
            else
            {
                x = currentPoint.x + lastVelocity.x * timeStep;
                z = currentPoint.z + lastVelocity.z * timeStep;
                y = currentPoint.y + lastVelocity.y * timeStep - 0.5f * gravity * timeStep * timeStep;
            }

            trajectoryPoints.Add(new Vector3(x, y, z));

            if (y < 0)
            {
                break;
            }
        }

        return trajectoryPoints;
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