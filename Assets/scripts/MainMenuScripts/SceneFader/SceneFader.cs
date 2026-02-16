using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance;

    [Header("References")]
    public RectTransform faderCircle; // Drag your FaderCircle here
    public float transitionTime = 1.0f;
    public float targetScale = 1f; // Scale needed to cover the whole screen

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject.transform.root.gameObject);
        }
        else { Destroy(gameObject); }
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(TransitionSequence(sceneName));
    }

    IEnumerator TransitionSequence(string sceneName)
    {
        // 1. Circle grows to black
        yield return StartCoroutine(ScaleCircle(0, targetScale));

        // 2. Load the scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        
        // Wait a tiny bit for the new scene to initialize
        yield return new WaitForSeconds(0.1f);

        // 3. Circle shrinks back to 0
        yield return StartCoroutine(ScaleCircle(targetScale, 0));
    }

    IEnumerator ScaleCircle(float start, float end)
    {
        float t = 0;
        while (t < transitionTime)
        {
            t += Time.deltaTime;
            // Use SmoothStep for a nice "pop" feel
            float currentScale = Mathf.SmoothStep(start, end, t / transitionTime);
            faderCircle.localScale = new Vector3(currentScale, currentScale, 1);
            yield return null;
        }
        faderCircle.localScale = new Vector3(end, end, 1);
    }
}