using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class WeaponDrawer : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform drawerRect;     
    public GameObject missileItemPrefab; 
    public Image currentWeaponIcon;
    public GameObject blocker;      

    [Header("Data Source")]
    public TankShooting playerTank;      

    [Header("Mode Settings")]
    public bool allMissilesOneShot = false;

    [Header("Sliding Settings")]
    public Vector2 hiddenPos; 
    public Vector2 shownPos;  
    public float slideSpeed = 10f;
    
    public GameObject weaponPanelGroupObject; // Parent object for the entire weapon panel (for easy show/hide)
    private bool isShown = false;

    // Use a Dictionary or a simple list to store the 'Master' starting amounts
    private List<int> masterAmmoValues = new List<int>(); 

    // 1. Remove logic from Start()
void Start()
{
    drawerRect.anchoredPosition = hiddenPos;
    if (blocker != null) blocker.SetActive(false);
}

// 2. Add this function for the BattleManager to call
public void SetupDrawerForActiveTank(TankShooting activeTank)
{
    Debug.Log("Drawer Setup called for: " + activeTank.gameObject.name);
    playerTank = activeTank;

    if (playerTank != null && playerTank.inventory.Count > 0)
    {
        masterAmmoValues.Clear();
        for (int i = 0; i < playerTank.inventory.Count; i++)
        {
            masterAmmoValues.Add(playerTank.inventory[i].ammoCount);
            if (allMissilesOneShot) playerTank.inventory[i].ammoCount = 1;
        }
        
        PickRandomAvailableWeapon();
        UpdateDisplay(); // This draws the buttons
    }
}

    public void UpdateDisplay()
{
    if (playerTank == null) return;

    // 1. Check for refill
    if (IsInventoryEmpty())
    {
        RefillAllAmmo();
    }

    // 2. Auto-switch weapon ONLY if the current one is truly empty
    // This will now only trigger after FireRoutine finishes
    if (playerTank.selectedMissile != null && playerTank.selectedMissile.ammoCount <= 0)
    {
        PickRandomAvailableWeapon();
    }

    // 3. Rebuild UI Buttons
    foreach (Transform child in drawerRect)
    {
        if (child.GetComponent<Button>() != null) Destroy(child.gameObject);
    }

    foreach (MissileData data in playerTank.inventory)
    {
        if (data.ammoCount <= 0) continue;

        GameObject newButton = Instantiate(missileItemPrefab, drawerRect);
        newButton.transform.Find("MissileName").GetComponent<TextMeshProUGUI>().text = data.missileName;
        newButton.transform.Find("AmmoCount").GetComponent<TextMeshProUGUI>().text = "x" + data.ammoCount;
        newButton.transform.Find("Icon").GetComponent<Image>().sprite = data.missileIcon;

        newButton.GetComponent<Button>().onClick.AddListener(() => SelectMissile(data));
    }
}

    void RefillAllAmmo()
    {
        for (int i = 0; i < playerTank.inventory.Count; i++)
        {
            playerTank.inventory[i].ammoCount = allMissilesOneShot ? 1 : masterAmmoValues[i];
        }
        // Note: We don't call UpdateDisplay here to avoid infinite loops
    }

    void PickRandomAvailableWeapon()
    {
        List<MissileData> available = new List<MissileData>();
        foreach (var m in playerTank.inventory)
        {
            if (m.ammoCount > 0) available.Add(m);
        }

        if (available.Count > 0)
        {
            int randomIndex = Random.Range(0, available.Count);
            SetSelectedWeapon(available[randomIndex]);
        }
    }

    bool IsInventoryEmpty()
    {
        foreach (var m in playerTank.inventory)
        {
            if (m.ammoCount > 0) return false;
        }
        return true;
    }

    void SetSelectedWeapon(MissileData data)
{
    playerTank.selectedMissile = data;
    currentWeaponIcon.sprite = data.missileIcon;
    currentWeaponIcon.color = Color.white;

    // --- NEW: SWAP BARREL ANIMATOR ---
    // We check if the tank has a barrel animator slot and if the missile has a controller
    if (playerTank.barrelAnimator != null)
    {
        if (data.muzzleFlashController != null)
        {
            playerTank.barrelAnimator.runtimeAnimatorController = data.muzzleFlashController;
            Debug.Log("Swapped barrel animator to: " + data.missileName);
        }
        else
        {
            Debug.LogWarning("Missile " + data.missileName + " is missing an Animator Controller!");
        }
    }
}

    public void ToggleDrawer()
    {
        isShown = !isShown;
        if (blocker != null) blocker.SetActive(isShown);
        StopAllCoroutines(); 
        StartCoroutine(SlideToTarget(isShown ? shownPos : hiddenPos));
        if (isShown) UpdateDisplay();
    }

    IEnumerator SlideToTarget(Vector2 target)
    {
        while (Vector2.Distance(drawerRect.anchoredPosition, target) > 0.1f)
        {
            drawerRect.anchoredPosition = Vector2.Lerp(drawerRect.anchoredPosition, target, Time.deltaTime * slideSpeed);
            yield return null;
        }
        drawerRect.anchoredPosition = target;
    }

    void SelectMissile(MissileData data)
    {
        SetSelectedWeapon(data);
        ToggleDrawer(); 
    }

    public void SetVisible(bool isVisible)
    {
        if (weaponPanelGroupObject != null)
        {
            weaponPanelGroupObject.SetActive(isVisible);
        }
    }
}