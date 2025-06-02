using System;
using UnityEngine;

public class ContinuousMovement : MonoBehaviour
{
    public Transform pointATransform;
    public Transform pointBTransform;

    [HideInInspector]
    public Vector3 pointA;
    [HideInInspector]
    public Vector3 pointB;
    
    public float speed = 1f;

    private float lerpFactor = 0f;
    private bool goingForward = true;

    private void Awake()
    {
        if (pointATransform)
            pointA = pointATransform.position;
        if (pointBTransform)
            pointB = pointBTransform.position;
    }

    private void OnEnable()
    {
        lerpFactor = 0f;
    }

    void Update()
    {
        transform.position = Vector3.Lerp(pointA, pointB, lerpFactor);

        if (goingForward)
        {
            lerpFactor += Time.deltaTime * speed;
            if (lerpFactor >= 1f)
            {
                lerpFactor = 1f;
                goingForward = false;
            }
        }
        else
        {
            lerpFactor -= Time.deltaTime * speed;
            if (lerpFactor <= 0f)
            {
                lerpFactor = 0f;
                goingForward = true;
            }
        }
    }
}
