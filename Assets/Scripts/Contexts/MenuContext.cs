using System.Collections.Generic;
using UnityEngine;

public class MenuContext : IContextProvider
{
    private Ball currentBall;
    private Transform aimTransform;
    private LineRenderer trajectory;
    private Tee tee;
    
    public Transform GetAimTransform()
    {
        return aimTransform;
    }

    public List<Vector3> GetTrajectory()
    {
        return SpoofTrajectory(currentBall.transform.position, currentBall.spinEffect);
    }
    
    public void SetBallState(BallState newState)
    {
        
    }

    public PinBehaviourPerTurn GetPinResetBehaviour()
    {
        return PinBehaviourPerTurn.Reset;
    }

    public Transform GetBallTeePosition()
    {
        return tee.ballPosition;
    }

    public List<Vector3> SpoofTrajectory(Vector3 startPosition, float spinEffect)
    {
        List<Vector3> trajectoryPoints = new List<Vector3>();
        float timeStep = 0.1f;

        float launchVelocity = 10f;
        Vector3 initialVelocity = trajectory.transform.forward * launchVelocity;

        Vector2 spin = new Vector2(Random.Range(-2f, 2f), Random.Range(-0.5f, 0.2f));
        float sideSpin = spin.x;
        float topSpin = spin.y;

        float randomCurveEffect = Random.Range(0f, 2f);
        float randomDipEffect = Random.Range(-0.5f, 0.2f);

        float curveScale = Mathf.Clamp(randomCurveEffect * Mathf.Abs(sideSpin), 0, spinEffect);
        float dipScale = -Mathf.Clamp(randomDipEffect * Mathf.Abs(topSpin), 0, spinEffect);

        float curveDuration = 2.0f;

        bool isCurving = true;
        Vector3 previousPoint;
        Vector3 currentPoint = Vector3.zero;
        Vector3 lastVelocity;
        float x = 0, y = 0, z = 0;

        for (int i = 0; i < 100; i++)
        {
            float t = i * timeStep;
            previousPoint = currentPoint;
            currentPoint = new Vector3(x, y, z);
            lastVelocity = (currentPoint - previousPoint) / timeStep;

            if (isCurving)
            {
                x = startPosition.x + initialVelocity.x * t;
                z = startPosition.z + initialVelocity.z * t;
                y = startPosition.y + initialVelocity.y * t - 0.5f * -Physics.gravity.y * t * t;

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
                y = currentPoint.y + lastVelocity.y * timeStep - 0.5f * -Physics.gravity.y * timeStep * timeStep;
            }

            trajectoryPoints.Add(new Vector3(x, y, z));

            if (y < 0)
            {
                break;
            }
        }

        return trajectoryPoints;
    }

    public void InitPreview(Ball _ball, Transform _aimTransform, LineRenderer _trajectory, Tee _tee)
    {
        currentBall = _ball;
        aimTransform = _aimTransform;
        trajectory = _trajectory;
        tee = _tee;
    }
    
    public void DrawTrajectory(Vector3[] trajectoryPoints)
    {
        trajectory.positionCount = trajectoryPoints.Length;
        trajectory.SetPositions(trajectoryPoints);

        trajectory.startWidth = 0.25f;
        trajectory.endWidth = 0.25f;
        // trajectory.material = new Material(Shader.Find("Sprites/Default")); // Basic material
        trajectory.startColor = Color.green;
        trajectory.endColor = Color.red;
    }
}
