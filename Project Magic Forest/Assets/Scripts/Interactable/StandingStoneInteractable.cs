using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public sealed class StandingStoneInteractable : Interactable
{
    [Header("Stone Visuals")]
    [SerializeField] private Sprite inactiveSprite;
    [SerializeField] private Sprite[] stoneSprites = new Sprite[PlayerStandingStoneTracker.MaxStandingStones];
    [SerializeField] private Sprite fallbackActivatedSprite;

    [Header("Audio")]
    [SerializeField] private AudioClips audioClips;
    [SerializeField] private AudioClip magicStoneSound;
    [SerializeField, Range(0f, 1f)] private float soundVolume = 1f;
    [SerializeField] private string audioChannel = "magic_stone";

    private SpriteRenderer spriteRenderer;
    private bool hasActivated;
    private int activatedStoneIndex = -1;
    private bool isSubscribedToTracker;

    protected override void Awake()
    {
        base.Awake();
        spriteRenderer = GetComponent<SpriteRenderer>();
        RefreshState();
    }

    private void OnEnable()
    {
        SubscribeToTracker();
    }

    private void OnDisable()
    {
        UnsubscribeFromTracker();
    }

    private void SubscribeToTracker()
    {
        if (isSubscribedToTracker)
        {
            return;
        }

        PlayerStandingStoneTracker tracker = PlayerStandingStoneTracker.Instance;
        if (tracker == null)
        {
            tracker = FindFirstObjectByType<PlayerStandingStoneTracker>();
        }

        if (tracker != null)
        {
            tracker.StoneCountChanged += OnStoneCountChanged;
            isSubscribedToTracker = true;
            RefreshState();
        }
    }

    private void UnsubscribeFromTracker()
    {
        if (!isSubscribedToTracker)
        {
            return;
        }

        PlayerStandingStoneTracker tracker = PlayerStandingStoneTracker.Instance;
        if (tracker != null)
        {
            tracker.StoneCountChanged -= OnStoneCountChanged;
        }

        isSubscribedToTracker = false;
    }

    public override bool CanInteract()
    {
        return base.CanInteract() && !hasActivated && PlayerStandingStoneTracker.Instance != null && PlayerStandingStoneTracker.Instance.StoneCount < PlayerStandingStoneTracker.MaxStandingStones;
    }

    protected override void HandleInteraction(PlayerInventory inventory, GameObject player)
    {
        if (!CanInteract())
        {
            return;
        }

        PlayerStandingStoneTracker tracker = PlayerStandingStoneTracker.Instance;
        if (tracker == null)
        {
            Debug.LogWarning("[StandingStoneInteractable] PlayerStandingStoneTracker instance not found.");
            return;
        }

        if (tracker.StoneCount >= PlayerStandingStoneTracker.MaxStandingStones)
        {
            RefreshState();
            return;
        }

        int newCount = tracker.AddStone();
        activatedStoneIndex = Mathf.Clamp(newCount - 1, 0, stoneSprites.Length - 1);
        PlayMagicStoneSound();
        hasActivated = true;
        RefreshState();
        SetHighlighted(false);
        enabled = false;
        base.HandleInteraction(inventory, player);
    }

    private void OnStoneCountChanged(int count)
    {
        RefreshState();
    }

    private void PlayMagicStoneSound()
    {
        AudioClip clipToPlay = magicStoneSound;
        if (clipToPlay == null && audioClips != null)
        {
            clipToPlay = audioClips.magicStone;
        }

        if (clipToPlay == null)
        {
            Debug.LogWarning("[StandingStoneInteractable] No magic stone sound assigned.");
            return;
        }

        AudioManager.PlaySFX(clipToPlay, soundVolume, 1f, audioChannel);
    }

    private void RefreshState()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        if (hasActivated)
        {
            spriteRenderer.sprite = GetActivatedSprite();
            return;
        }

        if (PlayerStandingStoneTracker.Instance != null && PlayerStandingStoneTracker.Instance.StoneCount >= PlayerStandingStoneTracker.MaxStandingStones)
        {
            spriteRenderer.sprite = inactiveSprite;
            return;
        }

        spriteRenderer.sprite = inactiveSprite;
    }

    private Sprite GetActivatedSprite()
    {
        if (activatedStoneIndex >= 0 && activatedStoneIndex < stoneSprites.Length && stoneSprites[activatedStoneIndex] != null)
        {
            return stoneSprites[activatedStoneIndex];
        }

        return fallbackActivatedSprite;
    }
}
