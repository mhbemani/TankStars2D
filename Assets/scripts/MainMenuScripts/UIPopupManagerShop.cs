using UnityEngine;
using System.Collections;

public class UIPopupManagerShop : MonoBehaviour
{
    [Header("References")]
    public GameObject dismissBlocker; // The Root (Background Dimmer/Blocker)
    public Transform popupContent;     // The actual Window (the box that scales)

    [Header("Settings")]
    public float animDuration = 0.2f;

    public void OpenComingSoon()
    {
        StopAllCoroutines();
        
        // 1. Show the background instantly
        dismissBlocker.SetActive(true);
        
        // 2. Animate the window from 0 to 1
        StartCoroutine(AnimatePopup(Vector3.zero, Vector3.one, false));
    }

    public void CloseComingSoon()
    {
        StopAllCoroutines();
        
        // Animate the window from 1 back to 0
        StartCoroutine(AnimatePopup(Vector3.one, Vector3.zero, true));
    }

    IEnumerator AnimatePopup(Vector3 startScale, Vector3 endScale, bool hideRootAtEnd)
    {
        float elapsed = 0;
        popupContent.localScale = startScale;

        while (elapsed < animDuration)
        {
            // Smoothly lerp the scale
            popupContent.localScale = Vector3.Lerp(startScale, endScale, elapsed / animDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        popupContent.localScale = endScale;

        // If we are closing, hide the background dim only after the window is gone
        if (hideRootAtEnd)
        {
            dismissBlocker.SetActive(false);
        }
    }
}