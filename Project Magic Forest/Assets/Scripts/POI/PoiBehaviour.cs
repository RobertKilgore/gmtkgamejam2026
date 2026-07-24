using System;
using UnityEngine;

public sealed class PoiBehaviour : Interactable
{
    [SerializeField] private PoiDefinition definition;
    [SerializeField] private float maxHighlightDistance = float.PositiveInfinity;
    private bool hasBeenUsed;

    public event Action<PoiBehaviour> BecameAvailable;
    public event Action<PoiBehaviour> Completed;

    public PoiDefinition Definition => definition;
    public bool CanInteract => definition != null && (!definition.OneShot || !hasBeenUsed);

    public override void OnClicked()
    {
        base.OnClicked();

        if (definition != null && definition.ActivationMode == PoiActivationMode.Manual)
        {
            // Manual POIs are triggered via TryInteract
        }
    }

    public bool TryInteract(PlayerInventory inventory, GameObject player)
    {
        return Interact(inventory, player);
    }

    public bool Interact(PlayerInventory inventory, GameObject player)
    {
        if (!CanInteract || inventory == null || player == null)
        {
            Debug.LogWarning($"[POI] Cannot interact with {Definition?.DisplayName ?? "Unknown"}: CanInteract={CanInteract}");
            return false;
        }

        Debug.Log($"[POI] Interacting with {definition.DisplayName}");
        PlayerTimers playerTimers = player != null ? player.GetComponent<PlayerTimers>() : null;
        if (playerTimers == null)
        {
            playerTimers = player != null ? player.GetComponentInChildren<PlayerTimers>() : null;
        }

        PoiContext context = new(inventory, player, transform.position, playerTimers);
        bool appliedAnyEffect = false;

        foreach (PoiEffect effect in definition.Effects)
        {
            if (effect != null && effect.Apply(context))
            {
                appliedAnyEffect = true;
            }
        }

        if (!appliedAnyEffect)
        {
            Debug.LogWarning($"[POI] {definition.DisplayName}: No effects were applied.");
            return false;
        }

        hasBeenUsed = true;
        Debug.Log($"[POI] {definition.DisplayName} completed and marked as used.");
        Completed?.Invoke(this);
        return true;
    }

    public void ResetForNewRun()
    {
        hasBeenUsed = false;
        IsHighlighted = false;
        BecameAvailable?.Invoke(this);
    }

    public bool IsInHighlightRange(Vector3 playerPosition)
    {
        float distance = (transform.position - playerPosition).magnitude;
        return distance <= maxHighlightDistance;
    }
}
