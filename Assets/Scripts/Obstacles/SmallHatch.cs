using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class SmallHatch : Obstacle, ISwitchable
{
    public Transform hatch;
    public Collider hatchCollider;

    private bool hatchOpen = false;
    
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
        WakeUpTouchingRigidbodies();
        hatch.gameObject.SetActive(false);
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
        BoxCollider box = (BoxCollider)hatchCollider;
        Vector3 worldCenter = box.transform.TransformPoint(box.center);
        Quaternion worldRotation = box.transform.rotation;
        Vector3 scaledBoxSize = Vector3.Scale(box.size, box.transform.localScale);
        Vector3 halfExtents = scaledBoxSize * 0.5f * 1.25f;
        Collider[] touchingColliders = Physics.OverlapBox(worldCenter, halfExtents, worldRotation, restingObjectsLayerMask);

        // DebugHelper.DrawOverlapBox(worldCenter, halfExtents, worldRotation);
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
