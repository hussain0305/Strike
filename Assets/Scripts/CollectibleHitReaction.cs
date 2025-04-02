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
            mr.sharedMaterial = Collectible.value > 0
                ? GlobalAssets.Instance.positiveCollectibleHitMaterial
                : GlobalAssets.Instance.negativeCollectibleHitMaterial;
        }

        Collectible.inBodyPointDisplay.fontMaterial = Collectible.HitFontColor;
    }

    public void SetDefaultVisuals(NextShotCuedEvent e)
    {
        foreach (MeshRenderer mr in edges.GetComponentsInChildren<MeshRenderer>())
        {
            mr.sharedMaterial = Collectible.value > 0
                ? GlobalAssets.Instance.positiveCollectibleMaterial
                : GlobalAssets.Instance.negativeCollectibleMaterial;
        }
        Collectible.inBodyPointDisplay.fontMaterial = Collectible.RegularFontColor;
    }

    public void UpdatePoints(int points)
    {
        hitPointsDisplay.text = points.ToString();
        Collectible.inBodyPointDisplay.text = points.ToString();
        Collectible.inBodyPointDisplay.fontMaterial = Collectible.RegularFontColor;
    }
}
