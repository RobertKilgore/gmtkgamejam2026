using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class MouseOverFixedUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 originalScale;

  
    public GameObject iconTooltip;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
     
    void Start()
    {
      
        iconTooltip.SetActive(false);
        
    }

    // Update is called once per frame
   
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
