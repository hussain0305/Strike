using System.Collections.Generic;
using UnityEngine;

public struct SpoofedTrajectoryParameters
{
    public MinMaxFloat spinX;
    public MinMaxFloat spinY;
    public MinMaxFloat angleX;
    public MinMaxFloat angleY;
    public MinMaxFloat launchForce;

    public void CreateDefault()
    {
        spinX.Min = -0.25f;
        spinX.Max = 0.25f;

        spinY.Min = -0.5f;
        spinY.Max = -0.25f;
        
        angleX.Min = -10;
        angleX.Max = -30;
        
        angleY.Min = -20;
        angleY.Max = 20;

        launchForce.Min = 7;
        launchForce.Max = 9;
    }

    public void CreateFromBallProperties(BallProperties properties)
    {
        float previewSpin = Mathf.Min(properties.spin / 5, 1.5f);
        spinX = new MinMaxFloat(-previewSpin, previewSpin);
        spinY = new MinMaxFloat(-previewSpin, 0);
        bool hasSpin = previewSpin > 0.1f;
        angleX = new MinMaxFloat(hasSpin ? -10 : -45, hasSpin ? -30 : -60);
        angleY = new MinMaxFloat(hasSpin ? -20 : -30, hasSpin ? 20 : 30);
        launchForce = new MinMaxFloat(7, 9);
    }
}

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
    private BallState ballState;
    
    public Transform GetAimTransform()
    {
        return aimTransform;
    }

    public Transform GetBallParent()
    {
        return tee.transform;
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
        ballState = newState;
    }

    public BallState GetBallState()
    {
        return ballState;
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

    public void SpoofNewTrajectory(SpoofedTrajectoryParameters parameters)
    {
        spoofedSpinVector = new Vector2(Random.Range(parameters.spinX.Min, parameters.spinX.Max), Random.Range(parameters.spinY.Min, parameters.spinY.Max));

        spoofedLaunchAngle = Quaternion.Euler(Random.Range(parameters.angleX.Min, parameters.angleX.Max), Random.Range(parameters.angleY.Min, parameters.angleY.Max), 0f);
        aimTransform.rotation = spoofedLaunchAngle;

        float mass = currentBall ? currentBall.rb.mass : 1f;
        float force = Random.Range(parameters.launchForce.Min, parameters.launchForce.Max);
        spoofedLaunchForce = force * mass;
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
