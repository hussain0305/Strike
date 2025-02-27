using System;
using UnityEngine;

public class ExhibitionPin : MonoBehaviour
{
    public Material defaultMaterial;
    public Material hitMaterial;
    public Transform edges;

    private Vector3 defaultPosition;
    private Rigidbody rBody;
    
    private void Awake()
    {
        defaultPosition = transform.position;
        rBody = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<NextShotCuedEvent>(Reset);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<NextShotCuedEvent>(Reset);
    }

    public void Reset(NextShotCuedEvent e)
    {
        transform.position = defaultPosition;
        transform.rotation = Quaternion.identity;
        rBody.angularVelocity = Vector3.zero;
        rBody.linearVelocity = Vector3.zero;
        SetDefaultVisuals();
    }
    
    public void SetHitVisuals()
    {
        foreach (MeshRenderer mr in edges.GetComponentsInChildren<MeshRenderer>())
        {
            mr.sharedMaterial = hitMaterial;
        }
    }

    public void SetDefaultVisuals()
    {
        foreach (MeshRenderer mr in edges.GetComponentsInChildren<MeshRenderer>())
        {
            mr.sharedMaterial = defaultMaterial;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        Ball ball = other.gameObject.GetComponent<Ball>();
        if (ball)
        {
            SetHitVisuals();
            ball.collidedWithSomething = true;
        }
    }
}
