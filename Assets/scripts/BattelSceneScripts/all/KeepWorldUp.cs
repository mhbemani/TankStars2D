using UnityEngine;

public class KeepWorldUp : MonoBehaviour
{
    void LateUpdate()
    {
        // This forces the rotation to stay at 0 (Up) regardless of the tank
        transform.rotation = Quaternion.identity;

        // // This prevents the fire from being squashed/flipped if the tank flips
        // Vector3 parentScale = transform.parent.lossyScale;
        // transform.localScale = new Vector3(1f / parentScale.x, 1f / parentScale.y, 1f);
    }
}