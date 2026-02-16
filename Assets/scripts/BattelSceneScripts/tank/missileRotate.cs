using UnityEngine;

public class MissileRotate : MonoBehaviour
{
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (rb.linearVelocity.magnitude > 0.1f)
        {
            // Calculate the angle based on the current velocity vector
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            
            // Apply the rotation
            transform.rotation = Quaternion.AngleAxis(angle + 180f, Vector3.forward);
        }
    }
}