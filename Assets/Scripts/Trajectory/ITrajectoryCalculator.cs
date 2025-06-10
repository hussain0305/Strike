using System.Collections.Generic;
using UnityEngine;

public interface ITrajectoryCalculator
{
    Ball ball { get; set; }
    void Initialize(IContextProvider _context, Ball ball, float _gravity, int trajectoryPointCount);
    List<Vector3> CalculateTrajectory(Vector3 startPosition);
}
