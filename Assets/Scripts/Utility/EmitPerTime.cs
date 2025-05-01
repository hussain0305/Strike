using System;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class EmitPerTime : MonoBehaviour
{
    public float emitFrequency = 0.05f;
    
    private float timeSinceLastEmission = 0;
    private bool shouldEmit = false;
    private ParticleSystem ps;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        var em = ps.emission;
        em.rateOverTime = 0;
    }

    private void OnEnable()
    {
        EventBus.Subscribe<BallShotEvent>(BallShot);
        EventBus.Subscribe<NextShotCuedEvent>(NextShotCued);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<BallShotEvent>(BallShot);
        EventBus.Unsubscribe<NextShotCuedEvent>(NextShotCued);
    }

    void Update()
    {
        if (!shouldEmit)
            return;

        timeSinceLastEmission += Time.deltaTime;
        if (timeSinceLastEmission >= emitFrequency)
        {
            timeSinceLastEmission = 0;
            ps.Emit(1);
        }
    }

    public void BallShot(BallShotEvent e)
    {
        shouldEmit = true;
    }
    
    public void NextShotCued(NextShotCuedEvent e)
    {
        shouldEmit = false;
    }
}