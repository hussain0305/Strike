using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform ball;
    public Rigidbody ballRigidbody;
    public float followDistance = 10f;
    public float followHeight = 5f;
    public float followSpeed = 5f;
    public float rotationSpeed = 5f;

    [HideInInspector]
    public bool followBall = false;

    private void LateUpdate()
    {
        if (ball == null || ballRigidbody == null || !followBall) return;

        Vector3 velocity = ballRigidbody.linearVelocity;

        if (velocity.sqrMagnitude < 0.01f)
        {
            velocity = ball.forward;
        }
        Vector3 direction = velocity.normalized;
        Vector3 targetPosition = ball.position - direction * followDistance + Vector3.up * followHeight;
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        Quaternion targetRotation = Quaternion.LookRotation(ball.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}
