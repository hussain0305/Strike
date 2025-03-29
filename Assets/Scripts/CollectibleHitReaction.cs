using System;
using TMPro;
using UnityEngine;

public class CollectibleHitReaction : MonoBehaviour
{
    public Transform edges;
    public TextMeshPro hitPointsDisplay;

    private Collectible collectible;
    private Collectible Collectible => collectible ??= GetComponent<Collectible>();
    
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
            mr.sharedMaterial = GlobalAssets.Instance.collectibleHitMaterial;
        }
        hitPointsDisplay?.gameObject.SetActive(true);
        Collectible.inBodyPointDisplayPositive?.gameObject.SetActive(false);
        Collectible.inBodyPointDisplayNegative?.gameObject.SetActive(false);
    }

    public void SetDefaultVisuals(NextShotCuedEvent e)
    {
        foreach (MeshRenderer mr in edges.GetComponentsInChildren<MeshRenderer>())
        {
            mr.sharedMaterial = Collectible.value > 0
                ? GlobalAssets.Instance.positiveCollectibleMaterial
                : GlobalAssets.Instance.negativeCollectibleMaterial;
        }
        hitPointsDisplay?.gameObject.SetActive(false);
        Collectible.InBodyActivePointDisplay?.gameObject.SetActive(true);
        Collectible.InBodyInactivePointDisplay?.gameObject.SetActive(false);
    }

    public void UpdatePoints(int points)
    {
        hitPointsDisplay.text = points.ToString();
        Collectible.InBodyActivePointDisplay.text = points.ToString();
    }
}
