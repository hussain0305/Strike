using UnityEngine;

public class WreckingBall : Obstacle
{
    public float forceStrength = 10f;
    public ForceMode forceMode = ForceMode.Impulse;

    private Rigidbody rb;
    private Rigidbody RBody => rb ??= GetComponent<Rigidbody>();
    
    private readonly string wreckingBallTag = "WreckingBall";
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(wreckingBallTag))
            return;

        float appliedForceStrength = forceStrength;
        Collectible collectible = collision.gameObject.GetComponent<Collectible>();
        if (collectible && collectible.type == CollectibleType.Points && collectible.value > 0)
        {
            appliedForceStrength /= 5;
        }
        
        Rigidbody collidedRigidbody = collision.rigidbody;

        if (collidedRigidbody != null)
        {
            Vector3 direction = (collision.transform.position - transform.position).normalized;
            collidedRigidbody.AddForce(direction * appliedForceStrength * RBody.linearVelocity.sqrMagnitude, forceMode);
        }
    }
}