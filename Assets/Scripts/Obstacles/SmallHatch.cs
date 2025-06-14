using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class SmallHatch : Obstacle, ISwitchable
{
    public Transform hatch;

    private bool hatchOpen = false;

    private Collider hatchCollider;
    private Collider HatchCollider => hatchCollider ??= GetComponentInChildren<Collider>();
    
    private LayerMask restingObjectsLayerMask;
    
    public void Switched(bool switchedOn)
    {
        if (switchedOn)
            SetDoorOpen();
        else
            SetDoorClosed();
    }
    
    public void SetDoorOpen()
    {
        hatchOpen = true;
        hatch.gameObject.SetActive(false);
        WakeUpTouchingRigidbodies();
    }

    public void SetDoorClosed()
    {
        hatchOpen = false;
        hatch.gameObject.SetActive(true);
    }

    public void Reset()
    {
        restingObjectsLayerMask = LayerMask.GetMask("CollideWithBall");
        SetDoorClosed();
    }
    
    public void SyncForPlayer(bool open)
    {
        if (open)
            SetDoorOpen();
        else
            SetDoorClosed();
    }
    
    public void WakeUpTouchingRigidbodies()
    {
        float margin = 4f;
        Bounds bounds = HatchCollider.bounds;
        bounds.Expand(margin);

        Vector3 center = bounds.center;
        Vector3 halfExtents = bounds.extents;

        Collider[] touchingColliders = Physics.OverlapBox(center, halfExtents, HatchCollider.transform.rotation, 
            restingObjectsLayerMask);

        HashSet<Rigidbody> awakened = new HashSet<Rigidbody>();

        foreach (Collider col in touchingColliders)
        {
            Rigidbody rb = col.attachedRigidbody;
            if (rb != null && !awakened.Contains(rb))
            {
                rb.WakeUp();
                awakened.Add(rb);
            }
        }
    }
}
