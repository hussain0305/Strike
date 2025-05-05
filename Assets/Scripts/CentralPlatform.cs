using System;
using UnityEngine;

public class LevelPlatformHasRotation
{
    public float RotationSpeed;
    public LevelPlatformHasRotation(float rotationSpeed)
    {
        RotationSpeed = rotationSpeed;
    }
}

public class CentralPlatform : MonoBehaviour
{
    private Vector3 rotationAxis = Vector3.up;
    private float rotationSpeed = 0f;

    private bool isRotating = false;
    
    private const float ROTATION_START_DELAY = 0.5f;
    private float rotationDelay;
    
    private void OnEnable()
    {
        EventBus.Subscribe<PreNextShotCuedEvent>(PauseRotation);
        EventBus.Subscribe<NextShotCuedEvent>(ResumeRotation);
        EventBus.Subscribe<LevelPlatformHasRotation>(SetRotationSpeed);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<PreNextShotCuedEvent>(PauseRotation);
        EventBus.Unsubscribe<NextShotCuedEvent>(ResumeRotation);
        EventBus.Unsubscribe<LevelPlatformHasRotation>(SetRotationSpeed);
    }

    public void PauseRotation(PreNextShotCuedEvent e)
    {
        isRotating = false;
        transform.rotation = Quaternion.identity;
    }

    public void ResumeRotation<T>(T e)
    {
        rotationDelay = 0;
        isRotating = true;
    }

    public void SetRotationSpeed(LevelPlatformHasRotation e)
    {
        rotationSpeed = e.RotationSpeed;
        rotationDelay = 0;
        isRotating = true;
    }
    
    private void Update()
    {
        if (!isRotating)
            return;

        if (rotationDelay < ROTATION_START_DELAY)
        {
            rotationDelay += Time.deltaTime;
            return;
        }
        
        transform.Rotate(rotationAxis.normalized * (rotationSpeed * Time.deltaTime));
    }
}
