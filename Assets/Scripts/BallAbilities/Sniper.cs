using System;
using UnityEngine;

public class SniperAbility : BallAbility
{
    private Transform aimTransform;
    private GameObject aimDot;
    private bool isActive = false;

    private Material unlitMaterial;

    private int aimDotLayer;
    
    public override void Initialize(Ball ownerBall, IContextProvider _context)
    {
        base.Initialize(ownerBall, _context);
        
        aimTransform = context.GetAimTransform();

        if (!aimDot)
        {
            aimDot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            aimDot.transform.localScale = Vector3.one * 1f;
            unlitMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            unlitMaterial.color = Color.red;
            aimDot.GetComponent<Renderer>().material = unlitMaterial;
            aimDotLayer = ~(1 << LayerMask.NameToLayer("Ball"));
            Destroy(aimDot.GetComponent<Collider>());
            aimDot.transform.parent = ownerBall.transform;
        }
        isActive = true;
    }
    
    public override void BallShot(BallShotEvent e)
    {
        StopSniper();
    }

    public override void NextShotCued(NextShotCuedEvent e)
    {
        ResumeSniper();
    }

    private void Update()
    {
        if (!isActive) return;

        if (Physics.Raycast(aimTransform.position, aimTransform.forward, out RaycastHit hit, Mathf.Infinity, aimDotLayer))
        {
            float squaredDistance = (aimTransform.position - hit.point).sqrMagnitude;
            SetDotSize(squaredDistance);
            
            aimDot.transform.position = hit.point;
            aimDot.SetActive(true);
        }
        else
        {
            aimDot.SetActive(false);
        }
    }

    private void StopSniper()
    {
        isActive = false;
        aimDot.SetActive(false);
    }

    private void ResumeSniper()
    {
        isActive = true;
    }

    private void SetDotSize(float squaredDistance)
    {
        float size;
        float relativeScaling = 1 / ball.transform.localScale.x;
        if (squaredDistance < 500f)
        {
            size = Mathf.Lerp(0.1f, 0.2f, squaredDistance / 1000);
        }
        else if(squaredDistance < 10000f)
        {
            size = Mathf.Clamp(squaredDistance / 3500, 0.2f, 0.45f);
        }
        else
        {
            size = Mathf.Clamp(squaredDistance / 18000, 1f, 1.5f);
        }

        aimDot.transform.localScale = size * relativeScaling * Vector3.one;
    }

    private void OnDestroy()
    {
        if (unlitMaterial != null)
        {
            Destroy(unlitMaterial);
        }
        UnregisterFromEvents();
    }
}