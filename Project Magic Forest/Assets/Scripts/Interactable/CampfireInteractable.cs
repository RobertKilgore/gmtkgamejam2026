using UnityEngine;
using UnityEngine.Rendering.Universal;

public enum CampfireState
{
    Unlit = 0,
    Lit = 1,
    Deactive = 2
}

[RequireComponent(typeof(SpriteRenderer))]
public sealed class CampfireInteractable : Interactable
{
    [Header("Campfire State")]
    [SerializeField] private CampfireState campfireState = CampfireState.Unlit;
    [SerializeField] private Sprite unlitSprite;
    [SerializeField] private Sprite deactiveSprite;

    [Header("Lit State")]
    [SerializeField] private Sprite litSprite;
    [SerializeField] private Sprite[] litAnimationFrames = new Sprite[0];
    [SerializeField, Range(1f, 60f)] private float litFrameRate = 12f;
    [SerializeField] private Light2D campfireLight;
    [SerializeField] private TimerModifierArea modifierArea;

    [Header("Audio")]
    [SerializeField] private AudioClips audioClips;
    [SerializeField] private AudioClip litLoopSound;
    [SerializeField, Range(0f, 1f)] private float litVolume = 1f;
    [SerializeField] private string audioChannel = "campfire";

    [Header("Duration")]
    [SerializeField] private float litDuration = -1f;

    private SpriteRenderer spriteRenderer;
    private float litTimer;
    private float litFrameTimer;
    private int currentLitFrameIndex;

    protected override void Awake()
    {
        base.Awake();

        spriteRenderer = GetComponent<SpriteRenderer>();

        RefreshState();
    }

    public override bool CanInteract()
    {
        return base.CanInteract() && campfireState == CampfireState.Unlit;
    }

    protected override void HandleInteraction(PlayerInventory inventory, GameObject player)
    {
        if (campfireState != CampfireState.Unlit)
        {
            return;
        }

        if (campfireState == CampfireState.Unlit)
        {
            SetCampfireState(CampfireState.Lit);
            base.HandleInteraction(inventory, player);
        }
    }

    public void SetCampfireState(CampfireState newState)
    {
        if (campfireState == newState)
            return;

        campfireState = newState;
        RefreshState();

        if (campfireState == CampfireState.Lit)
        {
            litTimer = 0f;
        }
    }

    public void Activate()
    {
        if (campfireState == CampfireState.Deactive)
        {
            SetCampfireState(CampfireState.Unlit);
        }
    }

    public void DeactivateCampfire()
    {
        SetCampfireState(CampfireState.Deactive);
    }

    public void LightCampfire()
    {
        SetCampfireState(CampfireState.Lit);
    }

    public void ExtinguishCampfire()
    {
        SetCampfireState(CampfireState.Unlit);
    }

    protected override void ApplyState(int stateIndex)
    {
        campfireState = (CampfireState)Mathf.Clamp(stateIndex, 0, 2);
        RefreshState();
    }

    private void RefreshState()
    {
        bool isLit = campfireState == CampfireState.Lit;
        bool isDeactive = campfireState == CampfireState.Deactive;
        bool isUnlit = campfireState == CampfireState.Unlit;

        if (spriteRenderer != null)
        {
            if (isLit)
            {
                if (litAnimationFrames != null && litAnimationFrames.Length > 0)
                {
                    spriteRenderer.sprite = litAnimationFrames[currentLitFrameIndex];
                }
                else
                {
                    spriteRenderer.sprite = litSprite;
                }
            }
            else if (isUnlit)
            {
                spriteRenderer.sprite = unlitSprite;
            }
            else if (isDeactive)
            {
                spriteRenderer.sprite = deactiveSprite;
            }
        }

        if (campfireLight != null)
        {
            campfireLight.enabled = isLit;
        }

        if (modifierArea != null)
        {
            modifierArea.SetAreaActive(isLit);
        }

        if (!isLit)
        {
            litTimer = 0f;
            litFrameTimer = 0f;
            currentLitFrameIndex = 0;
        }

        UpdateAudio(isLit);
    }

    private void Update()
    {
        if (campfireState == CampfireState.Lit)
        {
            if (litDuration > 0f)
            {
                litTimer += Time.deltaTime;
                if (litTimer >= litDuration)
                {
                    SetCampfireState(CampfireState.Deactive);
                    return;
                }
            }

            if (litAnimationFrames != null && litAnimationFrames.Length > 0 && litFrameRate > 0f)
            {
                litFrameTimer += Time.deltaTime;
                float frameDuration = 1f / litFrameRate;
                if (litFrameTimer >= frameDuration)
                {
                    litFrameTimer -= frameDuration;
                    currentLitFrameIndex = (currentLitFrameIndex + 1) % litAnimationFrames.Length;
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.sprite = litAnimationFrames[currentLitFrameIndex];
                    }
                }
            }
        }
    }

    private void UpdateAudio(bool enabled)
    {
        AudioClip clipToPlay = litLoopSound;
        if (clipToPlay == null && audioClips != null)
        {
            clipToPlay = audioClips.fire;
        }

        if (clipToPlay == null)
        {
            Debug.LogWarning("[CampfireInteractable] No fire audio clip assigned and AudioClips asset is missing the fire clip.");
            return;
        }

        if (enabled)
        {
            AudioManager.PlayLoopingSfx(clipToPlay, litVolume, 1f, audioChannel);
        }
        else
        {
            AudioManager.StopLoopingSfx(audioChannel);
        }
    }
}
