using UnityEngine;

[CreateAssetMenu(fileName = "NewMissileData", menuName = "Tanks/Missile Data")]
public class MissileData : ScriptableObject
{
    [Header("Identity")]
    public string missileName;
    public Sprite icon; // For missile shape in the game

    public Sprite missileIcon; // fro drawer
    // public GameObject missilePrefab;
    public int ammoCount;

    [Header("Visuals")]
    public GameObject missilePrefab;    // Should have your engine smoke/trail attached
    public GameObject explosionPrefab; // The specific explosion animation prefab

    public RuntimeAnimatorController muzzleFlashController;

    [Header("Physics")]
    public float gravityScale = 1.0f;  // Set to 0 for straight-line bullets
    public float speedMultiplier = 1.0f;

    [Header("Combat Stats")]
    public float explosionRadius = 2.0f;
    public int burstCount = 1;
    public float damage = 50f;

    public float runtimeMaxRange; // Calculated at Start
    public bool isLaser;          // Set this in Inspector for the laser missile
}