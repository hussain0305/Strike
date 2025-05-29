using System;
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
            { "soccBall", gameObject.AddComponent<PreviewSoccerBall>() },
            { "shotgunBall", gameObject.AddComponent<PreviewShotgunBall>() },
            { "sniperBall", gameObject.AddComponent<PreviewSniperBall>() },
            { "stickyBall", gameObject.AddComponent<PreviewStickyBall>() },
            { "mirrorBall", gameObject.AddComponent<PreviewMirrorBall>() },
            { "ricoBall", gameObject.AddComponent<PreviewRicochet>() },
        };
    }
    
    public void PlayPreview(string ballID, GameObject previewBall)
    {
        if (previewStrategies.TryGetValue(ballID, out IBallPreview preview))
        {
            preview.PlayPreview(previewBall);
        }
    }
}
