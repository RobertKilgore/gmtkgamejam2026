using System;
using UnityEngine;

public class TreeInteractable : Interactable
{
    [Header("Timer")]
    [SerializeField] private string timerKey = "";
    [SerializeField] private float timeToAdd = 10f;

    [Header("Interaction")]
    [SerializeField] private bool destroyOnInteract = true;
    [SerializeField] private bool requiresPlayer = true;

    [Header("Hooks")]
    [SerializeField] private bool invokeHookOnInteract = false;

    private PlayerTimers playerTimers;
    private bool hasBeenInteractedWith;

    public event Action<TreeInteractable> Interacted;

    public bool TryInteract()
    {
        return TryPerformInteraction();
    }

    protected override void HandleInteraction(PlayerInventory inventory, GameObject player)
    {
        if (!TryPerformInteraction())
        {
            return;
        }

        base.HandleInteraction(inventory, player);
    }

    private bool TryPerformInteraction()
    {
        if (hasBeenInteractedWith)
        {
            return false;
        }

        if (requiresPlayer)
        {
            playerTimers = FindFirstObjectByType<PlayerTimers>();
            if (playerTimers == null)
            {
                Debug.LogWarning("[TreeInteractable] No PlayerTimers found.");
                return false;
            }
        }

        if (playerTimers != null)
        {
            Timer timer = playerTimers.FindTimer(timerKey);
            if (timer != null)
            {
                timer.AddTime(timeToAdd);
                Debug.Log($"[TreeInteractable] Added {timeToAdd} seconds to timer '{timerKey}'.");
            }
            else
            {
                Debug.LogWarning($"[TreeInteractable] Timer '{timerKey}' not found.");
                return false;
            }
        }

        hasBeenInteractedWith = true;
        Interacted?.Invoke(this);

        if (invokeHookOnInteract)
        {
            OnInteractHook();
        }

        if (destroyOnInteract)
        {
            Destroy(gameObject);
        }

        return true;
    }

    protected virtual void OnInteractHook()
    {
    }

    public override bool CanInteract()
    {
        return !hasBeenInteractedWith;
    }
}
