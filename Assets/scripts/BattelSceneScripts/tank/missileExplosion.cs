using System;
using UnityEngine;
using UnityEngine.U2D;

public class MissileExplosion : MonoBehaviour
{
    [Header("Explosion Settings")]
    public float explosionRadius = 2.0f;
    public float explosionForce = 5.0f;
    // public GameObject explosionEffectPrefab;

    public MissileData data;

    void Start()
    {
        GetComponent<Rigidbody2D>().gravityScale = data.gravityScale;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Tree"))
        {
            Debug.Log("Direct hit on tree: " + collision.gameObject.name);
            Destroy(collision.gameObject);
        }
        Explode();
    }

    // Ensure this script has: public MissileData data; assigned by the tank
void Explode()
{
    if (data == null) 
    {
        Debug.LogWarning("Missile exploded without MissileData!");
        Destroy(gameObject);
        return;
    }

    Vector2 explosionPos = transform.position;

    // 1. Instantiate the SPECIFIC explosion effect for this missile type
    if (data.explosionPrefab != null)
        Instantiate(data.explosionPrefab, explosionPos, Quaternion.identity);

    CameraShake shaker = Camera.main.GetComponent<CameraShake>();
    if (shaker != null) StartCoroutine(shaker.Shake(0.3f, 1f));

    // 2. Use data.explosionRadius instead of a hardcoded variable
    Collider2D[] objectsInRange = Physics2D.OverlapCircleAll(explosionPos, data.explosionRadius);

    foreach (Collider2D col in objectsInRange)
    {
        Rigidbody2D rb = col.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 dir = (Vector2)col.transform.position - explosionPos;
            float distance = dir.magnitude;
            if (distance > 0)
            {
                // Force calculation now scales with data.explosionRadius
                float forceMultiplier = (data.explosionRadius - distance) / data.explosionRadius;
                rb.AddForce(dir.normalized * explosionForce * forceMultiplier, ForceMode2D.Impulse);
            }
        }

        // 3. Apply damage from data
        Health h = col.GetComponent<Health>();
        if (h != null) h.TakeDamage(data.damage);

        // Ground deformation (commented out per your snippet, but ready for data)
        // if (col.GetComponent<SpriteShapeController>() != null)
        //     DeformSpriteShape(col.gameObject);

        if (col.CompareTag("Tree")) Destroy(col.gameObject);
    }

    Destroy(gameObject);
}

//     void DeformSpriteShape(GameObject groundObj)
// {
//     SpriteShapeController shape = groundObj.GetComponent<SpriteShapeController>();
//     if (shape == null) return;

//     Spline spline = shape.spline;
//     Vector2 localExplosionPos = shape.transform.InverseTransformPoint(transform.position);
    
//     int firstPointInside = -1;
//     int lastPointInside = -1;

//     // 1. Identify points to remove
//     for (int i = 0; i < spline.GetPointCount(); i++)
//     {
//         if (Vector2.Distance(spline.GetPosition(i), localExplosionPos) < 1.4 * explosionRadius)
//         {
//             if (firstPointInside == -1) firstPointInside = i;
//             lastPointInside = i;
//         }
//     }

//     // Segment detection if no points were inside
//     if (firstPointInside == -1)
//     {
//         for (int i = 0; i < spline.GetPointCount() - 1; i++)
//         {
//             float p1X = spline.GetPosition(i).x;
//             float p2X = spline.GetPosition(i + 1).x;
//             if (localExplosionPos.x > Mathf.Min(p1X, p2X) && localExplosionPos.x < Mathf.Max(p1X, p2X))
//             {
//                 firstPointInside = i + 1;
//                 lastPointInside = i; 
//                 break;
//             }
//         }
//     }

//     // 2. Identify neighbors and calculate angles
//     int leftNeighborIdx = Mathf.Max(0, (firstPointInside != -1) ? firstPointInside - 1 : 0);
//     int rightNeighborIdx = (lastPointInside != -1) ? Mathf.Min(spline.GetPointCount() - 1, lastPointInside + 1) : firstPointInside;

//     Vector2 leftPos = spline.GetPosition(leftNeighborIdx);
//     Vector2 rightPos = spline.GetPosition(rightNeighborIdx);

//     float angleLeft = Mathf.Atan2(leftPos.y - localExplosionPos.y, leftPos.x - localExplosionPos.x) * Mathf.Rad2Deg;
//     float angleRight = Mathf.Atan2(rightPos.y - localExplosionPos.y, rightPos.x - localExplosionPos.x) * Mathf.Rad2Deg;

//     float angularSpan = angleRight - angleLeft;
//     if (angularSpan < 0) angularSpan += 360f;
//     if (angularSpan > 180) angularSpan = 360f - angularSpan;

//     // Tame existing neighbors' tangents to prevent them from "reaching in"
//     spline.SetRightTangent(leftNeighborIdx, spline.GetRightTangent(leftNeighborIdx).normalized * 0.5f);
//     spline.SetLeftTangent(rightNeighborIdx, spline.GetLeftTangent(rightNeighborIdx).normalized * 0.6f);

//     // 3. Remove old points
//     int insertIdx = (firstPointInside != -1) ? firstPointInside : 1;
//     if (firstPointInside != -1 && lastPointInside >= firstPointInside)
//     {
//         int countToRemove = (lastPointInside - firstPointInside) + 1;
//         for (int i = 0; i < countToRemove; i++) spline.RemovePointAt(firstPointInside);
//     }

//     // 4. INSERT LEFT ANCHOR POINT at ~2x Radius
//     if (Vector2.Distance(leftPos, localExplosionPos) > explosionRadius * 1.5f)
//     {
//         Vector3 anchorPos = (Vector3)localExplosionPos + (new Vector3(Mathf.Cos(angleLeft * Mathf.Deg2Rad), Mathf.Sin(angleLeft * Mathf.Deg2Rad), 0) * explosionRadius * 1.8f);
//         spline.InsertPointAt(insertIdx, anchorPos);
//         spline.SetTangentMode(insertIdx, ShapeTangentMode.Continuous);
//         spline.SetLeftTangent(insertIdx, Vector3.left * 0.1f);
//         spline.SetRightTangent(insertIdx, Vector3.right * 0.1f);
//         insertIdx++;
//     }

//     // 5. Generate Adaptive Arc with Rotated Tangents
//     int internalPoints = (angularSpan > 150) ? 4 : (angularSpan > 120) ? 3 : (angularSpan > 80) ? 2 : 1;
//     int totalNewPoints = internalPoints + 2; 
//     float tangentMag = (2 * Mathf.PI * explosionRadius) / totalNewPoints * 0.3f;

//     for (int i = 0; i < totalNewPoints; i++)
//     {
//         float t = (float)i / (totalNewPoints - 1);
//         float currentAngle = Mathf.LerpAngle(angleLeft, angleRight, t);
//         float rad = currentAngle * Mathf.Deg2Rad;
//         Vector3 newPointPos = (Vector3)localExplosionPos + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * explosionRadius;

//         int currentIdx = insertIdx + i;
//         spline.InsertPointAt(currentIdx, newPointPos);
//         spline.SetTangentMode(currentIdx, ShapeTangentMode.Continuous);

//         // Calculate perpendicular direction
//         Vector3 baseDir = new Vector3(-Mathf.Sin(rad), Mathf.Cos(rad), 0);

//         if (i == 0) // First intersection point (Left)
//         {
//             // Tilt tangent 45 degrees to bridge the gap smoothly
//             Vector3 tiltedDir = Quaternion.Euler(0, 0, 45) * baseDir;
//             spline.SetLeftTangent(currentIdx, -tiltedDir * 0.3f);
//             spline.SetRightTangent(currentIdx, tiltedDir * 0.3f);
//         }
//         else if (i == totalNewPoints - 1) // Last intersection point (Right)
//         {
//             // Tilt tangent 45 degrees the other way
//             Vector3 tiltedDir = Quaternion.Euler(0, 0, -45) * baseDir;
//             spline.SetLeftTangent(currentIdx, -tiltedDir * 0.3f);
//             spline.SetRightTangent(currentIdx, tiltedDir * 0.3f);
//         }
//         else // Middle points
//         {
//             Vector3 tangentDir = baseDir * tangentMag;
//             spline.SetLeftTangent(currentIdx, -tangentDir);
//             spline.SetRightTangent(currentIdx, tangentDir);
//         }
//     }

//     // 6. INSERT RIGHT ANCHOR POINT
//     if (Vector2.Distance(rightPos, localExplosionPos) > explosionRadius * 1.5f)
//     {
//         int rAnchorIdx = insertIdx + totalNewPoints;
//         Vector3 anchorPos = (Vector3)localExplosionPos + (new Vector3(Mathf.Cos(angleRight * Mathf.Deg2Rad), Mathf.Sin(angleRight * Mathf.Deg2Rad), 0) * explosionRadius * 1.8f);
//         spline.InsertPointAt(rAnchorIdx, anchorPos);
//         spline.SetTangentMode(rAnchorIdx, ShapeTangentMode.Continuous);
//         spline.SetLeftTangent(rAnchorIdx, Vector3.left * 0.1f);
//         spline.SetRightTangent(rAnchorIdx, Vector3.right * 0.1f);
//     }

//     CleanupSpikes(spline, localExplosionPos);
//     shape.RefreshSpriteShape();
// }

//     void CleanupSpikes(Spline spline, Vector2 center)
//     {
//         float safetyRadius = explosionRadius * 2.2f;
//         for (int j = 0; j < 2; j++)
//         {
//             for (int i = 1; i < spline.GetPointCount() - 1; i++)
//             {
//                 Vector2 pos = spline.GetPosition(i);
//                 if (Vector2.Distance(pos, center) < safetyRadius)
//                 {
//                     Vector2 d1 = ((Vector2)spline.GetPosition(i - 1) - pos).normalized;
//                     Vector2 d2 = ((Vector2)spline.GetPosition(i + 1) - pos).normalized;
//                     if (Vector2.Angle(d1, d2) < 35f) 
//                     {
//                         spline.RemovePointAt(i);
//                         i--;
//                     }
//                 }
//             }
//         }
//     }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius * 2f);
    }

//     void OnCollisionEnter2D(Collision2D collision)
// {
//     // 1. If we hit a tree, destroy it IMMEDIATELY by reference
//     if (collision.gameObject.CompareTag("Tree"))
//     {
//         Debug.Log("Direct hit on tree: " + collision.gameObject.name);
//         Destroy(collision.gameObject);
//     }
    
//     // 2. Then proceed with the normal explosion for ground/tanks
//     Explode();
// }
}