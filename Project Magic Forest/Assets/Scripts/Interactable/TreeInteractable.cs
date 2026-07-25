using System;
using System.Collections.Generic;
using UnityEngine;

public enum TreeState
{
    Untouched = 0,
    HitOnce = 1,
    HitTwice = 2,
    Fallen = 3
}

public class TreeInteractable : Interactable
{
    [Header("Timer")]
    [SerializeField] private string timerKey = "";
    [SerializeField] private float timeToAdd = 10f;

    [Header("Tree Sprites")]
    [SerializeField] private Sprite untouchedSprite;
    [SerializeField] private Sprite hitOnceSprite;
    [SerializeField] private Sprite hitTwiceSprite;
    [SerializeField] private Sprite fallenSprite;

    [Header("Interaction")]
    [SerializeField] private bool destroyOnInteract = true;
    [SerializeField] private bool requiresPlayer = true;

    [Header("Hooks")]
    [SerializeField] private bool invokeHookOnInteract = false;

    private PlayerTimers playerTimers;
    private SpriteRenderer spriteRenderer;
    private TreeState currentTreeState = TreeState.Untouched;

    private static readonly HashSet<TreeInteractable> dirtyTrees = new HashSet<TreeInteractable>();

    public static IReadOnlyCollection<TreeInteractable> DirtyTrees => dirtyTrees;

    public event Action<TreeInteractable> Interacted;

    protected override void Awake()
    {
        base.Awake();
        spriteRenderer = GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>(true);
        ApplyState((int)currentTreeState);
    }

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
        if (!CanInteract())
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

        AdvanceTreeState();
        return true;
    }

    private void AdvanceTreeState()
    {
        if (currentTreeState == TreeState.Fallen)
        {
            return;
        }

        currentTreeState++;
        SetState((int)currentTreeState);

        if (currentTreeState != TreeState.Untouched)
        {
            MarkDirty();
        }

        if (currentTreeState == TreeState.Fallen)
        {
            GrantFinalEffect();
        }
    }

    private void GrantFinalEffect()
    {
        if (requiresPlayer && playerTimers == null)
        {
            playerTimers = FindFirstObjectByType<PlayerTimers>();
        }

        if (playerTimers != null && !string.IsNullOrEmpty(timerKey))
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
            }
        }

        Interacted?.Invoke(this);

        if (invokeHookOnInteract)
        {
            OnInteractHook();
        }

        if (destroyOnInteract)
        {
            Destroy(gameObject);
        }
    }

    protected virtual void OnInteractHook()
    {
    }

    public override bool CanInteract()
    {
        return currentTreeState != TreeState.Fallen;
    }

    public override void SetState(int stateIndex)
    {
        base.SetState(stateIndex);
        currentTreeState = (TreeState)Mathf.Clamp(stateIndex, 0, 3);
    }

    protected override void ApplyState(int stateIndex)
    {
        currentTreeState = (TreeState)Mathf.Clamp(stateIndex, 0, 3);

        if (spriteRenderer == null)
        {
            return;
        }

        switch (currentTreeState)
        {
            case TreeState.Untouched:
                spriteRenderer.sprite = untouchedSprite;
                break;
            case TreeState.HitOnce:
                spriteRenderer.sprite = hitOnceSprite;
                break;
            case TreeState.HitTwice:
                spriteRenderer.sprite = hitTwiceSprite;
                break;
            case TreeState.Fallen:
                spriteRenderer.sprite = fallenSprite;
                break;
        }
    }

    public override void ResetState()
    {
        base.ResetState();
        currentTreeState = TreeState.Untouched;
        SetState((int)TreeState.Untouched);
        UnmarkDirty();
    }

    [ContextMenu("Reset Tree")]
    private void ResetTreeContextMenu()
    {
        ResetState();
    }

    private void MarkDirty()
    {
        dirtyTrees.Add(this);
    }

    private void UnmarkDirty()
    {
        dirtyTrees.Remove(this);
    }
}
