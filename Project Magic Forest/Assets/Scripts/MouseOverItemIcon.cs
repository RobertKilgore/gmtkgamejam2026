using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class MouseOverItemIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 originalScale;

    [SerializeField] private GameObject iconTooltip;
   
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
   
     public void OnPointerEnter(PointerEventData eventData)
    {
       iconTooltip.SetActive(true);
    }

    // Triggered when mouse leaves the UI element
    public void OnPointerExit(PointerEventData eventData)
    {
        iconTooltip.SetActive(false);
    }
}
