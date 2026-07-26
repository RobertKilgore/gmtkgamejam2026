using UnityEngine;
using UnityEngine.EventSystems;

public class MouseOverItemIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject iconTooltip;
    private RectTransform rectTransform;
    private bool isHovering;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (!isHovering || iconTooltip == null || rectTransform == null)
        {
            return;
        }

        if (!RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition, null))
        {
            HideTooltip();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowTooltip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
    }

    private void OnDisable()
    {
        HideTooltip();
    }

    private void ShowTooltip()
    {
        isHovering = true;

        if (iconTooltip != null)
        {
            iconTooltip.SetActive(true);
        }
    }

    private void HideTooltip()
    {
        isHovering = false;

        if (iconTooltip != null)
        {
            iconTooltip.SetActive(false);
        }
    }
}
