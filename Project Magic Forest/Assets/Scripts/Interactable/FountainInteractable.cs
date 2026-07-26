using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public sealed class FountainInteractable : Interactable
{
    [Header("Fountain State")]
    [SerializeField] private Sprite activeSprite;
    [SerializeField] private Sprite usedSprite;

    [Header("Audio")]
    [SerializeField] private AudioClips audioClips;
    [SerializeField] private AudioClip fountainSound;
    [SerializeField, Range(0f, 1f)] private float fountainVolume = 0.8f;
    [SerializeField] private string audioChannel = "fountain";

    [Header("Speed Buff")]
    [SerializeField, Range(1f, 3f)] private float speedMultiplier = 1.5f;
    [SerializeField, Tooltip("Use -1 for an infinite speed buff.")] private float buffDuration = 10f;

    private SpriteRenderer spriteRenderer;
    private bool isUsed;
    private bool isVisible;
    private bool isIdleSoundPlaying;

    protected override void Awake()
    {
        base.Awake();
        spriteRenderer = GetComponent<SpriteRenderer>();
        RefreshState();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (!isUsed && spriteRenderer != null && spriteRenderer.isVisible)
        {
            isIdleSoundPlaying = false;
            StartIdleSound();
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        StopIdleSound();
    }

    private void Update()
    {
        if (isUsed || spriteRenderer == null)
        {
            return;
        }

        if (spriteRenderer.isVisible && !isIdleSoundPlaying)
        {
            StartIdleSound();
        }
        else if (!spriteRenderer.isVisible && isIdleSoundPlaying)
        {
            StopIdleSound();
        }
    }

    public override bool CanInteract()
    {
        return base.CanInteract() && !isUsed;
    }

    protected override void HandleInteraction(PlayerInventory inventory, GameObject player)
    {
        if (isUsed)
        {
            return;
        }

        if (player == null)
        {
            Debug.LogWarning("[FountainInteractable] Missing player reference.");
            return;
        }

        playerMovement movement = ResolvePlayerMovement(player);
        if (movement == null)
        {
            Debug.LogWarning("[FountainInteractable] No playerMovement component found on player.");
            return;
        }

        ApplySpeedBuff(movement);
        MarkUsed();
        base.HandleInteraction(inventory, player);
    }

    private void ApplySpeedBuff(playerMovement movement)
    {
        if (movement == null)
        {
            return;
        }

        movement.MultiplySpeed(speedMultiplier);
        if (buffDuration >= 0f)
        {
            StartCoroutine(ResetSpeedAfterDelay(movement, buffDuration));
        }
    }

    private IEnumerator ResetSpeedAfterDelay(playerMovement movement, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (movement != null)
        {
            movement.ResetSpeed();
        }
    }

    private playerMovement ResolvePlayerMovement(GameObject player)
    {
        if (player == null)
        {
            return FindFirstObjectByType<playerMovement>();
        }

        playerMovement movement = player.GetComponent<playerMovement>();
        if (movement == null)
        {
            movement = player.GetComponentInChildren<playerMovement>(true);
        }

        return movement ?? FindFirstObjectByType<playerMovement>();
    }

    private void StartIdleSound()
    {
        if (isIdleSoundPlaying || isUsed)
        {
            return;
        }

        AudioClip clipToPlay = fountainSound;
        if (clipToPlay == null && audioClips != null)
        {
            clipToPlay = audioClips.fountainSound;
        }

        if (clipToPlay == null)
        {
            return;
        }

        AudioManager.PlayLoopingSfx(clipToPlay, fountainVolume, 1f, GetIdleAudioChannel());
        isIdleSoundPlaying = true;
    }

    private void StopIdleSound()
    {
        if (!isIdleSoundPlaying)
        {
            return;
        }

        AudioManager.StopLoopingSfx(GetIdleAudioChannel());
        isIdleSoundPlaying = false;
    }

    private string GetIdleAudioChannel()
    {
        return audioChannel + "_" + GetInstanceID();
    }

    private void MarkUsed()
    {
        isUsed = true;
        RefreshState();
        StopIdleSound();
    }

    private void OnBecameVisible()
    {
        isVisible = true;
        if (!isUsed)
        {
            StartIdleSound();
        }
    }

    private void OnBecameInvisible()
    {
        isVisible = false;
        StopIdleSound();
    }

    private void RefreshState()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        spriteRenderer.sprite = isUsed ? usedSprite : activeSprite;
    }
}
