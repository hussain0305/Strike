using System;
using UnityEngine;

using UnityEngine;
using System.Collections.Generic;

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
            { "Soccer Ball", new PreviewSoccerBall() },
            // { BallType.Sniper, new SniperBallPreview() },
            // { BallType.Shotgun, new ShotgunBallPreview() }
            // Add more types as needed
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
