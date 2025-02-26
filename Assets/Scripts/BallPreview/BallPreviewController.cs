using System;
using UnityEngine;

using UnityEngine;
using System.Collections.Generic;

public class BallPreviewController : MonoBehaviour
{
    public Transform ballLocation;
    public Transform aimTransform;
    public LineRenderer trajectory;
    public ExhibitionPin[] previewPins;
    
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
    }

    private void OnEnable()
    {
        EventBus.Subscribe<ResetPreviewEvent>(OnResetPreview);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<ResetPreviewEvent>(OnResetPreview);
    }

    public void PlayPreview(string ballName, GameObject previewBall)
    {
        if (previewStrategies.TryGetValue(ballName, out IBallPreview preview))
        {
            preview.PlayPreview(previewBall);
        }
    }

    public void OnResetPreview(ResetPreviewEvent e)
    {
        foreach (ExhibitionPin pin in previewPins)
        {
            pin.Reset();
        }
    }
}
