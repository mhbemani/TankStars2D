using UnityEngine;
using UnityEngine.UI;

public class ManagerSpawner : MonoBehaviour
{
    [Header("UI References (Drag from Hierarchy)")]
    public Text timerText; 
    public Image timerCircle;
    public Image p1FuelBar; 
    public Image p1HealthBar; 
    public Image p2FuelBar; 
    public Image p2HealthBar; 
    public Text announcementText;
    public WeaponDrawer weaponDrawer;
    public CameraController camController;

    void Start()
    {
        if (GameManager.Instance != null)
        {
            GameObject managerPrefab = GameManager.Instance.GetRequiredManagerPrefab();

            if (managerPrefab != null)
            {
                // 1. Spawn the manager
                GameObject spawnedObj = Instantiate(managerPrefab);
                BattleManager bm = spawnedObj.GetComponent<BattleManager>();

                // 2. CONNECT THE WIRES
                if (bm != null)
                {
                    bm.timerText = timerText;
                    bm.timerCircle = timerCircle;
                    bm.p1FuelBar = p1FuelBar;
                    bm.p1HealthBar = p1HealthBar;
                    bm.p2FuelBar = p2FuelBar;
                    bm.p2HealthBar = p2HealthBar;
                    // bm.announcementText = announcementText;
                    bm.weaponDrawer = weaponDrawer;
                    bm.camController = camController;
                    
                    Debug.Log("<color=green>SPAWNER: Linked all UI wires to " + managerPrefab.name + "</color>");
                }
            }
            else
            {
                Debug.LogError("SPAWNER: Prefab slot in GameManager is empty!");
            }
        }
        else
        {
            Debug.LogError("SPAWNER: No GameManager found. Start from MainMenu!");
        }
    }
}