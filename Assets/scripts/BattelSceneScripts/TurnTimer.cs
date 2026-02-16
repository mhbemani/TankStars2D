using UnityEngine;
using UnityEngine.UI; // Required for standard Text and Image

public class TurnTimer : MonoBehaviour
{
    private Image fillCircle;      
    private Text timerText; // Changed from TextMeshProUGUI to standard Text

    [Header("Settings")]
    public float turnDuration = 20f;
    private float currentTime;
    private bool isPaused = false;

    void Awake()
    {
        // 1. Get the Image component on this object
        fillCircle = GetComponent<Image>();

        // 2. Search all children for a component of type Text
        // This is safer than transform.Find if the name is slightly different
        timerText = GetComponentInChildren<Text>();

        // Error checking
        if (fillCircle == null) Debug.LogError("No Image component found on " + gameObject.name);
        if (timerText == null) Debug.LogError("No Text component found in children!");
        else Debug.Log("Found text child: " + timerText.gameObject.name);
    }

    void Start()
    {
        currentTime = turnDuration;
    }

    void Update()
    {
        if (isPaused) return;

        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            
            // Update the radial fill (0.0 to 1.0)
            if (fillCircle != null)
                fillCircle.fillAmount = currentTime / turnDuration;

            // Update the text
            if (timerText != null)
                timerText.text = Mathf.Ceil(currentTime).ToString();

            // Visual feedback: Turn red when time is low
            if (currentTime <= 5f) fillCircle.color = Color.red;
        }
        else
        {
            OnTimeOut();
        }
    }

    void OnTimeOut()
    {
        currentTime = 0;
        if (timerText != null) timerText.text = "0";
        isPaused = true;
        Debug.Log("Time Expired!");
    }

    public void ResetTimer()
    {
        currentTime = turnDuration;
        if (fillCircle != null) fillCircle.color = Color.white;
        isPaused = false;
    }
}