using UnityEngine;
using System.Collections;

public class SoloBattleManager : BattleManager
{
    [Header("AI Settings")]
    public float aiThinkTime = 2f;

    protected override void StartTurn(BattleState nextPlayer)
{
    base.StartTurn(nextPlayer);

    // --- UI HIDING LOGIC ---
    if (weaponDrawer != null)
    {
        // Show panel if it's Player 1's turn, hide it if it's the AI (Player 2)
        weaponDrawer.SetVisible(nextPlayer == BattleState.Player1Turn);
    }

    if (nextPlayer == BattleState.Player2Turn && currentState != BattleState.Win)
    {
        SetTankControlMode(activeTank2, true);
        StartCoroutine(AIRoutine());
    }
    else if (nextPlayer == BattleState.Player1Turn)
    {
        SetTankControlMode(activeTank1, false);
        
        // Ensure the drawer is updated with Player 1's unique inventory
        if (weaponDrawer != null)
        {
            TankShooting ts = activeTank1.GetComponentInChildren<TankShooting>();
            if (ts != null) weaponDrawer.SetupDrawerForActiveTank(ts);
        }
    }
}

    private void SetTankControlMode(GameObject tank, bool isAI)
    {
        if (tank == null) return;
        
        var movement = tank.GetComponent<TankMovement>();
        var shooting = tank.GetComponentInChildren<TankShooting>();
        var aim = tank.GetComponentInChildren<TankAim>();

        if (movement) movement.isAI = isAI;
        if (shooting) shooting.isAI = isAI;
        if (aim) aim.isAI = isAI;
    }

    private void CalculateMissileStats(TankShooting ts)
    {
        if (ts == null) return;

        float g = Mathf.Abs(Physics2D.gravity.y);

        foreach (var m in ts.inventory)
        {
            if (m.isLaser) continue;

            // Range = (v^2 * sin(2 * theta)) / g
            // Max range occurs at 45 degrees
            float v = ts.maxPower * m.speedMultiplier;
            float gravity = g * m.gravityScale;
            m.runtimeMaxRange = (v * v * Mathf.Sin(2 * 45f * Mathf.Deg2Rad)) / gravity;
        }
    }

    IEnumerator AIRoutine()
    {
        // Wait for the thinking time before taking any action
        yield return new WaitForSeconds(aiThinkTime);

        TankShooting ts = activeTank2.GetComponentInChildren<TankShooting>();
        TankAim ta = activeTank2.GetComponentInChildren<TankAim>();
        TankMovement tm = activeTank2.GetComponent<TankMovement>();
        
        if (ts == null || ta == null || tm == null) yield break;

        // Calculate missile ranges now that the tank is definitely assigned and active
        CalculateMissileStats(ts);

        // --- 1. MISSILE SELECTION & MOVEMENT (S-Distance Logic) ---
        float currentDist = Mathf.Abs(activeTank1.transform.position.x - activeTank2.transform.position.x);
        int bestMissileIndex = 0;
        float closestDiff = float.MaxValue;

        // Find the best missile based on the "Sweet Spot" (2/3 of Max Range)
        for (int i = 0; i < ts.inventory.Count; i++)
        {
            if (ts.inventory[i].isLaser) continue; 

            float midRange = ts.inventory[i].runtimeMaxRange * (2f / 3f);
            float diff = Mathf.Abs(currentDist - midRange);
            if (diff < closestDiff)
            {
                closestDiff = diff;
                bestMissileIndex = i;
            }
        }

        ts.selectedMissile = ts.inventory[bestMissileIndex];
        
        // Calculate S (Movement Difference) only if it's a projectile
        if (!ts.selectedMissile.isLaser)
        {
            float sweetSpot = ts.selectedMissile.runtimeMaxRange * (2f / 3f);
            float S = currentDist - sweetSpot; 
            
            float directionToPlayer = Mathf.Sign(activeTank1.transform.position.x - activeTank2.transform.position.x);
            float targetX = activeTank2.transform.position.x + (directionToPlayer * S);

            // Drive to the calculated position (Naturally limited by fuel)
            while (Mathf.Abs(activeTank2.transform.position.x - targetX) > 0.5f && activeTank2.GetComponent<TankFuel>().currentFuel > 0)
            {
                tm.aiMoveInput = (activeTank2.transform.position.x < targetX) ? 1f : -1f;
                yield return null;
            }
        }
        tm.aiMoveInput = 0f; // Stop movement

        // --- 2. AIMING (High Angle Preference) ---
        float newDist = Mathf.Abs(activeTank1.transform.position.x - activeTank2.transform.position.x);
        float targetAngle = 35f;
        float targetPower = ts.minPower;

        if (ts.selectedMissile.isLaser)
        {
            // Laser points directly at the target
            targetAngle = 0f; 
            targetPower = ts.maxPower;
        }
        else
        {
            // Projectile: Sweep from 60 down to 20 to find the first (highest) solution
            bool foundSolution = false;
            for (float a = 60f; a >= 20f; a -= 2f)
            {
                for (float p = ts.minPower; p <= ts.maxPower; p += 1f)
                {
                    if (SimulateRange(a, p, ts.selectedMissile) >= newDist)
                    {
                        targetAngle = a;
                        targetPower = p;
                        foundSolution = true;
                        break;
                    }
                }
                if (foundSolution) break;
            }
        }

        // --- 3. DIFFICULTY OFFSET ---
        float finalPower = targetPower;
        if (!ts.selectedMissile.isLaser)
        {
            float errorRange = 2f; // Default for Easy
            if (GameManager.Instance != null)
            {
                if (GameManager.Instance.currentDifficulty == GameManager.Difficulty.Hard) errorRange = 0f;
                else if (GameManager.Instance.currentDifficulty == GameManager.Difficulty.Medium) errorRange = 1.2f;
            }
            finalPower += Random.Range(-errorRange, errorRange);
        }

        // --- 4. EXECUTION ---
        ts.forceShowTrajectory = true; 

        // Visual synchronization of barrel and power
        while (Mathf.Abs(ta.currentAngle - targetAngle) > 0.5f || Mathf.Abs(ts.currentPower - finalPower) > 0.5f)
        {
            ta.aiAimInput = (ta.currentAngle < targetAngle) ? 1f : -1f;
            ts.aiPowerInput = (ts.currentPower < finalPower) ? 1f : -1f;
            yield return null;
        }

        ta.aiAimInput = 0f;
        ts.aiPowerInput = 0f;

        yield return new WaitForSeconds(0.8f);

        ts.forceShowTrajectory = false;
        ts.AIShoot();
    }

    private float SimulateRange(float angle, float power, MissileData m)
{
    float v = power * m.speedMultiplier;
    float g = Mathf.Abs(Physics2D.gravity.y) * m.gravityScale;
    float rad = angle * Mathf.Deg2Rad;
    
    // Theoretical Physics Formula
    float theoreticalRange = (v * v * Mathf.Sin(2 * rad)) / g;

    // --- CALIBRATION ---
    // Change this value to align the AI's math with your game's physics.
    // 1.0 = Default
    // 1.05 = AI will shoot 5% harder (use if AI is undershooting)
    // 0.95 = AI will shoot 5% softer (use if AI is overshooting)
    float calibration = 1.03f; 

    return theoreticalRange * calibration;
}
}