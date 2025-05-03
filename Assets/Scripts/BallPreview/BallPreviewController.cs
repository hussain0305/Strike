using System;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class BallPreviewController : MonoBehaviour
{
    public Transform aimTransform;
    public LineRenderer trajectory;
    public Transform previewSceneObjects;
    public Tee tee;
    
    private Dictionary<string, IBallPreview> previewStrategies;
    
    private void Awake()
    {
        previewStrategies = new Dictionary<string, IBallPreview>
        {
            { "soccBall", transform.AddComponent<PreviewSoccerBall>() },
            { "shotgunBall", transform.AddComponent<PreviewShotgunBall>() },
            { "sniperBall", transform.AddComponent<PreviewSniperBall>() },
            { "stickyBall", transform.AddComponent<PreviewStickyBall>() }
        };

        InitializeElements();
    }

    private void InitializeElements()
    {
        foreach (Collectible collectible in previewSceneObjects.GetComponentsInChildren<Collectible>())
        {
            collectible.Initialize(MainMenu.Context);
        }
    }

    public void PlayPreview(string ballID, GameObject previewBall)
    {
        if (previewStrategies.TryGetValue(ballID, out IBallPreview preview))
        {
            preview.PlayPreview(previewBall);
        }
    }
}
