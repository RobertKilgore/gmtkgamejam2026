using System;
using UnityEngine;

public sealed class PoiBehaviour : Interactable
{
    [SerializeField] private PoiDefinition definition;
    private bool hasBeenUsed;

    public event Action<PoiBehaviour> BecameAvailable;
    public event Action<PoiBehaviour> Completed;

    public PoiDefinition Definition => definition;

    public override bool CanInteract()
    {
        return definition != null && (!definition.OneShot || !hasBeenUsed);
    }

    protected override void HandleInteraction(PlayerInventory inventory, GameObject player)
    {
        if (!Interact(inventory, player))
        {
            return;
        }

        base.HandleInteraction(inventory, player);
    }

    public override bool TryInteract(PlayerInventory inventory, GameObject player)
    {
        return Interact(inventory, player);
    }

    public bool Interact(PlayerInventory inventory, GameObject player)
    {
        bool canInteract = CanInteract();
        if (!canInteract || inventory == null || player == null)
        {
            Debug.LogWarning($"[POI] Cannot interact with {Definition?.DisplayName ?? "Unknown"}: CanInteract={canInteract}");
            return false;
        }

        Debug.Log($"[POI] Interacting with {definition.DisplayName}");
        PlayerTimers playerTimers = player != null ? player.GetComponent<PlayerTimers>() : null;
        if (playerTimers == null)
        {
            playerTimers = player != null ? player.GetComponentInChildren<PlayerTimers>() : null;
        }

        PoiContext context = new(inventory, player, transform.position, playerTimers, this);
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

    public override bool IsInHighlightRange(Vector3 playerPosition)
    {
        return base.IsInHighlightRange(playerPosition);
    }
}
