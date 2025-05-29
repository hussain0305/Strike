using System;
using UnityEngine;

public class ExhibitionPin : MonoBehaviour, ICollectible
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
        Hit(other.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        Hit(other.gameObject);
    }

    public void Hit(GameObject collidingObject)
    {
        Ball ball = collidingObject.GetComponent<Ball>();
        if (ball)
        {
            SetHitVisuals();
            ball.collidedWithSomething = true;
        }
    }

    public bool CanBeCollected()
    {
        return true;
    }
}
