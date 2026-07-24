using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private int poiLayerMask = Physics2D.AllLayers;
    [SerializeField] private Camera mainCamera;

    private readonly List<Interactable> nearbyInteractables = new();
    private Interactable currentClickTarget;

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
        HandleButtonInput();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((poiLayerMask & (1 << other.gameObject.layer)) == 0)
        {
            return;
        }

        Interactable interactable = other.GetComponentInParent<Interactable>();
        if (interactable == null || nearbyInteractables.Contains(interactable))
        {
            return;
        }

        nearbyInteractables.Add(interactable);

        if (interactable.InteractMode == InteractMode.Proximity)
        {
            interactable.OnClicked();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Interactable interactable = other.GetComponentInParent<Interactable>();
        if (interactable == null)
        {
            return;
        }

        nearbyInteractables.Remove(interactable);
        interactable.SetHighlighted(false);
    }

    private void UpdateHighlightState()
    {
        nearbyInteractables.RemoveAll(interactable => interactable == null);

        List<Interactable> inRangeInteractables = new();
        foreach (Interactable interactable in nearbyInteractables)
        {
            if (interactable == null)
            {
                continue;
            }

            bool inRange = interactable.IsInHighlightRange(transform.position);
            if (interactable.CanInteract() && inRange)
            {
                inRangeInteractables.Add(interactable);
            }
            else
            {
                interactable.SetHighlighted(false);
            }
        }

        if (inRangeInteractables.Count == 0)
        {
            return;
        }

        bool hasButtonPress = false;
        Interactable closestButtonInteractable = null;
        float closestButtonDistance = float.PositiveInfinity;

        foreach (Interactable interactable in inRangeInteractables)
        {
            if (interactable.InteractMode != InteractMode.ButtonPress)
            {
                interactable.SetHighlighted(true);
                continue;
            }

            hasButtonPress = true;
            float distance = Vector2.Distance(interactable.transform.position, transform.position);
            if (distance < closestButtonDistance)
            {
                closestButtonDistance = distance;
                closestButtonInteractable = interactable;
            }
        }

        if (!hasButtonPress || closestButtonInteractable == null)
        {
            return;
        }

        foreach (Interactable interactable in inRangeInteractables)
        {
            interactable.SetHighlighted(interactable == closestButtonInteractable);
        }
    }

    private void HandleClickInput()
    {
        if (mainCamera == null)
        {
            return;
        }

        Vector3 mousePos = Mouse.current?.position.ReadValue() ?? Vector3.zero;
        Vector2 worldPos = mainCamera.ScreenToWorldPoint(mousePos);

        Collider2D hitCollider = Physics2D.OverlapPoint(worldPos, poiLayerMask);

        if (hitCollider != null)
        {
            Interactable interactable = hitCollider.GetComponentInParent<Interactable>();
            currentClickTarget = interactable;
        }
        else
        {
            currentClickTarget = null;
        }

        bool clicked = Mouse.current?.leftButton.wasPressedThisFrame == true || Input.GetMouseButtonDown(0);
        if (!clicked || currentClickTarget == null)
        {
            return;
        }

        if ((currentClickTarget.InteractMode != InteractMode.Click && currentClickTarget.InteractMode != InteractMode.ClickAndButton) || !currentClickTarget.IsHighlighted || !currentClickTarget.CanInteract())
        {
            return;
        }

        currentClickTarget.TryInteract(inventory, gameObject);
    }

    private void HandleButtonInput()
    {
        for (int i = 0; i < nearbyInteractables.Count; i++)
        {
            Interactable interactable = nearbyInteractables[i];
            if (interactable == null || (interactable.InteractMode != InteractMode.ButtonPress && interactable.InteractMode != InteractMode.ClickAndButton) || !interactable.IsHighlighted || !interactable.CanInteract())
            {
                continue;
            }

            if (Input.GetKeyDown(interactable.InteractionKey))
            {
                interactable.TryInteract(inventory, gameObject);
                return;
            }
        }
    }

    public bool TryInteract(PoiBehaviour poi)
    {
        return poi != null && poi.Interact(inventory, gameObject);
    }
}
