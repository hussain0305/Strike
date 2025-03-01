using System.Collections.Generic;
using UnityEngine;

public class SniperBall : Ball
{
    public override List<Vector3> CalculateTrajectory()
    {
        gravity = 0;
        trajectoryPoints = new List<Vector3>();
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
                y = startPosition.y + initialVelocity.y * t;

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
                y = currentPoint.y + lastVelocity.y * timeStep;
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
