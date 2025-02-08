using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [HideInInspector]
    public bool collidedWithSomething = false;
    
    public delegate void BallHitSomething(Collision collision, HashSet<PFXType> pfxTypes);
    public static event BallHitSomething OnBallHitSomething;

    private void Awake()
    {
        teePosition = transform.position;
    }

    public void Shoot()
    {
        trajectoryPoints = GameManager.Instance.CalculateTrajectoryPoints();
        RoundDataManager.Instance.StartLoggingShotInfo();
        StartCoroutine(CaptureTrajectory());
        rb.isKinematic = true;
        GameManager.BallState = BallState.InControlledMotion;
        StartCoroutine(FollowTrajectory());
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
                GameManager.BallState = BallState.InPhysicsMotion;
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

        GameManager.BallState = BallState.InPhysicsMotion;
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

    public void ResetBall()
    {
        StopAllCoroutines();
        RoundDataManager.Instance.FinishLoggingShotInfo(capturedTrajectoryPoints);
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
            OnBallHitSomething?.Invoke(other, new HashSet<PFXType> {PFXType.FlatHitEffect});
        }
        else if(other.gameObject.GetComponent<Collectible>())
        {
            OnBallHitSomething?.Invoke(other, new HashSet<PFXType> {PFXType.FlatHitEffect, PFXType.HitPFX3D});
        }
    }
}
