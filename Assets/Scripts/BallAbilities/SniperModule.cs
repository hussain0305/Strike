using UnityEngine;

public class SniperModule : IBallAbilityModule, IBallAbilityUpdateableModule
{
    public AbilityAxis Axis => AbilityAxis.AimModifier;

    private Ball ball;
    private IContextProvider context;
    private Transform aimTransform;
    private GameObject aimDot;
    private Material unlitMaterial;
    private int aimDotLayerMask;
    private bool isActive;

    public void Initialize(Ball _ownerBall, IContextProvider _context)
    {
        ball = _ownerBall;
        context = _context;
        aimTransform = context.GetAimTransform();
        aimDotLayerMask = ~(1 << LayerMask.NameToLayer("Ball"));

        aimDot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        aimDot.transform.parent     = ball.transform;
        aimDot.transform.localScale = Vector3.one;
        Object.Destroy(aimDot.GetComponent<Collider>());

        unlitMaterial = new Material(
            Shader.Find("Universal Render Pipeline/Unlit"));
        unlitMaterial.color =
            Color.red;
        aimDot.GetComponent<Renderer>().material = unlitMaterial;

        isActive = true;
    }

    public void OnBallShot(BallShotEvent e)
    {
        isActive = false;
        if (aimDot != null)
            aimDot.SetActive(false);
    }
    
    public void OnNextShotCued(NextShotCuedEvent e)
    {
        isActive = true;
    }
    
    public void OnUpdate()
    {
        if (!isActive) return;

        if (Physics.Raycast(
                aimTransform.position,
                aimTransform.forward,
                out RaycastHit hit,
                Mathf.Infinity,
                aimDotLayerMask))
        {
            float distSqr = (aimTransform.position - hit.point)
                               .sqrMagnitude;
            UpdateDotSize(distSqr);

            aimDot.transform.position = hit.point;
            aimDot.SetActive(true);
        }
        else if (aimDot != null)
        {
            aimDot.SetActive(false);
        }
    }

    public void Cleanup()
    {
        if (unlitMaterial != null)
            Object.Destroy(unlitMaterial);
        if (aimDot != null)
            Object.Destroy(aimDot);
    }

    private void UpdateDotSize(float squaredDistance)
    {
        float size;
        float relativeScale = 1f / ball.transform.localScale.x;

        if (squaredDistance < 500f)
        {
            size = Mathf.Lerp(0.1f, 0.2f,
                              squaredDistance / 1000f);
        }
        else if (squaredDistance < 10000f)
        {
            size = Mathf.Clamp(
                squaredDistance / 3500f,
                0.2f, 0.45f);
        }
        else
        {
            size = Mathf.Clamp(
                squaredDistance / 18000f,
                1f, 1.5f);
        }

        aimDot.transform.localScale =
            size * relativeScale * Vector3.one;
    }

    public void OnProjectilesSpawned(ProjectilesSpawnedEvent e)
    {
        foreach (GameObject proj in e.projectiles)
        {
            Rigidbody rBody = proj.GetComponent<Rigidbody>();
            rBody.useGravity = false;
        }
    }
    public void OnHitSomething(BallHitSomethingEvent e) { }
}
