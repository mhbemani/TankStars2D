using UnityEngine;
using UnityEngine.InputSystem;

public class TankMovement : MonoBehaviour
{
    [Header("Control Mode")]
    public bool isAI = false;          // <-- NEW: Control Switch
    public float aiMoveInput = 0f;     // <-- NEW: Virtual "Key" for AI

    [Header("Movement Settings")]
    public float speed = 5f;
    public LayerMask groundLayer;
    public float baseGravity = 3f; 

    public int playerID; // 1 for Player 1, 2 for Player 2
    
    [Header("Sensor States")]
    private bool centerGrounded, frontGrounded, backGrounded;
    private Vector2 centerNormal, frontNormal, backNormal;

    private Rigidbody2D rb;
    private float moveInput; // Now filled by either Keyboard or AI
    
    private bool facingLeft = false; 
    private TankFuel fuelScript;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = baseGravity; 
        fuelScript = GetComponent<TankFuel>();
    }

    void Update()
    {
        // GATEKEEPER: Check if it's this player's turn
        if (BattleManager.Instance != null)
        {
            bool isMyTurn = (playerID == 1 && BattleManager.Instance.currentState == BattleManager.BattleState.Player1Turn) ||
                            (playerID == 2 && BattleManager.Instance.currentState == BattleManager.BattleState.Player2Turn);

            if (!isMyTurn) 
            {
                rb.linearVelocity = Vector2.zero;
                moveInput = 0f;
                aiMoveInput = 0f; // Reset AI input too
                return; 
            }
        }
        
        // --- MODIFIED SECTION START ---
        // 1. GET INPUT (Multiplexer)
        if (!isAI)
        {
            // Human Keyboard Control
            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveInput = -1f;
                else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveInput = 1f;
                else moveInput = 0f;
            }
        }
        else
        {
            // AI Virtual Control
            moveInput = aiMoveInput;
        }
        // --- MODIFIED SECTION END ---

        // 2. FLIP LOGIC
        if (moveInput < 0 && !facingLeft) Flip();
        else if (moveInput > 0 && facingLeft) Flip();

        // 3. FUEL CHECK
        if (fuelScript != null && fuelScript.currentFuel <= 0)
        {
            moveInput = 0;
        }

        // 4. THE "ONE RULE" PHYSICS (Unchanged)
        bool isTouchingGround = centerGrounded || (frontGrounded && backGrounded);

        if (isTouchingGround)
        {
            rb.gravityScale = 0f; 

            Vector2 targetNormal;
            if (centerGrounded) targetNormal = centerNormal;
            else targetNormal = (frontNormal + backNormal).normalized;

            float angle = Mathf.Atan2(targetNormal.x, targetNormal.y) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, -angle), Time.deltaTime * 10f);

            Vector2 slopeTangent = new Vector2(targetNormal.y, -targetNormal.x);
            rb.linearVelocity = slopeTangent * moveInput * speed;
        }
        else
        {
            rb.gravityScale = baseGravity;
        }

        // 5. FUEL CONSUMPTION (Unchanged)
        if (fuelScript != null && isTouchingGround && moveInput != 0)
        {
            fuelScript.ConsumeFuel();
        }
    }

    void Flip()
    {
        facingLeft = !facingLeft;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    public void UpdateSensorStatus(string sensorName, bool grounded, Vector2 normal)
    {
        if (sensorName == "Center") { centerGrounded = grounded; centerNormal = normal; }
        else if (sensorName == "Front") { frontGrounded = grounded; frontNormal = normal; }
        else if (sensorName == "Back") { backGrounded = grounded; backNormal = normal; }
    }
    
    public void SetInitialFacing(bool isFacingLeft)
    {
        facingLeft = isFacingLeft;
    }
}