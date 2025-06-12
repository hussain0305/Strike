using System;
using UnityEngine;

public class ContinuousMovement : MonoBehaviour
{
    public Transform[] pointTransforms;
    [HideInInspector]
    public Vector3[] points;
    
    public float speed = 1f;

    private float lerpFactor = 0f;
    private int currentSegment = 0;
    private bool canMove = false;
    
    private void OnEnable()
    {
        lerpFactor = 0f;
        EventBus.Subscribe<NewGameStartedEvent>(NewGameStarted);
    }

    private void OnDisable()
    {
        canMove = false;
        EventBus.Unsubscribe<NewGameStartedEvent>(NewGameStarted);
    }

    private void NewGameStarted(NewGameStartedEvent e)
    {
        Transform platform = transform.parent.parent;
        for (int i = 0; i < pointTransforms.Length; i++)
        {
            if (pointTransforms[i])
                pointTransforms[i].parent = platform;
        }

        canMove = true;
        currentSegment = 0;
    }

    void Update()
    {
        if (!canMove)
            return;
        
        Transform a = pointTransforms[currentSegment];
        Transform b = pointTransforms[(currentSegment + 1) % pointTransforms.Length];
        transform.position = Vector3.Lerp(a.position, b.position, lerpFactor);

        lerpFactor += Time.deltaTime * speed;
        if (lerpFactor >= 1f)
        {
            lerpFactor = 0f;
            currentSegment = (currentSegment + 1) % pointTransforms.Length;
        }
    }

    public void CreateMarkers(Vector3[] positions)
    {
        currentSegment = 0;
        pointTransforms = new Transform[positions.Length];
        points = positions;
        for (int i = 0; i < positions.Length; i++)
        {
            pointTransforms[i] = CreateMarker("Point" + i, positions[i]);
        }
    }
    
    Transform CreateMarker(string _name, Vector3 position)
    {
        var markerTransform = new GameObject(_name).transform;
        markerTransform.position = position;
        markerTransform.SetParent(transform, true);
        return markerTransform;
    }
}