using UnityEngine;

public class MenuController : MonoBehaviour
{
    [Header("Inventory & Preview")]
    public GameObject[] tankInventory;   // Drag Abrams, T34, Tiger prefabs here
    public Transform tankPreviewParent;  // The UI Anchor/Box in the center

    [Header("UI Reference")]
    public UIPopupManagerTanks popupManager; // Drag the _MenuManager object here

    private GameObject currentPreview; // Keeps track of the spawned tank in the menu

    void Start()
    {
        // Automatically spawn the first tank as a default
        if (tankInventory.Length > 0)
        {
            SelectTank(0);
        }
    }

    // This is what your Tank Card Buttons call
    public void SelectTank(int index)
    {
        if (index < 0 || index >= tankInventory.Length) return;

        GameObject chosenPrefab = tankInventory[index];

        // 1. Assign to GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.player1Prefab = chosenPrefab;
            
            // Assign a random opponent
            int randomIndex = Random.Range(0, tankInventory.Length);
            GameManager.Instance.player2Prefab = tankInventory[randomIndex];
        }

        // 2. Visual Preview
        SpawnTankPreview(index);

        // 3. Close the Selection Window
        if (popupManager != null)
        {
            popupManager.CloseTankSelection();
        }
    }

    public void SpawnTankPreview(int index)
{
    if (currentPreview != null) Destroy(currentPreview);

    // 1. Force the parent to be active so we can see the new tank
    if (tankPreviewParent != null) tankPreviewParent.gameObject.SetActive(true);

    currentPreview = Instantiate(tankInventory[index], tankPreviewParent);

    // 2. Freeze Physics immediately
    if (currentPreview.TryGetComponent(out Rigidbody2D rb2d))
    {
        rb2d.bodyType = RigidbodyType2D.Static;
        rb2d.simulated = false; // Extra safety to stop all physics
    }

    // 3. Disable Logic Scripts (So they don't shoot/log in the menu)
    MonoBehaviour[] allScripts = currentPreview.GetComponentsInChildren<MonoBehaviour>();
    foreach (MonoBehaviour s in allScripts) 
    {
        // Don't disable renderers or the MenuController itself
        if (!(s is SpriteRenderer) && s != this) 
        {
            s.enabled = false; 
        }
    }

    // 4. Reset Position and Scale
    currentPreview.transform.localPosition = Vector3.zero;
    currentPreview.transform.localScale = Vector3.one * 100f; 

    // 5. Visibility Fix: Force Sorting & Layer
    // This ensures that even if the prefab is set to 'Default', it shows on UI
    SetLayerRecursive(currentPreview, LayerMask.NameToLayer("UI"));

    SpriteRenderer[] srs = currentPreview.GetComponentsInChildren<SpriteRenderer>();
    foreach (SpriteRenderer sr in srs) 
    {
        sr.sortingLayerName = "UI"; // Ensure you have a 'UI' sorting layer or use 'Default'
        // sr.sortingOrder = 100;      // Higher than your Background Image
    }
}

// Helper to fix layers on all children (wheels, turrets, etc.)
private void SetLayerRecursive(GameObject obj, int newLayer)
{
    obj.layer = newLayer;
    foreach (Transform child in obj.transform)
    {
        SetLayerRecursive(child.gameObject, newLayer);
    }
}

public void HidePreview()
{
    if (tankPreviewParent != null) tankPreviewParent.gameObject.SetActive(false);
}

// Call this from the Dismiss Blocker or SelectTank
public void ShowPreview()
{
    if (tankPreviewParent != null) tankPreviewParent.gameObject.SetActive(true);
}

// Helper to ensure every part of the tank (turret, treads) is on the UI layer


    // --- Mode Selection & Navigation ---

    public void SelectSoloMode()
    {
        if(GameManager.Instance != null) GameManager.Instance.SetSoloMode(true);
    }

    public void SelectDuoMode()
    {
        if(GameManager.Instance != null) GameManager.Instance.SetSoloMode(false);
    }

    public void PlayGame()
    {
        if (GameManager.Instance != null) GameManager.Instance.StartBattle();
    }

    public void QuitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void SetPreviewVisibility(bool isVisible)
{
    if (tankPreviewParent != null)
    {
        // This turns the entire "box" (and the tank inside it) on or off
        tankPreviewParent.gameObject.SetActive(isVisible);
    }
}
}