using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public enum DragonState
{
    Sleeping = 0,
    Deactivated = 1
}

[RequireComponent(typeof(SpriteRenderer))]
public sealed class DragonInteractable : Interactable
{
    [Header("Dragon State")]
    [SerializeField] private DragonState dragonState = DragonState.Sleeping;
    [SerializeField] private Sprite sleepingSprite;
    [SerializeField] private List<Sprite> sleepingAnimationFrames = new List<Sprite>();
    [SerializeField, Range(1f, 30f)] private float sleepingFrameRate = 8f;

    [Header("Positive Outcome")]
    [SerializeField] private Sprite rewardSprite;
    [SerializeField, Range(0f, 100f)] private float positiveOutcomeChance = 70f;
    [SerializeField] private Vector2 foodRewardRange = new Vector2(10f, 25f);
    [SerializeField] private Vector2 sleepRewardRange = new Vector2(10f, 25f);
    [SerializeField] private Vector2 fuelRewardRange = new Vector2(10f, 25f);
    [SerializeField] private string foodTimerKey = "Food";
    [SerializeField] private string sleepTimerKey = "Sleep";
    [SerializeField] private string fuelTimerKey = "Fuel";

    [Header("Negative Outcome")]
    [SerializeField] private List<Sprite> scorchAnimationFrames = new List<Sprite>();
    [SerializeField, Range(1f, 30f)] private float scorchFrameRate = 12f;
    [SerializeField, Min(1)] private int scorchLoopCount = 2;
    [SerializeField] private float scorchTemperatureBonus = 30f;
    [SerializeField] private Light2D scorchLight;

    [Header("Audio")]
    [SerializeField] private AudioClips audioClips;
    [SerializeField] private AudioClip sleepingBreathSound;
    [SerializeField] private AudioClip positiveOutcomeSound;
    [SerializeField] private AudioClip negativeOutcomeSound;
    [SerializeField, Range(0f, 1f)] private float outcomeVolume = 1f;
    [SerializeField] private string audioChannel = "dragon";

    private enum RewardType
    {
        Food = 0,
        Sleep = 1,
        Fuel = 2
    }

    private SpriteRenderer spriteRenderer;
    private PlayerTimers playerTimers;
    private bool isProcessingOutcome;
    private bool isVisible;
    private float sleepingFrameTimer;
    private int currentSleepingFrameIndex;

    protected override void Awake()
    {
        base.Awake();
        spriteRenderer = GetComponent<SpriteRenderer>();
        RefreshState();
    }

    private void Update()
    {
        if (dragonState == DragonState.Sleeping && !isProcessingOutcome)
        {
            AnimateSleeping();
        }
    }

    public override bool CanInteract()
    {
        return base.CanInteract() && dragonState == DragonState.Sleeping && !isProcessingOutcome;
    }

    protected override void HandleInteraction(PlayerInventory inventory, GameObject player)
    {
        if (dragonState != DragonState.Sleeping || isProcessingOutcome)
        {
            return;
        }

        playerTimers = ResolvePlayerTimers(player);
        bool positiveOutcome = Random.value <= positiveOutcomeChance / 100f;

        if (positiveOutcome)
        {
            ApplyPositiveOutcome();
        }
        else
        {
            StartCoroutine(PlayNegativeOutcome());
        }

        base.HandleInteraction(inventory, player);
    }

    private void RefreshState()
    {
        bool isSleeping = dragonState == DragonState.Sleeping;
        bool isDeactivated = dragonState == DragonState.Deactivated;

        if (spriteRenderer != null)
        {
            if (isSleeping)
            {
                if (sleepingAnimationFrames.Count > 0)
                {
                    spriteRenderer.sprite = sleepingAnimationFrames[Mathf.Clamp(currentSleepingFrameIndex, 0, sleepingAnimationFrames.Count - 1)];
                }
                else if (sleepingSprite != null)
                {
                    spriteRenderer.sprite = sleepingSprite;
                }
            }
            else if (rewardSprite != null && !isProcessingOutcome)
            {
                spriteRenderer.sprite = rewardSprite;
            }
        }

        if (scorchLight != null)
        {
            scorchLight.enabled = false;
        }
    }

    private void AnimateSleeping()
    {
        if (sleepingAnimationFrames == null || sleepingAnimationFrames.Count == 0 || sleepingFrameRate <= 0f || spriteRenderer == null)
        {
            return;
        }

        sleepingFrameTimer += Time.deltaTime;
        float frameDuration = 1f / sleepingFrameRate;
        if (sleepingFrameTimer >= frameDuration)
        {
            sleepingFrameTimer -= frameDuration;
            currentSleepingFrameIndex = (currentSleepingFrameIndex + 1) % sleepingAnimationFrames.Count;
            spriteRenderer.sprite = sleepingAnimationFrames[currentSleepingFrameIndex];
        }
    }

    private PlayerTimers ResolvePlayerTimers(GameObject player)
    {
        if (player == null)
        {
            return FindFirstObjectByType<PlayerTimers>();
        }

        PlayerTimers timers = player.GetComponent<PlayerTimers>();
        if (timers == null)
        {
            timers = player.GetComponentInChildren<PlayerTimers>(true);
        }

        return timers ?? FindFirstObjectByType<PlayerTimers>();
    }

    private void ApplyPositiveOutcome()
    {
        isProcessingOutcome = true;
        dragonState = DragonState.Deactivated;
        SetHighlighted(false);
        currentSleepingFrameIndex = 0;
        sleepingFrameTimer = 0f;

        if (spriteRenderer != null && rewardSprite != null)
        {
            spriteRenderer.sprite = rewardSprite;
        }

        PlayOutcomeSound(true);
        GrantRandomReward();
        FinalizeInteraction();
    }

    private IEnumerator PlayNegativeOutcome()
    {
        isProcessingOutcome = true;
        dragonState = DragonState.Deactivated;
        SetHighlighted(false);

        PlayOutcomeSound(false);

        if (scorchLight != null)
        {
            scorchLight.enabled = true;
        }

        yield return PlayScorchAnimation();

        if (scorchLight != null)
        {
            scorchLight.enabled = false;
        }

        ApplyNegativeEffects();
        FinalizeInteraction();
    }

    private IEnumerator PlayScorchAnimation()
    {
        if (scorchAnimationFrames == null || scorchAnimationFrames.Count == 0 || scorchFrameRate <= 0f)
        {
            yield return null;
            yield break;
        }

        float frameDuration = 1f / scorchFrameRate;
        int totalLoops = Mathf.Max(1, scorchLoopCount);
        int frameIndex = 0;

        for (int loop = 0; loop < totalLoops; loop++)
        {
            for (int i = 0; i < scorchAnimationFrames.Count; i++)
            {
                if (spriteRenderer != null && scorchAnimationFrames[i] != null)
                {
                    spriteRenderer.sprite = scorchAnimationFrames[i];
                }

                yield return new WaitForSeconds(frameDuration);
            }
        }
    }

    private void GrantRandomReward()
    {
        if (playerTimers == null)
        {
            Debug.LogWarning("[DragonInteractable] PlayerTimers not found. Cannot grant reward.");
            return;
        }

        RewardType choice = (RewardType)Random.Range(0, 3);
        float amount = 0f;
        string timerKey = null;

        switch (choice)
        {
            case RewardType.Food:
                amount = Random.Range(foodRewardRange.x, foodRewardRange.y);
                timerKey = foodTimerKey;
                break;
            case RewardType.Sleep:
                amount = Random.Range(sleepRewardRange.x, sleepRewardRange.y);
                timerKey = sleepTimerKey;
                break;
            case RewardType.Fuel:
                amount = Random.Range(fuelRewardRange.x, fuelRewardRange.y);
                timerKey = fuelTimerKey;
                break;
        }

        Timer timer = playerTimers.FindTimer(timerKey);
        if (timer != null)
        {
            timer.AddTime(amount);
            Debug.Log($"[DragonInteractable] Positive reward: {choice} +{amount:F1} to timer '{timerKey}'.");
        }
        else
        {
            Debug.LogWarning($"[DragonInteractable] Timer '{timerKey}' not found for reward {choice}.");
        }
    }

    private void ApplyNegativeEffects()
    {
        if (playerTimers == null)
        {
            Debug.LogWarning("[DragonInteractable] PlayerTimers not found. Cannot apply negative effects.");
            return;
        }

        Timer fuelTimer = playerTimers.FindTimer(fuelTimerKey);
        if (fuelTimer != null)
        {
            fuelTimer.SetTimer(0f);
            Debug.Log("[DragonInteractable] Negative outcome: Fuel set to 0.");
        }
        else
        {
            Debug.LogWarning($"[DragonInteractable] Fuel timer '{fuelTimerKey}' not found.");
        }

        if (playerTimers.TemperatureTimer != null)
        {
            playerTimers.TemperatureTimer.AddTime(scorchTemperatureBonus);
            Debug.Log($"[DragonInteractable] Negative outcome: Temperature +{scorchTemperatureBonus:F1}.");
        }
        else
        {
            Debug.LogWarning("[DragonInteractable] TemperatureTimer not found.");
        }
    }

    private void PlayOutcomeSound(bool isPositive)
    {
        AudioClip clipToPlay = null;

        if (isPositive)
        {
            clipToPlay = positiveOutcomeSound;
            if (clipToPlay == null && audioClips != null)
            {
                clipToPlay = audioClips.getItemSound;
            }
        }
        else
        {
            clipToPlay = negativeOutcomeSound;
            if (clipToPlay == null && audioClips != null)
            {
                clipToPlay = audioClips.dragonAngry;
            }
        }

        if (clipToPlay == null)
        {
            Debug.LogWarning("[DragonInteractable] No outcome sound assigned.");
            return;
        }

        string channel = isPositive ? audioChannel : audioChannel + "_angry";
        AudioManager.PlaySFX(clipToPlay, outcomeVolume, 1f, channel);
    }

    private void FinalizeInteraction()
    {
        StopBreathingLoop();
        dragonState = DragonState.Deactivated;
        SetHighlighted(false);
        enabled = false;
    }

    private void StartBreathingLoop()
    {
        if (isProcessingOutcome || dragonState != DragonState.Sleeping || !isVisible)
        {
            return;
        }

        AudioClip breathClip = sleepingBreathSound;
        if (breathClip == null && audioClips != null)
        {
            breathClip = audioClips.dragonBreathing;
        }

        if (breathClip == null)
        {
            return;
        }

        AudioManager.PlayLoopingSfx(breathClip, outcomeVolume, 1f, audioChannel + "_breath");
    }

    private void StopBreathingLoop()
    {
        AudioManager.StopLoopingSfx(audioChannel + "_breath");
    }

    private void OnBecameVisible()
    {
        isVisible = true;
        if (!isProcessingOutcome && dragonState == DragonState.Sleeping)
        {
            StartBreathingLoop();
        }
    }

    private void OnBecameInvisible()
    {
        isVisible = false;
        StopBreathingLoop();
    }

    private void OnDisable()
    {
        StopBreathingLoop();
    }
}
