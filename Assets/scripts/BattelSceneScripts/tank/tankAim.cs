using UnityEngine;
using UnityEngine.InputSystem;

public class TankAim : MonoBehaviour
{
    [Header("Control Mode")]
    public bool isAI = false;          // <-- NEW: Switcher
    public float aiAimInput = 0f;      // <-- NEW: Virtual "Key" for AI

    public float aimSpeed = 50f;
    public float minAngle = -20f;      // Kept your updated clamp values
    public float maxAngle = 60f;
    public float currentAngle = 20f;   // <-- CHANGED: Made public for AI visibility

    void Update()
    {
        // Turn checking logic remains exactly the same
        TankMovement parentTank = GetComponentInParent<TankMovement>();
        
        if (parentTank != null && BattleManager.Instance != null)
        {
            bool isMyTurn = (parentTank.playerID == 1 && BattleManager.Instance.currentState == BattleManager.BattleState.Player1Turn) ||
                            (parentTank.playerID == 2 && BattleManager.Instance.currentState == BattleManager.BattleState.Player2Turn);
    
            if (!isMyTurn) return; 
        }
        
        float aimInput = 0f;

        // --- MODIFIED SECTION START ---
        if (!isAI)
        {
            // Human Keyboard Control
            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                    aimInput = 1f; 
                else if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                    aimInput = -1f;
            }
        }
        else
        {
            // AI "Virtual" Control
            aimInput = aiAimInput;
        }
        // --- MODIFIED SECTION END ---

        currentAngle += aimInput * aimSpeed * Time.deltaTime;
        currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);
        
        transform.localRotation = Quaternion.Euler(0, 0, currentAngle);
    }
}