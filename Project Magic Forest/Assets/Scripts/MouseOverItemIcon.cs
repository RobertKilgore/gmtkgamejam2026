using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class MouseOverItemIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 originalScale;

    [SerializeField] private GameObject iconTooltip;

    void Update()
    {
       
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
