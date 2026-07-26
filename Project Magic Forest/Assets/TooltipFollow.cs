using UnityEngine;

public class TooltipFollow : MonoBehaviour
{
    private RectTransform rectTransform;
    [SerializeField] private Vector2 offset = new Vector2(15f, -15f);
    [SerializeField] private Vector2 padding = new Vector2(10f, 10f);

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        Vector2 mousePos = Input.mousePosition;
        Vector2 targetPosition = mousePos + offset;

        Rect safeArea = Screen.safeArea;
        Vector2 minPosition = new Vector2(
            safeArea.xMin + padding.x + rectTransform.rect.width / 2f,
            safeArea.yMin + padding.y + rectTransform.rect.height / 2f
        );
        Vector2 maxPosition = new Vector2(
            safeArea.xMax - padding.x - rectTransform.rect.width / 2f,
            safeArea.yMax - padding.y - rectTransform.rect.height / 2f
        );

        targetPosition.x = Mathf.Clamp(targetPosition.x, minPosition.x, maxPosition.x);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minPosition.y, maxPosition.y);

        rectTransform.position = targetPosition;
    }
}
