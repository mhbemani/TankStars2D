using UnityEngine;
using UnityEngine.UI; // For Legacy Text

public class floatingText : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float fadeSpeed = 1f;
    
    private Text myText;
    private Color textColor;

    void Start()
    {
        myText = GetComponentInChildren<Text>();
        if (myText != null)
        {
            textColor = myText.color;
        }

        // Deletes this object automatically after 1.5 seconds
        Destroy(gameObject, 1.5f);
    }

    void Update()
    {
        // 1. MOVE UPWARD
        // This moves the whole object (the Canvas/Text) up every frame
        transform.position += new Vector3(0, moveSpeed * Time.deltaTime, 0);

        // 2. FADE AWAY
        if (myText != null)
        {
            // Subtract from the "Alpha" (transparency)
            textColor.a -= fadeSpeed * Time.deltaTime;
            myText.color = textColor;
        }
    }
}