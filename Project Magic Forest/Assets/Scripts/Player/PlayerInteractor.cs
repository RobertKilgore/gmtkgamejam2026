using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private LayerMask poiLayerMask = Physics2D.AllLayers;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private playerMovement playerMovement;

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

        if (playerMovement == null)
        {
            playerMovement = GetComponent<playerMovement>() ?? GetComponentInChildren<playerMovement>(true);
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
        Interactable interactable = ResolveInteractableFromCollider(other);
        if (interactable == null || !interactable.enabled || nearbyInteractables.Contains(interactable))
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
        Interactable interactable = ResolveInteractableFromCollider(other);
        if (interactable == null)
        {
            return;
        }

        nearbyInteractables.Remove(interactable);
        interactable.SetHighlighted(false);
    }

    private Interactable ResolveInteractableFromCollider(Collider2D collider)
    {
        if (collider == null)
        {
            return null;
        }

        // Prefer the interactable on the collider's own GameObject first.
        Interactable interactable = collider.GetComponent<Interactable>();
        if (interactable != null && interactable.enabled)
        {
            return interactable;
        }

        // Parent colliders should not resolve to child interactables.
        // Child colliders may still resolve to a parent interactable.
        interactable = collider.GetComponentInParent<Interactable>();
        if (interactable != null && interactable.enabled)
        {
            return interactable;
        }

        return null;
    }

    private void UpdateHighlightState()
    {
        nearbyInteractables.RemoveAll(interactable => interactable == null || !interactable.enabled);

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

        // Prefer the interactable's dedicated click collider and fall back to the configured mask.
        Collider2D[] hitColliders = Physics2D.OverlapPointAll(worldPos, poiLayerMask);
        currentClickTarget = null;

        if (hitColliders == null || hitColliders.Length == 0)
        {
            hitColliders = Physics2D.OverlapPointAll(worldPos);
        }

        if (hitColliders != null && hitColliders.Length > 0)
        {
            Interactable bestTarget = null;
            float bestDistance = float.PositiveInfinity;

            for (int i = 0; i < hitColliders.Length; i++)
            {
                Collider2D hitCollider = hitColliders[i];
                Interactable resolvedInteractable = ResolveInteractableFromCollider(hitCollider);
                if (resolvedInteractable == null || !resolvedInteractable.enabled)
                {
                    continue;
                }

                Collider2D clickCollider = resolvedInteractable.GetClickCollider();
                bool directHit = clickCollider != null && clickCollider.OverlapPoint(worldPos);
                if (directHit)
                {
                    bestTarget = resolvedInteractable;
                    break;
                }

                float distance = Vector2.Distance(worldPos, resolvedInteractable.transform.position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestTarget = resolvedInteractable;
                }
            }

            currentClickTarget = bestTarget;
        }

        bool clicked = Mouse.current?.leftButton.wasPressedThisFrame == true || Input.GetMouseButtonDown(0);
        if (!clicked || currentClickTarget == null)
        {
            return;
        }

        bool directClickHit = currentClickTarget.GetClickCollider() != null && currentClickTarget.GetClickCollider().OverlapPoint(worldPos);
        bool canUseDirectClick = currentClickTarget.IsHighlighted || directClickHit;

        if ((currentClickTarget.InteractMode != InteractMode.Click && currentClickTarget.InteractMode != InteractMode.ClickAndButton) || !canUseDirectClick || !currentClickTarget.CanInteract())
        {
            return;
        }

        if (TryHandleTreeInteraction(currentClickTarget))
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
                if (TryHandleTreeInteraction(interactable))
                {
                    return;
                }

                interactable.TryInteract(inventory, gameObject);
                return;
            }
        }
    }

    private bool TryHandleTreeInteraction(Interactable interactable)
    {
        if (interactable is not TreeInteractable treeInteractable)
        {
            return false;
        }

        if (playerMovement != null && playerMovement.IsPlayingAxeAnimation)
        {
            return true;
        }

        if (playerMovement != null && playerMovement.PlayAxeSwingAnimation())
        {
            StartCoroutine(DelayTreeInteraction(treeInteractable));
            return true;
        }

        return false;
    }

    private IEnumerator DelayTreeInteraction(TreeInteractable treeInteractable)
    {
        if (playerMovement != null)
        {
            yield return new WaitUntil(() => !playerMovement.IsPlayingAxeAnimation);
        }

        treeInteractable.TryInteract(inventory, gameObject);
    }

    public bool TryInteract(PoiBehaviour poi)
    {
        return poi != null && poi.Interact(inventory, gameObject);
    }
}
