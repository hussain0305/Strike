using System;
using UnityEngine;

public class ContinuousMovement : MonoBehaviour
{
    public Transform pointATransform;
    public Transform pointBTransform;

    [HideInInspector] public Vector3 pointA;
    [HideInInspector] public Vector3 pointB;
    public float speed = 1f;

    private float t = 0f;
    private bool goingForward = true;

    private void Awake()
    {
        if (pointATransform) pointA = pointATransform.position;
        if (pointBTransform) pointB = pointBTransform.position;
    }

    void Update()
    {
        transform.position = Vector3.Lerp(pointA, pointB, t);

        if (goingForward)
        {
            t += Time.deltaTime * speed;
            if (t >= 1f)
            {
                t = 1f;
                goingForward = false;
            }
        }
        else
        {
            t -= Time.deltaTime * speed;
            if (t <= 0f)
            {
                t = 0f;
                goingForward = true;
            }
        }
    }
}
