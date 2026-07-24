using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private int poiLayerMask = Physics2D.AllLayers;
    [SerializeField] private Camera mainCamera;

    private readonly List<PoiBehaviour> nearbyPois = new();
    private PoiBehaviour currentClickTarget;

    public PoiBehaviour CurrentPoi { get; private set; }

    private void Awake()
    {
        if (inventory == null)
        {
            inventory = GetComponent<PlayerInventory>();
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void Update()
    {
        UpdateHighlightState();
        HandleClickInput();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((poiLayerMask & (1 << other.gameObject.layer)) == 0)
        {
            return;
        }

        PoiBehaviour poi = other.GetComponentInParent<PoiBehaviour>();
        if (poi == null || nearbyPois.Contains(poi))
        {
            return;
        }

        nearbyPois.Add(poi);

        if (poi.Definition != null && poi.Definition.ActivationMode == PoiActivationMode.Proximity)
        {
            poi.Interact(inventory, gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PoiBehaviour poi = other.GetComponentInParent<PoiBehaviour>();
        if (poi == null)
        {
            return;
        }

        nearbyPois.Remove(poi);
        poi.IsHighlighted = false;
    }

    private void UpdateHighlightState()
    {
        // Remove invalid entries
        nearbyPois.RemoveAll(poi => poi == null);

        // Update highlight for all nearby POIs based on distance and interaction state
        foreach (PoiBehaviour poi in nearbyPois)
        {
            if (poi != null && poi.CanInteract && poi.IsInHighlightRange(transform.position))
            {
                poi.IsHighlighted = true;
            }
            else if (poi != null)
            {
                poi.IsHighlighted = false;
            }
        }
    }

    private void HandleClickInput()
    {
        if (mainCamera == null)
        {
            return;
        }

        // Get mouse position (works with new InputSystem)
        Vector3 mousePos = Mouse.current?.position.ReadValue() ?? Vector3.zero;
        Vector2 worldPos = mainCamera.ScreenToWorldPoint(mousePos);

        // Raycast for interactables
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero, 1f, poiLayerMask);

        if (hit.collider != null)
        {
            Interactable interactable = hit.collider.GetComponentInParent<Interactable>();
            currentClickTarget = interactable as PoiBehaviour;

            // On left click, interact
            if (Mouse.current?.leftButton.wasPressedThisFrame == true)
            {
                if (currentClickTarget != null && currentClickTarget.CanInteract)
                {
                    Debug.Log($"[PlayerInteractor] Clicked POI: {currentClickTarget.Definition?.DisplayName ?? "Unknown"}");
                    currentClickTarget.TryInteract(inventory, gameObject);
                }
                else if (currentClickTarget != null)
                {
                    Debug.LogWarning($"[PlayerInteractor] Clicked POI but it cannot interact: {currentClickTarget.Definition?.DisplayName ?? "Unknown"}");
                }
            }
        }
        else
        {
            currentClickTarget = null;
        }
    }

    public bool TryInteract(PoiBehaviour poi)
    {
        return poi != null && poi.Interact(inventory, gameObject);
    }
}
