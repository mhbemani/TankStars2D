using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;
    private bool isDead = false;

    // NEW: We need to know who this tank belongs to
    private int playerID; 

    [Header("UI Visuals")]
    public Image healthBarImage;    
    public float lerpSpeed = 5f;    

    [Header("Death Effects")]
    public GameObject deathExplosionPrefab;
    public float deathDelay = 2.0f; 
    public float shakeIntensity = 0.1f;

    public GameObject damageTextPrefab;

    [Header("Damage Visuals")]
    public GameObject fireEffectObject; 
    public float damageThreshold = 20f;

    private float targetFillAmount = 1f;

    void Start()
    {
        currentHealth = maxHealth;
        targetFillAmount = 1f;
        if (healthBarImage != null) healthBarImage.fillAmount = 1f;

        // NEW: Automatically grab the PlayerID from the TankMovement script on Start
        TankMovement move = GetComponent<TankMovement>();
        if (move != null) playerID = move.playerID;
    }

    void Update()
    {
        if (healthBarImage != null)
        {
            healthBarImage.fillAmount = Mathf.Lerp(healthBarImage.fillAmount, targetFillAmount, Time.deltaTime * lerpSpeed);
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return; 

        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;

        targetFillAmount = currentHealth / maxHealth;

        Vector3 spawnOffset = new Vector3(0, 2.5f, 0);
        GameObject textObj = Instantiate(damageTextPrefab, transform.position + spawnOffset, Quaternion.identity);
        textObj.GetComponentInChildren<Text>().text = "-" + amount;

        if (currentHealth <= damageThreshold && currentHealth > 0)
        {
            if (fireEffectObject != null) fireEffectObject.SetActive(true); 
        }

        if (currentHealth <= 0)
        {
            if (fireEffectObject != null) fireEffectObject.SetActive(false);
            StartCoroutine(DeathSequence());
        }
    }

    IEnumerator DeathSequence()
    {
        isDead = true;

        // NEW: Tell BattleManager immediately that this tank is dying 
        // to stop turn switching and timer
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.CheckGameOver(playerID);
        }

        Vector3 finalExplosionPos = transform.position; 

        float timer = 0;
        while (timer < deathDelay)
        {
            float x = Random.Range(-1f, 1f) * shakeIntensity;
            float y = Random.Range(-1f, 1f) * shakeIntensity;
            transform.position = finalExplosionPos + new Vector3(x, y, 0);
            timer += Time.deltaTime;
            yield return null;
        }

        if (deathExplosionPrefab != null)
        {
            GameObject explosion = Instantiate(deathExplosionPrefab, finalExplosionPos, Quaternion.identity);
            var sr = explosion.GetComponent<SpriteRenderer>();
            if(sr != null) {
                sr.sortingLayerName = "Foreground"; 
                sr.sortingOrder = 50; 
            }
        }

        Destroy(gameObject);
    }
}