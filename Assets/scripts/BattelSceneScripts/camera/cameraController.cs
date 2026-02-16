using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player1;
    public Transform player2;

    [Header("Constraints")]
    public float minWidth = 90f;
    public float maxWidth = 110f;
    public float worldMinX = -95f;
    public float worldMaxX = 195f;
    public float edgePadding = 10f; 

    private Camera cam;
    private float aspectRatio;
    private Transform focusPlayer;

    void Awake()
    {
        cam = GetComponent<Camera>();
        UpdateAspectRatio();
    }

    void UpdateAspectRatio()
    {
        // Prevent division by zero if screen isn't ready
        float width = Screen.width > 0 ? Screen.width : 1920;
        float height = Screen.height > 0 ? Screen.height : 1080;
        aspectRatio = width / height;
    }

    void LateUpdate()
    {
        if (player1 == null || player2 == null) return;

        // 1. Refresh aspect ratio in case window resized
        UpdateAspectRatio();

        // 2. Identify Focus
        if (BattleManager.Instance != null)
        {
            if (BattleManager.Instance.currentState == BattleManager.BattleState.Player1Turn) 
                focusPlayer = player1;
            else if (BattleManager.Instance.currentState == BattleManager.BattleState.Player2Turn) 
                focusPlayer = player2;
        }
        if (focusPlayer == null) focusPlayer = player1;

        // 3. Calculate Midpoints
        float midX = (player1.position.x + player2.position.x) / 2f;
        float midY = (player1.position.y + player2.position.y) / 2f;

        // 4. Calculate Required Width
        float distBetweenTanks = Mathf.Abs(player1.position.x - player2.position.x);
        
        // ADDED: Explicitly use Mathf.Max to ensure we NEVER go below minWidth
        float requiredWidth = distBetweenTanks + (edgePadding * 2f);
        float targetWidth = Mathf.Clamp(requiredWidth, minWidth, maxWidth);
        
        // 5. Calculate Size
        // Size = Width / (2 * Aspect)
        float targetSize = targetWidth / (2f * aspectRatio);
        
        // Prevent accidental "Micro-zoom"
        if (targetSize < 5f) targetSize = minWidth / (2f * aspectRatio); 
        
        cam.orthographicSize = targetSize;

        // 6. Horizontal Pushing Logic
        float currentHalfWidth = cam.orthographicSize * aspectRatio;
        float camX = transform.position.x;
        
        int side = (focusPlayer.position.x < camX) ? -1 : 1;
        float distToEdge = (side == -1) ? (focusPlayer.position.x - (camX - currentHalfWidth)) : ((camX + currentHalfWidth) - focusPlayer.position.x);

        if (distToEdge < edgePadding)
        {
            float pushAmount = edgePadding - distToEdge;
            camX += pushAmount * side;
        }
        else
        {
            camX = Mathf.MoveTowards(camX, midX, Time.deltaTime * 10f);
        }

        // 7. Clamp and Apply
        float finalHalfWidth = cam.orthographicSize * aspectRatio;
        camX = Mathf.Clamp(camX, worldMinX + finalHalfWidth, worldMaxX - finalHalfWidth);

        transform.position = new Vector3(camX, midY, transform.position.z);
    }

    public float GetRandomSpawnCenter()
{
    // We need 25 units for the tank offset + some padding (e.g., 10) 
    // so they don't spawn right against the world edge.
    float margin = 35f; 
    return Random.Range(worldMinX + margin, worldMaxX - margin);
}
}