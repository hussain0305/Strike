using System;
using UnityEngine;

public class SniperAbility : BallAbility, IBallAbility
{
    private Transform cylinderPivot;
    private GameObject aimDot;
    private bool isActive = false;

    private Material unlitMaterial;
    
    private void Start()
    {
        cylinderPivot = GameManager.Instance.angleInput.cylinderPivot;

        aimDot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        aimDot.transform.localScale = Vector3.one * 1f;
        unlitMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        unlitMaterial.color = Color.red;
        aimDot.GetComponent<Renderer>().material = unlitMaterial;
        
        Destroy(aimDot.GetComponent<Collider>());
        isActive = true;
    }

    private void OnEnable()
    {
        GameManager.OnBallShot += StopSniper;
        GameManager.OnNextShotCued += ResumeSniper;
    }

    private void OnDisable()
    {
        GameManager.OnBallShot -= StopSniper;
        GameManager.OnNextShotCued -= ResumeSniper;
    }

    private void Update()
    {
        if (!isActive) return;

        if (Physics.Raycast(cylinderPivot.position, cylinderPivot.forward, out RaycastHit hit, Mathf.Infinity))
        {
            float squaredDistance = (cylinderPivot.position - hit.point).sqrMagnitude;
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

        aimDot.transform.localScale = Vector3.one * size;
    }

    private void OnDestroy()
    {
        if (unlitMaterial != null)
        {
            Destroy(unlitMaterial);
        }
    }
}