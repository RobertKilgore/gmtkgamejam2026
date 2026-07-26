using UnityEngine;

public enum ChestState
{
    Shut = 0,
    Open = 1
}

[RequireComponent(typeof(SpriteRenderer))]
public sealed class ChestInteractable : Interactable
{
    [Header("Chest State")]
    [SerializeField] private ChestState chestState = ChestState.Shut;
    [SerializeField] private Sprite shutSprite;
    [SerializeField] private Sprite openSprite;

    [Header("Audio")]
    [SerializeField] private AudioClips audioClips;
    [SerializeField] private AudioClip openSound;
    [SerializeField, Range(0f, 1f)] private float openVolume = 1f;
    [SerializeField] private string audioChannel = "interaction";

    [Header("Reward")]
    [SerializeField] private InventoryItemPoolDefinition itemPool;

    private SpriteRenderer spriteRenderer;

    protected override void Awake()
    {
        base.Awake();
        spriteRenderer = GetComponent<SpriteRenderer>();
        RefreshState();
    }

    public override bool CanInteract()
    {
        return base.CanInteract() && chestState == ChestState.Shut;
    }

    protected override void HandleInteraction(PlayerInventory inventory, GameObject player)
    {
        if (chestState != ChestState.Shut)
        {
            return;
        }

        OpenChest(inventory);
        base.HandleInteraction(inventory, player);
    }

    public void OpenChest(PlayerInventory inventory)
    {
        if (chestState == ChestState.Open)
        {
            return;
        }

        chestState = ChestState.Open;
        RefreshState();
        PlayOpenSound();
        GiveItem(inventory);
    }

    private void RefreshState()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        spriteRenderer.sprite = chestState == ChestState.Shut ? shutSprite : openSprite;
    }

    private void PlayOpenSound()
    {
        AudioClip clipToPlay = openSound;
        if (clipToPlay == null && audioClips != null)
        {
            clipToPlay = audioClips.chestOpen;
        }

        if (clipToPlay == null)
        {
            Debug.LogWarning("[ChestInteractable] No open sound assigned and AudioClips asset is missing the chest open clip.");
            return;
        }

        AudioManager.PlaySFX(clipToPlay, openVolume, 1f, audioChannel);
    }

    private void GiveItem(PlayerInventory inventory)
    {
        if (itemPool == null)
        {
            Debug.LogWarning("[ChestInteractable] No item pool assigned.");
            return;
        }

        if (inventory == null)
        {
            Debug.LogWarning("[ChestInteractable] No player inventory available.");
            return;
        }

        ItemDefinition rewardedItem = itemPool.GetRandomItemNotOwned(inventory);
        if (rewardedItem == null)
        {
            Debug.LogWarning("[ChestInteractable] No available item to give the player.");
            return;
        }

        if (inventory.Add(rewardedItem, 1f))
        {
            Debug.Log($"[ChestInteractable] Gave player item '{rewardedItem.DisplayName}'.");
        }
        else
        {
            Debug.LogWarning($"[ChestInteractable] Failed to add item '{rewardedItem?.DisplayName}' to inventory.");
        }
    }
}
