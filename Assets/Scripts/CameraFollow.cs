using UnityEngine;
using Zenject;

public class CameraFollow : MonoBehaviour
{
    public float followDistance = 10f;
    public float followHeight = 5f;
    public float followSpeed = 5f;
    public float rotationSpeed = 5f;

    [HideInInspector]
    public bool followBall = false;

    private Transform ballTransform;
    public Transform BallTransform
    {
        get
        {
            if (ballTransform == null)
            {
                ballTransform = gameManager.ball.transform;
            }

            return ballTransform;
        }
        set => ballTransform = value;
    }
    
    private Ball ball;
    public Ball Ball
    {
        get
        {
            if (ball == null)
            {
                ball = gameManager.ball;
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
                ballRigidbody = gameManager?.ball.rb;
            }

            return ballRigidbody;
        }
    }

    private GameManager gameManager;

    [Inject]
    public void Construct(GameManager _gameManager)
    {
        gameManager = _gameManager;
    }

    private void LateUpdate()
    {
        if (BallTransform == null || BallRigidbody == null || !followBall)
            return;

        Vector3 velocity = BallRigidbody.linearVelocity;

        if (velocity.sqrMagnitude < 0.01f)
        {
            velocity = Ball.LastKnownVelocity;
        }
        Vector3 direction = velocity.normalized;
        Vector3 targetPosition = BallTransform.position - direction * followDistance + Vector3.up * followHeight;
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        Quaternion targetRotation = Quaternion.LookRotation(BallTransform.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}
