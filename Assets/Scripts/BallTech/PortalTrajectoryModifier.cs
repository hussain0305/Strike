using System.Collections.Generic;
using UnityEngine;

public class PortalTrajectoryModifier : ITrajectoryModifier
{
    public List<List<Vector3>> ModifyTrajectory(List<Vector3> trajectoryPoints)
    {
        List<List<Vector3>> trajectorySegments = new List<List<Vector3>>();
        List<Vector3> currentSegment = new List<Vector3>();

        Transform currentExitPortal = null;
        Transform currentEntryPortal = null;

        for (int i = 0; i < trajectoryPoints.Count - 1; i++)
        {
            Vector3 currentPoint = trajectoryPoints[i];
            Vector3 nextPoint = trajectoryPoints[i + 1];
            
            RaycastHit hit;
            bool portalHit = false;
            if (Physics.Linecast(currentPoint, nextPoint, out hit))
            {
                Portal portal = hit.collider.GetComponentInParent<Portal>();
                if (portal != null && portal.linkedPortal != null)
                {
                    portalHit = true;
                    currentSegment.Add(hit.point);
                    trajectorySegments.Add(new List<Vector3>(currentSegment));
                    
                    currentSegment.Clear();
                    Vector3 entryPointLocal = portal.transform.InverseTransformPoint(hit.point);
                    Vector3 exitPointWorld = portal.linkedPortal.transform.TransformPoint(entryPointLocal);
                    currentSegment.Add(exitPointWorld);

                    currentEntryPortal = portal.transform;
                    currentExitPortal = portal.linkedPortal.transform;

                    for (int j = i + 1; j < trajectoryPoints.Count; j++)
                    {
                        trajectoryPoints[j] = currentExitPortal.TransformPoint(
                            currentEntryPortal.InverseTransformPoint(trajectoryPoints[j])
                        );
                    }
                }
            }

            if (!portalHit)
            {
                currentSegment.Add(currentPoint);
            }
        }
        if (currentSegment.Count > 0)
        {
            currentSegment.Add(trajectoryPoints[^1]);
            trajectorySegments.Add(currentSegment);
        }
        return trajectorySegments;
    }
}
