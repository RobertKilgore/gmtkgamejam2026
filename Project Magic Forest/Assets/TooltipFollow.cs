using UnityEngine;

public class TooltipFollow : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private RectTransform rectTransform;
    [SerializeField] private Vector2 offset = new Vector2(15f, -15f); // Keeps tooltip away from cursor

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        // 1. Get the screen mouse position
        Vector2 mousePos = Input.mousePosition;

        // 2. Apply the offset so the text isn't directly under the cursor
        rectTransform.position = mousePos + offset;
    }
   
}
