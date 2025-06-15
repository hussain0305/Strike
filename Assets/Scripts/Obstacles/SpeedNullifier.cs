using System;
using UnityEngine;

public class SpeedNullifier : Obstacle
{
    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rBody = other.GetComponent<Rigidbody>();
        if (rBody)
        {
            rBody.linearVelocity = Vector3.zero;
            rBody.angularVelocity = Vector3.zero;
        }
    }
}
