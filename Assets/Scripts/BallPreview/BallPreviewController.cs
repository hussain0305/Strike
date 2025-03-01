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
            { "Soccer Ball", transform.AddComponent<PreviewSoccerBall>() },
            { "Shotgun Ball", transform.AddComponent<PreviewShotgunBall>() },
            { "Sniper Ball", transform.AddComponent<PreviewSniperBall>() }
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

    public void PlayPreview(string ballName, GameObject previewBall)
    {
        if (previewStrategies.TryGetValue(ballName, out IBallPreview preview))
        {
            preview.PlayPreview(previewBall);
        }
    }
}
