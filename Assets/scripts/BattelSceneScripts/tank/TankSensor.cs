using UnityEngine;

public class TankSensor : MonoBehaviour
{
    public string sensorName; 
    public LayerMask groundLayer;
    public Vector2 boxSize = new Vector2(0.6f, 0.3f); 
    private TankMovement mainTank;

    void Start()
    {
        mainTank = GetComponentInParent<TankMovement>();
    }

    void Update()
    {
        // 1. DETECTION (The "InsideTouchingYes" part)
        // OverlapBox detects the ground even if the sensor is partially or fully inside it.
        Collider2D hit = Physics2D.OverlapBox(transform.position, boxSize, 0f, groundLayer);
        
        if (hit != null)
        {
            // 2. THE "NO HOVER" FIX
            // We start a ray slightly ABOVE the sensor and shoot down.
            // This finds the actual surface of the ground so the tank settles correctly.
            RaycastHit2D slopeHit = Physics2D.Raycast(transform.position + Vector3.up * 0.5f, Vector2.down, 1.5f, groundLayer);
            
            Vector2 normal = slopeHit.collider != null ? slopeHit.normal : Vector2.up;

            mainTank.UpdateSensorStatus(sensorName, true, normal);
        }
        else
        {
            mainTank.UpdateSensorStatus(sensorName, false, Vector2.up);
        }
    }

    // This helps you see the sensor area in the Scene View
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, boxSize);
    }
}