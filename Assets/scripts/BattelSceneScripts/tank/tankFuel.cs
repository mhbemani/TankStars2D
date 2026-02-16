using UnityEngine;
using UnityEngine.UI;

public class TankFuel : MonoBehaviour
{
    [Header("Fuel Settings")]
    public float maxFuel = 100f;
    public float fuelBurnRate = 15f; 
    [HideInInspector] public float currentFuel;

    [Header("UI (Only for Player)")]
    public Image fuelBarImage; 
    public float lerpSpeed = 5f;

    private float targetFillAmount = 1f;

    void Start()
    {
        currentFuel = maxFuel;
        if (fuelBarImage != null) fuelBarImage.fillAmount = 1f;
    }

    void Update()
    {
        currentFuel = Mathf.Clamp(currentFuel, 0, maxFuel);

        if (fuelBarImage != null)
        {
            targetFillAmount = currentFuel / maxFuel;
            fuelBarImage.fillAmount = Mathf.Lerp(fuelBarImage.fillAmount, targetFillAmount, Time.deltaTime * lerpSpeed);
        }
    }

    // This is the new function your Movement script will call
    public void ConsumeFuel()
    {
        if (currentFuel > 0)
        {
            currentFuel -= fuelBurnRate * Time.deltaTime;
        }
    }
}