using UnityEngine;

public class BattelStarter : MonoBehaviour
{
    public GameObject tank1;
    public GameObject tank2;
    public CameraController camController;
    public LayerMask groundLayer;

    void Start()
    {
        // 1. Setup A and B
        float A = 45f; 
        float B = Random.Range(-100f + A + 25f, 200f - A - 25f);

        // 2. Position and Rotate Tank 1
        SetupTank(tank1, B - 30f);

        // 3. Position and Rotate Tank 2
        SetupTank(tank2, B + 30f);
        // Mirror tank2 to face tank1
        tank2.transform.localScale = new Vector3(-Mathf.Abs(tank2.transform.localScale.x), tank2.transform.localScale.y, 1);

        // 4. Initialize Camera
        Camera.main.orthographicSize = 90f / (2f * ((float)Screen.width / Screen.height));
        Camera.main.transform.position = new Vector3(B, tank1.transform.position.y, -10f);

        camController.player1 = tank1.transform;
        camController.player2 = tank2.transform;
    }

    // Helper method to handle both Position and Rotation
    void SetupTank(GameObject tank, float x)
    {
        RaycastHit2D hit = Physics2D.Raycast(new Vector2(x, 85f), Vector2.down, 100f, groundLayer);

        if (hit.collider != null)
        {
            // Position: 1 unit above ground point
            tank.transform.position = new Vector3(x, hit.point.y + 1f, 0);

            // Rotation: Calculate angle based on ground normal
            float angle = Mathf.Atan2(hit.normal.x, hit.normal.y) * Mathf.Rad2Deg;
            tank.transform.rotation = Quaternion.Euler(0, 0, -angle);
        }
        else
        {
            tank.transform.position = new Vector3(x, 15f, 0);
        }
    }
}