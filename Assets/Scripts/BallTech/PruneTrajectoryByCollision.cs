using System.Collections.Generic;
using UnityEngine;

public static class TrajectoryPruner
{
    private static LayerMask collisionMask = LayerMask.GetMask("Ground", "Wall", "CollideWithBall");
    
    public static List<List<Vector3>> PruneByCollision(List<List<Vector3>> trajectorySegments)
    {
        var prunedList = new List<List<Vector3>>();
        for (int segmentIndex = 0; segmentIndex < trajectorySegments.Count; segmentIndex++)
        {
            var segment = trajectorySegments[segmentIndex];
            
            for (int i = 0; i < segment.Count - 1; i++)
            {
                Vector3 p0 = segment[i];
                Vector3 p1 = segment[i + 1];

                if (Physics.Linecast(p0, p1, out RaycastHit hit, collisionMask))
                {
                    var partialSegment = new List<Vector3>();
                    for (int k = 0; k <= i; k++)
                    {
                        partialSegment.Add(segment[k]);
                    }

                    partialSegment.Add(hit.point);

                    prunedList.Add(partialSegment);
                    return prunedList;
                }
            }

            prunedList.Add(new List<Vector3>(segment));
        }

        return prunedList;
    }
}


// float probeDistance = launchForce / 10f;
// if (Physics.SphereCast(currentPoint, Global.BALL_RADIUS, lastVelocity.normalized, out RaycastHit hit,
//         probeDistance, collisionMask))
// {
//     trajectoryPoints.Add(hit.point);
//     break;
// }
//
// return ModifyTrajectoryForDrawing(trajectorySegments);
