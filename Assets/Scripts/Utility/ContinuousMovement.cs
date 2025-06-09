using System;
using UnityEngine;

public class ContinuousMovement : MonoBehaviour
{
    public Transform pointATransform;
    public Transform pointBTransform;
    
    public float speed = 1f;

    private float lerpFactor = 0f;
    private bool goingForward = true;

    private void Start()
    {
        if (pointATransform)
            pointATransform.parent = transform.parent.parent;
        if (pointBTransform)
            pointBTransform.parent = transform.parent.parent;
    }

    private void OnEnable()
    {
        lerpFactor = 0f;
    }

    void Update()
    {
        transform.position = Vector3.Lerp(pointATransform.position, pointBTransform.position, lerpFactor);

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

    public void CreateMarkers(Vector3 pointA, Vector3 pointB)
    {
        pointATransform = CreateMarker("PointA", pointA);
        pointBTransform = CreateMarker("PointB", pointB);
    }
    
    Transform CreateMarker(string _name, Vector3 position)
    {
        var markerTransform = new GameObject(_name).transform;
        markerTransform.position = position;
        markerTransform.SetParent(transform, true);
        return markerTransform;
    }
}
