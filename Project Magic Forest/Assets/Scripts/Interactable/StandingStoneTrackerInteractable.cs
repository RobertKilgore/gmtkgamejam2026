using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public sealed class StandingStoneTrackerInteractable : Interactable
{
    [Header("Tracker Sprites")]
    [SerializeField] private Sprite baseSprite;
    [SerializeField] private Sprite[] progressionSprites = new Sprite[PlayerStandingStoneTracker.MaxStandingStones];
    [SerializeField] private Sprite completedSprite;

    [Header("Audio")]
    [SerializeField] private AudioClips audioClips;
    [SerializeField] private AudioClip magicStoneSound;
    [SerializeField, Range(0f, 1f)] private float soundVolume = 1f;
    [SerializeField] private string audioChannel = "magic_stone_tracker";

    [Header("Credits")]
    [SerializeField] private float creditsDelay = 1f;

    private SpriteRenderer spriteRenderer;
    private bool isCompleted;
    private bool isSubscribedToTracker;

    protected override void Awake()
    {
        base.Awake();
        spriteRenderer = GetComponent<SpriteRenderer>();
        RefreshState();
    }

    private void OnEnable()
    {
        EnsureTrackerSubscription();
    }

    private void OnDisable()
    {
        UnsubscribeFromTracker();
    }

    private void Update()
    {
        if (!isSubscribedToTracker)
        {
            EnsureTrackerSubscription();
        }
    }

    public override bool CanInteract()
    {
        PlayerStandingStoneTracker tracker = GetTracker();
        return base.CanInteract() && tracker != null && tracker.StoneCount >= PlayerStandingStoneTracker.MaxStandingStones && !isCompleted;
    }

    protected override void HandleInteraction(PlayerInventory inventory, GameObject player)
    {
        if (!CanInteract())
        {
            return;
        }

        PlayMagicStoneSound();
        isCompleted = true;
        RefreshState();
        StartCoroutine(TriggerCreditsAfterDelay());
        base.HandleInteraction(inventory, player);
    }

    private void OnStoneCountChanged(int count)
    {
        RefreshState();
    }

    private void EnsureTrackerSubscription()
    {
        if (isSubscribedToTracker)
        {
            return;
        }

        PlayerStandingStoneTracker tracker = GetTracker();
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

    private IEnumerator TriggerCreditsAfterDelay()
    {
        yield return new WaitForSeconds(creditsDelay);
        if (SceneFlowManager.Instance != null)
        {
            SceneFlowManager.Instance.LoadEndScene();
        }
        else
        {
            Debug.LogWarning("[StandingStoneTrackerInteractable] SceneFlowManager instance not found to load credits.");
        }
    }

    private PlayerStandingStoneTracker GetTracker()
    {
        return PlayerStandingStoneTracker.Instance ?? FindFirstObjectByType<PlayerStandingStoneTracker>();
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
            Debug.LogWarning("[StandingStoneTrackerInteractable] No magic stone sound assigned.");
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

        if (PlayerStandingStoneTracker.Instance == null)
        {
            spriteRenderer.sprite = baseSprite;
            return;
        }

        int count = PlayerStandingStoneTracker.Instance.StoneCount;
        if (count <= 0)
        {
            spriteRenderer.sprite = baseSprite;
            return;
        }

        if (count >= PlayerStandingStoneTracker.MaxStandingStones)
        {
            spriteRenderer.sprite = completedSprite != null ? completedSprite : progressionSprites[progressionSprites.Length - 1];
            return;
        }

        spriteRenderer.sprite = progressionSprites[count - 1] != null ? progressionSprites[count - 1] : baseSprite;
    }
}
