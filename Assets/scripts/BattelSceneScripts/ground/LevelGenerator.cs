using UnityEngine;
using UnityEngine.U2D; // Needed for SpriteShape

public class LevelGenerator : MonoBehaviour
{
    public SpriteShapeController shapeParent;
    public int levelWidth = 50; // How long the ground is
    public float scale = 5f;    // How "bumpy" the hills are
    public float surfaceHeight = 2f; // Base height

    void Start()
    {
        GenerateLevel();
    }

    void GenerateLevel()
    {
        Spline spline = shapeParent.spline;
        spline.Clear(); // Remove any existing points

        for (int i = 0; i < levelWidth; i++)
        {
            // PerlinNoise creates smooth, natural-looking hills
            float xPos = i * 2f; 
            float yPos = Mathf.PerlinNoise(i * 0.3f, Random.Range(0f, 10f)) * scale;

            spline.InsertPointAt(i, new Vector3(xPos, yPos + surfaceHeight, 0));
            
            // This makes the points "smooth" instead of sharp corners
            spline.SetTangentMode(i, ShapeTangentMode.Continuous);
        }

        // Add points at the bottom to close the shape so it looks solid
        int lastIdx = levelWidth;
        spline.InsertPointAt(lastIdx, new Vector3((levelWidth - 1) * 2f, -10, 0));
        spline.InsertPointAt(lastIdx + 1, new Vector3(0, -10, 0));
    }
}