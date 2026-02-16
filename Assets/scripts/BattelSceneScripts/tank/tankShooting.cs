using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class TankShooting : MonoBehaviour
{
    [Header("Control Mode")]
    public bool isAI = false;             
    public float aiPowerInput = 0f;       
    public bool forceShowTrajectory = false; 

    [HideInInspector] public MissileData selectedMissile;
    public Transform firePoint;
    private LineRenderer trajectoryLine;

    [Header("Inventory Management")]
    public List<MissileData> inventory; 
    private List<MissileData> masterInstances = new List<MissileData>();
    // This stores the "Starting Ammo" for each instance so we can refill correctly
    private Dictionary<MissileData, int> ammoDefaults = new Dictionary<MissileData, int>();

    [Header("Power Settings")]
    public float currentPower = 15f; 
    public float minPower = 5f;    
    public float maxPower = 35f;   
    public float powerChangeSpeed = 20f;

    [Header("Alignment")]
    public float angleCorrection = 0f; 

    [Header("Visuals")]
    public float maxVisualDistance = 10f; 
    public int resolution = 30; 

    [Header("Animation")]
    public Animator barrelAnimator;

    private bool isAiming = false;

    void Awake()
    {
        trajectoryLine = GetComponent<LineRenderer>();
        SetupDottedLine();
        trajectoryLine.enabled = false; 
    }

    void Start()
    {
        // 1. Setup the Master Instances
        masterInstances = new List<MissileData>();
        ammoDefaults = new Dictionary<MissileData, int>();

        foreach (MissileData m in inventory)
        {
            if (m != null)
            {
                MissileData instance = Instantiate(m);
                masterInstances.Add(instance);
                
                // Remember what the "Full" ammo count was from the Inspector
                ammoDefaults.Add(instance, instance.ammoCount);
            }
        }

        // 2. Initial fill
        RefillInventory();
    }

    public void RefillInventory()
    {
        inventory.Clear();
        foreach (MissileData master in masterInstances)
        {
            // Restore the ammoCount from our saved defaults
            if (ammoDefaults.ContainsKey(master))
            {
                master.ammoCount = ammoDefaults[master];
            }
            
            inventory.Add(master);
        }

        if (inventory.Count > 0) selectedMissile = inventory[0];
        
        UpdateWeaponUI();
        Debug.Log(gameObject.name + " inventory refilled to original counts.");
    }

    void SetupDottedLine()
    {
        trajectoryLine.useWorldSpace = true;
        trajectoryLine.startWidth = 0.2f;
        trajectoryLine.endWidth = 0.2f;
        trajectoryLine.sortingOrder = 10;
        
        Texture2D dotTex = new Texture2D(2, 1);
        dotTex.SetPixel(0, 0, Color.white);
        dotTex.SetPixel(1, 0, Color.clear);
        dotTex.Apply();

        trajectoryLine.material = new Material(Shader.Find("Sprites/Default"));
        trajectoryLine.material.mainTexture = dotTex;
        trajectoryLine.textureMode = LineTextureMode.RepeatPerSegment;
    }

    void Update()
    {
        TankMovement parentTank = GetComponentInParent<TankMovement>();
        if (parentTank != null && BattleManager.Instance != null)
        {
            bool isMyTurn = (parentTank.playerID == 1 && BattleManager.Instance.currentState == BattleManager.BattleState.Player1Turn) ||
                            (parentTank.playerID == 2 && BattleManager.Instance.currentState == BattleManager.BattleState.Player2Turn);
    
            if (!isMyTurn)
            {
                if (trajectoryLine.enabled) trajectoryLine.enabled = false;
                isAiming = false; 
                return;
            }
        }
        
        SyncFirePointDirection();

        if (!isAI)
        {
            if (Keyboard.current.qKey.isPressed || Keyboard.current.eKey.isPressed || 
                Keyboard.current.wKey.isPressed || Keyboard.current.sKey.isPressed)
                isAiming = true;

            bool isMoving = Keyboard.current.aKey.isPressed || Keyboard.current.dKey.isPressed;
            
            if (isMoving) 
            {
                isAiming = false;
                trajectoryLine.enabled = false;
            }
            else if (isAiming) 
            {
                trajectoryLine.enabled = true;
                HandlePowerInput();
                DrawTrajectory();
            }

            if (Keyboard.current.spaceKey.wasPressedThisFrame) StartCoroutine(FireRoutine());
        }
        else
        {
            currentPower = Mathf.Clamp(currentPower + aiPowerInput * Time.deltaTime * powerChangeSpeed, minPower, maxPower);
            trajectoryLine.enabled = forceShowTrajectory;
            if (forceShowTrajectory) DrawTrajectory();
        }
    }

    public void AIShoot() => StartCoroutine(FireRoutine());

    void SyncFirePointDirection()
    {
        if (firePoint == null) return;
        float tankFacing = transform.root.localScale.x;

        if (tankFacing > 0)
            firePoint.localEulerAngles = Vector3.zero;
        else
            firePoint.localEulerAngles = new Vector3(0, 180, 0);

        if (barrelAnimator != null)
        {
            float scaleX = Mathf.Abs(barrelAnimator.transform.localScale.x) * Mathf.Sign(tankFacing);
            barrelAnimator.transform.localScale = new Vector3(scaleX, barrelAnimator.transform.localScale.y, 1f);
        }
    }

    void HandlePowerInput()
    {
        if (Keyboard.current.eKey.isPressed)
            currentPower = Mathf.Clamp(currentPower + Time.deltaTime * powerChangeSpeed, minPower, maxPower);
        if (Keyboard.current.qKey.isPressed)
            currentPower = Mathf.Clamp(currentPower - Time.deltaTime * powerChangeSpeed, minPower, maxPower);
    }

    void DrawTrajectory()
    {
        if (selectedMissile == null) return;
        Vector2 startPos = firePoint.position;
        Vector2 correctedDir = Quaternion.Euler(0, 0, angleCorrection) * firePoint.right;
        Vector2 velocity = correctedDir * (currentPower * selectedMissile.speedMultiplier);
        trajectoryLine.positionCount = resolution;

        for (int i = 0; i < resolution; i++)
        {
            float t = i * 0.06f; 
            Vector2 pos = startPos + (velocity * t) + (0.5f * (Physics2D.gravity * selectedMissile.gravityScale) * t * t);
            if (Vector2.Distance(startPos, pos) > maxVisualDistance)
            {
                trajectoryLine.positionCount = i;
                break;
            }
            trajectoryLine.SetPosition(i, new Vector3(pos.x, pos.y, -1f));
        }
    }

    public IEnumerator FireRoutine()
    {
        if (selectedMissile == null || selectedMissile.ammoCount <= 0) yield break;

        trajectoryLine.enabled = false;

        if (BattleManager.Instance != null)
        {
            TankMovement parent = GetComponentInParent<TankMovement>();
            int myID = (parent != null) ? parent.playerID : 1;
            BattleManager.Instance.OnShotFired(myID);
        }

        if (barrelAnimator != null) barrelAnimator.SetTrigger("Shoot");

        MissileData weaponToUse = selectedMissile;
        int burst = weaponToUse.burstCount > 0 ? weaponToUse.burstCount : 1;
        
        weaponToUse.ammoCount--;

        // Check for next weapon or refill immediately
        AutoSelectNextWeapon();

        for (int i = 0; i < burst; i++)
        {
            SpawnMissile(weaponToUse);
            if (burst > 1) yield return new WaitForSeconds(0.12f);
        }

        UpdateWeaponUI();

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.FinishTurnAfterDelay(2.0f);
        }
    }

    public void AutoSelectNextWeapon()
    {
        if (selectedMissile != null && selectedMissile.ammoCount > 0) return;

        bool foundAmmo = false;
        foreach (MissileData m in inventory)
        {
            if (m != null && m.ammoCount > 0)
            {
                selectedMissile = m;
                foundAmmo = true;
                break;
            }
        }

        if (!foundAmmo)
        {
            RefillInventory();
        }

        UpdateWeaponUI();
    }

    private void UpdateWeaponUI()
    {
        try 
        {
            WeaponDrawer drawer = Object.FindFirstObjectByType<WeaponDrawer>();
            if (drawer != null) drawer.UpdateDisplay();
        }
        catch { }
    }

    void SpawnMissile(MissileData data)
    {
        GameObject missile = Instantiate(data.missilePrefab, firePoint.position, firePoint.rotation);
        MissileExplosion missileScript = missile.GetComponent<MissileExplosion>();
        if (missileScript != null) missileScript.data = data;

        Vector3 prefabScale = data.missilePrefab.transform.localScale;
        missile.transform.localScale = new Vector3(Mathf.Abs(prefabScale.x), Mathf.Abs(prefabScale.y), prefabScale.z);

        Rigidbody2D rb = missile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = data.gravityScale;
            Vector2 correctedDir = Quaternion.Euler(0, 0, angleCorrection) * firePoint.right;
            Vector2 initialVelocity = correctedDir * (currentPower * data.speedMultiplier);
            rb.linearVelocity = initialVelocity;
            float angle = Mathf.Atan2(initialVelocity.y, initialVelocity.x) * Mathf.Rad2Deg;
            missile.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}