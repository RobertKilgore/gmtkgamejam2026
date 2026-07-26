using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class BedInteractable : Interactable
{
    [Header("Sleep Settings")]
    [SerializeField] private float sleepGainPerSecond = 1f;
    [SerializeField] private float sleepTimeScale = 0.2f;
    [SerializeField] private float sleepTransitionDuration = 1f;
    [SerializeField] private float wakeTransitionDuration = 1f;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private bool disablePlayerMovementWhileSleeping = true;

    [Header("Fade Overlay")]
    [SerializeField] private bool showFadeOverlayInEditor = true;
    [SerializeField] private int overlaySortingOrder = -100;
    [Range(0f, 1f)]
    [SerializeField] private float maxFadeAlpha = 0.8f;
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private Image fadeImage;

    private bool isSleeping;
    private bool isTransitioning;
    private Coroutine transitionRoutine;
    private playerMovement playerMovement;
    private PlayerTimers playerTimers;
    private float normalTimeScale = 1f;
    private int transitionVersion;

    protected override void Awake()
    {
        base.Awake();
        normalTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;
        EnsureOverlay();
        RefreshOverlayVisibility();
        SetOverlayAlpha(0f);
    }

    private void Update()
    {
        RefreshOverlayVisibility();

        if (!isSleeping && !isTransitioning && Time.timeScale > 0f)
        {
            normalTimeScale = Time.timeScale;
        }
    }

    private void FixedUpdate()
    {
        ApplySleepGain();
    }

    protected override void HandleInteraction(PlayerInventory inventory, GameObject player)
    {
        ToggleSleep(player);
        base.HandleInteraction(inventory, player);
    }

    protected override void ApplyState(int stateIndex)
    {
        isSleeping = stateIndex != 0;
    }

    private void ToggleSleep(GameObject player)
    {
        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
            transitionRoutine = null;
        }

        bool shouldSleep = !isSleeping;
        SetState(shouldSleep ? 1 : 0);

        if (shouldSleep)
        {
            BeginSleep(player);
        }
        else
        {
            EndSleep();
        }
    }

    private void BeginSleep(GameObject player)
    {
        if (Time.timeScale > 0f)
        {
            normalTimeScale = Time.timeScale;
        }

        ResolvePlayerReferences(player);
        AddSleepGain();
        SetPlayerMovementEnabled(false);

        transitionVersion++;
        float startTimeScale = Time.timeScale > 0f ? Time.timeScale : normalTimeScale;
        Debug.Log($"[Bed] Begin sleep | start={startTimeScale:F3} target={sleepTimeScale:F3} duration={sleepTransitionDuration:F3}");
        transitionRoutine = StartCoroutine(RunTransition(startTimeScale, sleepTimeScale, 1f, transitionVersion));
    }

    private void EndSleep()
    {
        SetPlayerMovementEnabled(false);

        transitionVersion++;
        float startTimeScale = Time.timeScale > 0f ? Time.timeScale : normalTimeScale;
        Debug.Log($"[Bed] Begin wake | start={startTimeScale:F3} target={normalTimeScale:F3} duration={wakeTransitionDuration:F3}");
        transitionRoutine = StartCoroutine(RunTransition(startTimeScale, 1, 0f, transitionVersion));
    }

    private IEnumerator RunTransition(float startTimeScale, float targetTimeScale, float targetAlpha, int transitionId)
    {
        isTransitioning = true;
        float startAlpha = fadeCanvasGroup != null ? fadeCanvasGroup.alpha : 0f;
        float timeScaleDuration = targetTimeScale < startTimeScale ? Mathf.Max(sleepTransitionDuration, 0.01f) : Mathf.Max(wakeTransitionDuration, 0.01f);
        float fadeDurationValue = Mathf.Max(fadeDuration, 0.01f);
        float elapsed = 0f;

        Time.timeScale = startTimeScale;
        SetOverlayAlpha(startAlpha);

        while (elapsed < timeScaleDuration)
        {
            if (transitionId != transitionVersion)
            {
                yield break;
            }

            float timeT = Mathf.Clamp01(elapsed / timeScaleDuration);
            float easedTimeT = Mathf.SmoothStep(0f, 1f, timeT);
            Time.timeScale = Mathf.Lerp(startTimeScale, targetTimeScale, easedTimeT);

            float fadeT = Mathf.Clamp01(elapsed / fadeDurationValue);
            float easedFadeT = Mathf.SmoothStep(0f, 1f, fadeT);
            SetOverlayAlpha(Mathf.Lerp(startAlpha, targetAlpha, easedFadeT));

            if (Time.frameCount % 10 == 0)
            {
                float currentAlpha = fadeCanvasGroup != null ? fadeCanvasGroup.alpha : 0f;
                Debug.Log($"[Bed] Transition tick | elapsed={elapsed:F3} timeScale={Time.timeScale:F3} alpha={currentAlpha:F3}");
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (transitionId != transitionVersion)
        {
            yield break;
        }

        Time.timeScale = targetTimeScale;
        SetOverlayAlpha(targetAlpha);
        Debug.Log($"[Bed] Transition complete | finalTimeScale={Time.timeScale:F3} targetAlpha={targetAlpha:F3}");

        if (isSleeping)
        {
            SetPlayerMovementEnabled(false);
        }
        else
        {
            RemoveSleepGain();
        }

        if (!isSleeping && disablePlayerMovementWhileSleeping && playerMovement != null)
        {
            SetPlayerMovementEnabled(true);
        }

        transitionRoutine = null;
        isTransitioning = false;
    }

    private void ApplySleepGain()
    {
        if (!isSleeping || playerTimers == null || sleepGainPerSecond <= 0f)
        {
            return;
        }

        if (playerTimers.SleepTimer != null)
        {
            playerTimers.SleepTimer.AddTime(sleepGainPerSecond * Time.unscaledDeltaTime);
        }
    }

    private void ResolvePlayerReferences(GameObject player)
    {
        playerTimers = player != null ? player.GetComponent<PlayerTimers>() : null;
        if (playerTimers == null && player != null)
        {
            playerTimers = player.GetComponentInChildren<PlayerTimers>(true);
        }

        playerMovement = player != null ? player.GetComponent<playerMovement>() : null;
        if (playerMovement == null && player != null)
        {
            playerMovement = player.GetComponentInChildren<playerMovement>(true);
        }
    }

    private void AddSleepGain()
    {
        if (playerTimers != null && playerTimers.SleepTimer != null)
        {
            playerTimers.SleepTimer.AddAdditiveModifier("bed_sleep_gain", sleepGainPerSecond);
        }
    }

    private void RemoveSleepGain()
    {
        if (playerTimers != null && playerTimers.SleepTimer != null)
        {
            Debug.Log("[Bed] Removing sleep gain modifier from player timers.");
            playerTimers.SleepTimer.RemoveAdditiveModifier("bed_sleep_gain");
        }
    }

    private void SetPlayerMovementEnabled(bool enabled)
    {
        if (!disablePlayerMovementWhileSleeping || playerMovement == null)
        {
            return;
        }

        playerMovement.enabled = enabled;

        if (!enabled)
        {
            Rigidbody2D rb = playerMovement.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    private void EnsureOverlay()
    {
        if (fadeCanvasGroup != null && fadeImage != null)
        {
            return;
        }

        GameObject overlayObject = new GameObject("BedFadeOverlay");
        overlayObject.transform.SetParent(transform, false);

        Canvas canvas = overlayObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = overlaySortingOrder;

        fadeCanvasGroup = overlayObject.AddComponent<CanvasGroup>();
        fadeCanvasGroup.blocksRaycasts = false;
        fadeCanvasGroup.interactable = false;

        fadeImage = overlayObject.AddComponent<Image>();
        fadeImage.color = new Color(0f, 0f, 0f, 0f);
        fadeImage.raycastTarget = false;
        fadeImage.rectTransform.anchorMin = Vector2.zero;
        fadeImage.rectTransform.anchorMax = Vector2.one;
        fadeImage.rectTransform.sizeDelta = Vector2.zero;
        fadeImage.rectTransform.anchoredPosition = Vector2.zero;
    }

    private void RefreshOverlayVisibility()
    {
        bool shouldShowOverlay = showFadeOverlayInEditor || Application.isPlaying;
        if (fadeImage != null)
        {
            fadeImage.enabled = shouldShowOverlay;
            fadeImage.raycastTarget = false;
        }

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.blocksRaycasts = false;
            fadeCanvasGroup.interactable = false;
        }
    }

    private void SetOverlayAlpha(float alpha)
    {
        float effectiveAlpha = Mathf.Clamp01(alpha) * maxFadeAlpha;

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = effectiveAlpha;
        }

        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = effectiveAlpha;
            fadeImage.color = color;
        }
    }
}
