using System.Collections.Generic;
using UnityEngine;

public interface ITrajectoryModifier
{
    List<List<Vector3>> ModifyTrajectory(List<Vector3> trajectoryPoints);
}

