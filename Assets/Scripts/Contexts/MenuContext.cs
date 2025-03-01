using System.Collections.Generic;
using UnityEngine;

public class MenuContext : IContextProvider
{
    private Ball currentBall;
    private Transform aimTransform;
    private LineRenderer trajectory;
    private Tee tee;
    private BallPreviewController previewController;

    private Vector2 spoofedSpinVector;
    private Quaternion spoofedLaunchAngle;
    private float spoofedLaunchForce;
    
    private float gravity;
    
    public Transform GetAimTransform()
    {
        return aimTransform;
    }
    
    public Vector2 GetSpinVector()
    {
        return spoofedSpinVector;
    }
    
    public Quaternion GetLaunchAngle()
    {
        return spoofedLaunchAngle;
    }

    public float GetLaunchForce()
    {
        return spoofedLaunchForce;
    }

    public int GetTrajectoryDefinition()
    {
        return 100;
    }

    public void SetBallState(BallState newState)
    {
        
    }

    public float GetGravity()
    {
        return gravity;
    }
    
    public Tee GetTee()
    {
        return tee;
    }

    public PinBehaviourPerTurn GetPinResetBehaviour()
    {
        return PinBehaviourPerTurn.Reset;
    }

    public void SpoofNewTrajectory()
    {
        spoofedSpinVector = new Vector2(Random.Range(-0.25f, 0.25f), Random.Range(-0.5f, -0.25f));
        spoofedLaunchAngle = Quaternion.identity;
        spoofedLaunchForce = Random.Range(6f, 7f) / (currentBall ? currentBall.rb.mass : 1);
        aimTransform.rotation = spoofedLaunchAngle;
    }

    public void SetSpoofedSpinVector(Vector2 _spinVector)
    {
        spoofedSpinVector = _spinVector;
    }

    public void SetSpoofedLaunchAngle(Quaternion _launchAngle)
    {
        spoofedLaunchAngle = _launchAngle;
        aimTransform.rotation = spoofedLaunchAngle;
    }

    public void SetSpoofedLaunchForce(float _launchForce)
    {
        spoofedLaunchForce = _launchForce;
    }
    
    public void InitPreview(Ball _ball, BallPreviewController _previewController)
    {
        previewController = _previewController;
        currentBall = _ball;
        aimTransform = previewController.aimTransform;
        trajectory = previewController.trajectory;
        tee = previewController.tee;
        gravity = -Physics.gravity.y;
    }
    
    public void DrawTrajectory(Vector3[] trajectoryPoints)
    {
        trajectory.positionCount = trajectoryPoints.Length;
        trajectory.SetPositions(trajectoryPoints);
        // trajectory.material = new Material(Shader.Find("Sprites/Default")); // Basic material
        trajectory.startColor = Color.green;
        trajectory.endColor = Color.red;
    }
}
