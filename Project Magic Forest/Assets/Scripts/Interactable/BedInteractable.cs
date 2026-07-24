using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class BedInteractable : Interactable
{
    [Header("Sleep Settings")]
    [SerializeField] private float sleepGainPerSecond = 1f;
    [SerializeField] private float sleepTimeScale = 0.2f;
    [SerializeField] private float transitionDuration = 1f;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private bool disablePlayerMovementWhileSleeping = true;

    [Header("Fade Overlay")]
    [SerializeField] private bool showFadeOverlayInEditor = true;
    [SerializeField] private int overlaySortingOrder = -100;
    [Range(0f, 1f)]
    [SerializeField] private float maxFadeAlpha = 0.8f;
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private Image fadeImage;
    [SerializeField] private float currentTimeScaleDisplay = 1f;

    private bool isSleeping;
    private Coroutine transitionRoutine;
    private playerMovement playerMovement;
    private PlayerTimers playerTimers;
    private float originalTimeScale = 1f;

    protected override void Awake()
    {
        base.Awake();
        EnsureOverlay();
        RefreshOverlayVisibility();
        SetOverlayAlpha(0f);
    }

    private void Update()
    {
        currentTimeScaleDisplay = Time.timeScale;
        RefreshOverlayVisibility();
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

    private void ApplySleepGain()
    {
        if (!isSleeping || playerTimers == null || sleepGainPerSecond <= 0f)
        {
            return;
        }

        if (playerTimers.SleepTimer != null)
        {
            float gain = sleepGainPerSecond * Time.unscaledDeltaTime;
            playerTimers.SleepTimer.AddTime(gain);
        }
    }

    private void BeginSleep(GameObject player)
    {
        originalTimeScale = Time.timeScale <= 0f ? 1f : Time.timeScale;
        playerTimers = player != null ? player.GetComponent<PlayerTimers>() : null;
        if (playerTimers == null && player != null)
        {
            playerTimers = player.GetComponentInChildren<PlayerTimers>(true);
        }

        if (playerTimers != null && playerTimers.SleepTimer != null)
        {
            playerTimers.SleepTimer.AddAdditiveModifier("bed_sleep_gain", sleepGainPerSecond);
        }

        playerMovement = player != null ? player.GetComponent<playerMovement>() : null;
        if (playerMovement == null && player != null)
        {
            playerMovement = player.GetComponentInChildren<playerMovement>(true);
        }

        if (disablePlayerMovementWhileSleeping && playerMovement != null)
        {
            playerMovement.enabled = false;
            Rigidbody2D rb = playerMovement.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }

        transitionRoutine = StartCoroutine(TransitionToSleep(sleepTimeScale, 1f));
    }

    private void FixedUpdate()
    {
        ApplySleepGain();
    }

    private void EndSleep()
    {
        if (playerTimers != null && playerTimers.SleepTimer != null)
        {
            playerTimers.SleepTimer.RemoveAdditiveModifier("bed_sleep_gain");
        }

        if (disablePlayerMovementWhileSleeping && playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        transitionRoutine = StartCoroutine(TransitionToSleep(originalTimeScale, 0f));
    }

    private IEnumerator TransitionToSleep(float targetTimeScale, float targetAlpha)
    {
        float startTimeScale = Time.timeScale;
        float startAlpha = fadeCanvasGroup != null ? fadeCanvasGroup.alpha : 0f;
        float fadeTransitionDuration = fadeDuration > 0f ? fadeDuration : transitionDuration;
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            float t = Mathf.Clamp01(elapsed / transitionDuration);
            float easedT = Mathf.SmoothStep(0f, 1f, t);
            Time.timeScale = Mathf.Lerp(startTimeScale, targetTimeScale, easedT);
            float alphaProgress = Mathf.Clamp01(elapsed / fadeTransitionDuration);
            float easedAlpha = Mathf.SmoothStep(0f, 1f, alphaProgress);
            SetOverlayAlpha(Mathf.Lerp(startAlpha, targetAlpha, easedAlpha));
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        Time.timeScale = targetTimeScale;
        SetOverlayAlpha(targetAlpha);
        transitionRoutine = null;
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
        fadeCanvasGroup.blocksRaycasts = true;

        fadeImage = overlayObject.AddComponent<Image>();
        fadeImage.color = new Color(0f, 0f, 0f, 0f);
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
        }

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.blocksRaycasts = shouldShowOverlay;
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
