using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            // We use Random.insideUnitSphere to get a random point in a circle
            // Then we multiply by magnitude to control the "strength"
            Vector3 randomPoint = originalPos + (Vector3)Random.insideUnitCircle * magnitude;

            // Apply the random jump immediately
            transform.localPosition = new Vector3(randomPoint.x, randomPoint.y, originalPos.z);

            elapsed += Time.deltaTime;
            
            // This 'yield return null' says "Wait exactly 1 frame" 
            // This ensures we jump to a NEW position 60+ times per second!
            yield return null; 
        }

        // Snap back to exactly where we started
        transform.localPosition = originalPos;
    }
}