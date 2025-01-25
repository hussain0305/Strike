using UnityEngine;

public class WallChecker : MonoBehaviour
{
    public BallControl ball;

    private void OnTriggerEnter(Collider other)
    {
        if (other && other.gameObject.layer == LayerMask.NameToLayer("CollideWithBallUnaffected"))
        {
            return;
        }
        ball.collidedWithSomething = true;
    }
}
