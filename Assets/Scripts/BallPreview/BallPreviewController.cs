using System;
using UnityEngine;
using System.Collections.Generic;

public class BallPreviewController : MonoBehaviour
{
    public Transform aimTransform;
    public LineRenderer trajectory;
    public Transform previewSceneObjects;
    public Tee tee;
    
    private IBallPreview defaultPreview;
    private Dictionary<string, IBallPreview> previewOverrides;
    
    private void Awake()
    {
        defaultPreview = gameObject.AddComponent<PreviewBallGenericRules>();
        previewOverrides = new Dictionary<string, IBallPreview>
        {
            { "sniperBall", gameObject.AddComponent<PreviewSniperBall>() },
            { "mirrorBall", gameObject.AddComponent<PreviewMirrorBall>() },
            { "shotgunBall", gameObject.AddComponent<PreviewShotgunBall>() },
            { "spinnyBall", gameObject.AddComponent<PreviewSpinny>() }
            // { "stickyBall", gameObject.AddComponent<PreviewStickyBall>() },
        };
    }
    
    public void PlayPreview(string ballID, GameObject previewBall)
    {
        if (previewOverrides.TryGetValue(ballID, out IBallPreview preview))
        {
            preview.PlayPreview(ballID, previewBall);
        }
        else
        {
            defaultPreview.PlayPreview(ballID, previewBall);
        }
    }
}
