using UnityEngine;
using UnityEngine.Serialization;

public class WallChecker : MonoBehaviour
{
    private Ball ball;
    private Ball Ball => ball ??= GetComponentInParent<Ball>();

    private void OnTriggerEnter(Collider other)
    {
        if (Ball.collidedWithSomething)
        {
            return;
        }
        if (other && other.gameObject.layer == LayerMask.NameToLayer("CollideWithBallUnaffected"))
        {
            return;
        }
        Ball.collidedWithSomething = true;
    }
}
