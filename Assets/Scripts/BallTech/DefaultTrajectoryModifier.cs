using System.Collections.Generic;
using UnityEngine;

public class DefaultTrajectoryModifier : ITrajectoryModifier
{
    public List<List<Vector3>> ModifyTrajectory(List<Vector3> trajectoryPoints)
    {
        return new List<List<Vector3>> { trajectoryPoints };
    }
}