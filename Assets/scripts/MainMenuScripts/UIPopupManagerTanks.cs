using UnityEngine;
using System.Collections;

public class UIPopupManagerTanks : MonoBehaviour
{
    [Header("References")]
    public GameObject dismissBlocker; // The Root (Background Dimmer/Blocker)
    public Transform popupContent;     // The actual Window (the box that scales)

    [Header("Settings")]
    public float animDuration = 0.2f;

    // --- YOUR EXISTING METHODS (Keep these for Shop/Coming Soon) ---

    public void OpenPopup()
    {
        StopAllCoroutines();
        dismissBlocker.SetActive(true);
        StartCoroutine(AnimatePopup(Vector3.zero, Vector3.one, false));
    }

    public void ClosePopup()
    {
        StopAllCoroutines();
        StartCoroutine(AnimatePopup(Vector3.one, Vector3.zero, true));
    }

    // --- NEW METHOD: Specifically for the MenuController to call ---
    // This allows the MenuController to close the window after a tank is picked
    public void CloseTankSelection()
    {
        ClosePopup();
    }

    IEnumerator AnimatePopup(Vector3 startScale, Vector3 endScale, bool hideRootAtEnd)
    {
        float elapsed = 0;
        popupContent.localScale = startScale;

        while (elapsed < animDuration)
        {
            popupContent.localScale = Vector3.Lerp(startScale, endScale, elapsed / animDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        popupContent.localScale = endScale;

        if (hideRootAtEnd)
        {
            dismissBlocker.SetActive(false);
        }
    }
}