using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;
    public enum BattleState { Setup, Intro, Player1Turn, Player2Turn, Paused, Resolution, Win }
    public BattleState currentState;

    [Header("Settings")]
    public LayerMask groundLayer;
    public CameraController camController;
    public float turnDuration = 20f;
    public float timer;

    [Header("UI Assignments")]
    public Text timerText; 
    public Image timerCircle;
    public Image p1FuelBar; 
    public Image p1HealthBar; 
    public Image p2FuelBar; 
    public Image p2HealthBar; 
    // public Text announcementText; 
    public WeaponDrawer weaponDrawer; 

    public Animator introAnimator;

    [HideInInspector] public GameObject activeTank1;
    [HideInInspector] public GameObject activeTank2;

    private int lastPlayerID = 1; 

    void Awake() => Instance = this;

    protected virtual void Start()
{
    // 1. Find the IntroAnnouncer even if it is disabled in the scene
    if (introAnimator == null)
    {
        // We look through all GameObjects in the scene memory
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            // Match the name exactly as it appears in your Hierarchy
            if (obj.name == "IntroAnnouncer")
            {
                introAnimator = obj.GetComponent<Animator>();
                break;
            }
        }
    }

    // 2. Start the countdown sequence
    if (introAnimator != null)
    {
        StartCoroutine(BeginBattleFlow());
    }
    else
    {
        Debug.LogError("BattleManager: Could not find 'IntroAnnouncer' in the scene! Check the name.");
        // Start anyway so the game doesn't freeze
        StartTurn(BattleState.Player1Turn);
    }
}

    void Update()
    {
        // Only run the timer during actual player turns
        if (currentState == BattleState.Player1Turn || currentState == BattleState.Player2Turn)
        {
            timer -= Time.deltaTime;
            
            // UI Updates
            if (timerText != null) timerText.text = Mathf.CeilToInt(timer).ToString();
            if (timerCircle != null) timerCircle.fillAmount = timer / turnDuration;

            // Handle Timeout
            if (timer <= 0) 
            {
                timer = 0;
                lastPlayerID = (currentState == BattleState.Player1Turn) ? 1 : 2;
                EndTurn();
            }
        }
    }

    IEnumerator BeginBattleFlow()
{
    currentState = BattleState.Setup;
    PrepareBattle();

    if (introAnimator != null)
    {
        // Show the object
        introAnimator.gameObject.SetActive(true);
        
        // Ensure it plays the specific animation clip name you created
        introAnimator.Play("Intro_Countdown", 0, 0f);
    }

    // Wait for the duration of 3-2-1-GO (4 seconds)
    yield return new WaitForSeconds(3.1f);

    if (introAnimator != null)
    {
        // Hide it so it's not blocking the view during the game
        introAnimator.gameObject.SetActive(false);
    }

    // Begin the first turn
    StartTurn(BattleState.Player1Turn);
}

    protected virtual void StartTurn(BattleState nextPlayer)
    {
        if (currentState == BattleState.Win) return;

        currentState = nextPlayer;
        timer = turnDuration; 

        // Identify which tank is moving
        GameObject currentActive = (currentState == BattleState.Player1Turn) ? activeTank1 : activeTank2;
        GameObject inactiveTank = (currentState == BattleState.Player1Turn) ? activeTank2 : activeTank1;

        // Enable active tank controls
        if (currentActive != null)
        {
            currentActive.GetComponent<TankMovement>().enabled = true;
            TankShooting shooting = currentActive.GetComponentInChildren<TankShooting>();
            
            if (shooting != null)
            {
                shooting.enabled = true;
                // Update the UI Weapon Drawer for the new active tank
                if (weaponDrawer != null) weaponDrawer.SetupDrawerForActiveTank(shooting);
            }

            // Refill fuel for the new turn
            TankFuel fuel = currentActive.GetComponent<TankFuel>();
            if (fuel != null) fuel.currentFuel = fuel.maxFuel;
        }

        // Safety: Disable controls for the tank that just finished its turn
        if (inactiveTank != null)
        {
            inactiveTank.GetComponent<TankMovement>().enabled = false;
            TankShooting inactiveShooting = inactiveTank.GetComponentInChildren<TankShooting>();
            if (inactiveShooting != null) inactiveShooting.enabled = false;
        }
    }

    public void OnShotFired(int playerID)
    {
        // Prevents double-firing logic if the shot takes time to resolve
        if (currentState == BattleState.Resolution || currentState == BattleState.Win) return;
        
        lastPlayerID = playerID;
        currentState = BattleState.Resolution;
    }

    public void FinishTurnAfterDelay(float delay)
    {
        if (currentState == BattleState.Win) return;
        StartCoroutine(WaitAndEndTurn(delay));
    }

    private IEnumerator WaitAndEndTurn(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (currentState != BattleState.Win) EndTurn();
    }

    public void EndTurn()
    {
        if (currentState == BattleState.Win) return;

        // Swap turns based on who just went
        if (lastPlayerID == 1) StartTurn(BattleState.Player2Turn);
        else StartTurn(BattleState.Player1Turn);
    }

    void PrepareBattle()
    {
        if (GameManager.Instance == null) return;

        float spawnXCenter = (camController != null) ? camController.GetRandomSpawnCenter() : 0f;

        // Spawn P1
        activeTank1 = SpawnTank(GameManager.Instance.player1Prefab, spawnXCenter - 25f, false);
        InitializeTankUI(activeTank1, 1, p1FuelBar, p1HealthBar);

        // Spawn P2
        activeTank2 = SpawnTank(GameManager.Instance.player2Prefab, spawnXCenter + 25f, true);
        InitializeTankUI(activeTank2, 2, p2FuelBar, p2HealthBar);

        // Link to Camera
        if (camController != null)
        {
            camController.player1 = activeTank1.transform;
            camController.player2 = activeTank2.transform;
            
            // Center camera between tanks immediately
            float midX = (activeTank1.transform.position.x + activeTank2.transform.position.x) / 2f;
            camController.transform.position = new Vector3(midX, activeTank1.transform.position.y, camController.transform.position.z);
        }
    }

    private void InitializeTankUI(GameObject tank, int id, Image fuelBar, Image healthBar)
    {
        tank.GetComponent<TankMovement>().playerID = id;
        tank.GetComponent<TankFuel>().fuelBarImage = fuelBar;
        Health h = tank.GetComponent<Health>();
        if (h != null) h.healthBarImage = healthBar;
    }

    GameObject SpawnTank(GameObject prefab, float x, bool flip)
    {
        float currentX = x;
        Vector3 spawnPoint = Vector3.zero;
        Quaternion spawnRot = Quaternion.identity;
        bool foundSafeSpot = false;
        int attempts = 0;
        int maxAttempts = 15; 
        int objectsLayer = LayerMask.NameToLayer("object");

        while (!foundSafeSpot && attempts < maxAttempts)
        {
            RaycastHit2D hit = Physics2D.Raycast(new Vector2(currentX, 50f), Vector2.down, 100f, groundLayer);
            
            if (hit.collider != null)
            {
                // Check if we hit a Rock (via Layer or Tag)
                if (hit.collider.gameObject.layer == objectsLayer || hit.collider.CompareTag("Rock"))
                {
                    currentX += 10f; // Shift right 10 units
                    attempts++;
                }
                else
                {
                    // Found ground! Add small Y offset to prevent stuck colliders
                    spawnPoint = new Vector3(hit.point.x, hit.point.y + 1.5f, 0);
                    float angle = Mathf.Atan2(hit.normal.y, hit.normal.x) * Mathf.Rad2Deg - 90f;
                    spawnRot = Quaternion.Euler(0, 0, angle);
                    foundSafeSpot = true;
                }
            }
            else
            {
                spawnPoint = new Vector3(currentX, 10f, 0);
                foundSafeSpot = true;
            }
        }

        GameObject tank = Instantiate(prefab, spawnPoint, spawnRot);
        if (flip) tank.transform.localScale = new Vector3(-1, 1, 1);
        
        return tank;
    }

    public void CheckGameOver(int deadPlayerID)
    {
        if (currentState == BattleState.Win) return;
        currentState = BattleState.Win;

        int winner = (deadPlayerID == 1) ? 2 : 1;
        // if(announcementText != null) announcementText.text = "PLAYER " + winner + " WINS!";
        StartCoroutine(ReturnToMenuDelayed(4f));
    }

    IEnumerator ReturnToMenuDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (SceneFader.Instance != null) SceneFader.Instance.LoadScene("MainMenu"); 
    }
}