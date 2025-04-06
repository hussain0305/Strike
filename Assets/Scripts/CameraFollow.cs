using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public float followDistance = 10f;
    public float followHeight = 5f;
    public float followSpeed = 5f;
    public float rotationSpeed = 5f;

    [HideInInspector]
    public bool followBall = false;

    private Transform ball;
    public Transform Ball
    {
        get
        {
            if (ball == null)
            {
                ball = GameManager.Instance.ball.transform;
            }

            return ball;
        }
        set => ball = value;
    }
    
    public Rigidbody ballRigidbody;
    public Rigidbody BallRigidbody
    {
        get
        {
            if (ballRigidbody == null)
            {
                ballRigidbody = GameManager.Instance?.ball.rb;
            }

            return ballRigidbody;
        }
    }

    private void LateUpdate()
    {
        if (Ball == null || BallRigidbody == null || !followBall) return;

        Vector3 velocity = BallRigidbody.linearVelocity;

        if (velocity.sqrMagnitude < 0.01f)
        {
            velocity = Ball.forward;
        }
        Vector3 direction = velocity.normalized;
        Vector3 targetPosition = Ball.position - direction * followDistance + Vector3.up * followHeight;
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        Quaternion targetRotation = Quaternion.LookRotation(Ball.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}
