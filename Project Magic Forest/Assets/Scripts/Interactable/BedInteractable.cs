using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class BedInteractable : Interactable
{
    [Header("Sleep Settings")]
    [SerializeField] private string timerKey = "sleep";
    [SerializeField] private string modifierId = "sleep";
    [SerializeField] private float sleepTimeScale = 0.2f;
    [SerializeField] private float transitionDuration = 1f;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private bool disablePlayerMovementWhileSleeping = true;

    [Header("Fade Overlay")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private Image fadeImage;

    private bool isSleeping;
    private Coroutine transitionRoutine;
    private playerMovement playerMovement;
    private PlayerTimers playerTimers;
    private float originalTimeScale = 1f;

    protected override void Awake()
    {
        base.Awake();
        EnsureOverlay();
        SetOverlayAlpha(0f);
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

    private void BeginSleep(GameObject player)
    {
        originalTimeScale = Time.timeScale <= 0f ? 1f : Time.timeScale;
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

        if (playerTimers != null && !string.IsNullOrEmpty(timerKey) && !string.IsNullOrEmpty(modifierId))
        {
            playerTimers.AddModifierToTimer(timerKey, modifierId, -1f);
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

    private void EndSleep()
    {
        if (playerTimers != null && !string.IsNullOrEmpty(timerKey) && !string.IsNullOrEmpty(modifierId))
        {
            playerTimers.RemoveModifierFromTimer(timerKey, modifierId);
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
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            float t = Mathf.Clamp01(elapsed / transitionDuration);
            float easedT = Mathf.SmoothStep(0f, 1f, t);
            Time.timeScale = Mathf.Lerp(startTimeScale, targetTimeScale, easedT);
            SetOverlayAlpha(Mathf.Lerp(startAlpha, targetAlpha, easedT));
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
        canvas.sortingOrder = 1000;

        fadeCanvasGroup = overlayObject.AddComponent<CanvasGroup>();
        fadeCanvasGroup.blocksRaycasts = true;

        fadeImage = overlayObject.AddComponent<Image>();
        fadeImage.color = new Color(0f, 0f, 0f, 0f);
        fadeImage.rectTransform.anchorMin = Vector2.zero;
        fadeImage.rectTransform.anchorMax = Vector2.one;
        fadeImage.rectTransform.sizeDelta = Vector2.zero;
        fadeImage.rectTransform.anchoredPosition = Vector2.zero;
    }

    private void SetOverlayAlpha(float alpha)
    {
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = alpha;
        }

        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = alpha;
            fadeImage.color = color;
        }
    }
}
