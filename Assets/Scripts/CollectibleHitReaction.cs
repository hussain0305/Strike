using System;
using TMPro;
using UnityEngine;

public class CollectibleHitReaction : MonoBehaviour
{
    public Material defaultMaterial;
    public Material hitMaterial;
    public Transform edges;
    public TextMeshPro defaultPointsDisplay;
    public TextMeshPro hitPointsDisplay;

    private void OnEnable()
    {
        SetDefaultVisuals(null);
        EventBus.Subscribe<NextShotCuedEvent>(SetDefaultVisuals);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<NextShotCuedEvent>(SetDefaultVisuals);
    }

    public void CheckIfHitsExhasuted(int numTimesCollected, int numTimesCanBeCollected)
    {
        if (numTimesCollected >= numTimesCanBeCollected)
        {
            SetHitVisuals();
        }
    }

    public void SetHitVisuals()
    {
        foreach (MeshRenderer mr in edges.GetComponentsInChildren<MeshRenderer>())
        {
            mr.sharedMaterial = hitMaterial;
        }
        hitPointsDisplay?.gameObject.SetActive(true);
        defaultPointsDisplay?.gameObject.SetActive(false);
    }

    public void SetDefaultVisuals(NextShotCuedEvent e)
    {
        foreach (MeshRenderer mr in edges.GetComponentsInChildren<MeshRenderer>())
        {
            mr.sharedMaterial = defaultMaterial;
        }
        hitPointsDisplay?.gameObject.SetActive(false);
        defaultPointsDisplay?.gameObject.SetActive(true);
    }

    public void UpdatePoints(int points)
    {
        hitPointsDisplay.text = points.ToString();
        defaultPointsDisplay.text = points.ToString();
    }
}
