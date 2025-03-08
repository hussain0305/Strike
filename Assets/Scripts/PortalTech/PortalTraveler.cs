using UnityEngine;

public class PortalTraveler : MonoBehaviour
{
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Teleport(Vector3 newPosition, Quaternion newRotation)
    {
        transform.position = newPosition;
        transform.rotation = newRotation;

        if (rb)
        {
            Vector3 localVelocity = rb.linearVelocity;
            rb.linearVelocity = newRotation * localVelocity;
        }
    }
}