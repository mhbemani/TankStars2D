using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Tank Blueprints")]
    public GameObject player1Prefab;
    public GameObject player2Prefab;
    
    [Header("Manager Prefabs")]
    public GameObject duoBattleManagerPrefab;
    public GameObject soloBattleManagerPrefab;

    [Header("Game Settings")]
    public bool isSoloMode;

    // --- DIFFICULTY SYSTEM START ---
    public enum Difficulty { Easy, Medium, Hard }
    
    [Header("Difficulty Settings")]
    public Difficulty currentDifficulty = Difficulty.Medium;

    // Link these to your 3 Menu Buttons
    public void SetDifficultyEasy() 
    { 
        currentDifficulty = Difficulty.Easy; 
        Debug.Log("Difficulty set to: Easy");
    }
    
    public void SetDifficultyMedium() 
    { 
        currentDifficulty = Difficulty.Medium; 
        Debug.Log("Difficulty set to: Medium");
    }
    
    public void SetDifficultyHard() 
    { 
        currentDifficulty = Difficulty.Hard; 
        Debug.Log("Difficulty set to: Hard");
    }

    // Helper method for the SoloBattleManager to determine how much to "miss"
    public float GetAIDisplayError()
    {
        switch (currentDifficulty)
        {
            case Difficulty.Easy: return 12f;   // Big random miss
            case Difficulty.Medium: return 5f; // Small miss
            case Difficulty.Hard: return 1f;   // Very accurate
            default: return 5f;
        }
    }
    // --- DIFFICULTY SYSTEM END ---

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else 
        { 
            Destroy(gameObject); 
        }
    }

    public void SetSoloMode(bool solo)
    {
        isSoloMode = solo;
        Debug.Log("GameManager: Solo Mode is now " + (isSoloMode ? "ON" : "OFF"));
    }

    public void StartBattle()
    {
        Debug.Log("GameManager: Initiating Battle. Solo Mode: " + isSoloMode);

        if (SceneFader.Instance != null)
        {
            SceneFader.Instance.LoadScene("BattleScene");
        }
        else
        {
            SceneManager.LoadScene("BattleScene");
        }
    }

    public GameObject GetRequiredManagerPrefab()
    {
        return isSoloMode ? soloBattleManagerPrefab : duoBattleManagerPrefab;
    }
}