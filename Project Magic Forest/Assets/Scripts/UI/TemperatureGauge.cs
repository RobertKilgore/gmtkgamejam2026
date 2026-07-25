using UnityEngine;
using UnityEngine.UI;

public class TemperatureGauge : MonoBehaviour
{
    [Header("Timer")]
    [SerializeField] private Timer timer;
    [SerializeField] private string timerKey = "Temperature";

    [Header("UI")]
    [SerializeField] private Slider slider;
    [SerializeField] private Image fillImage;

    [Header("Range")]
    [SerializeField] private float minValue = 0f;
    [SerializeField] private float maxValue = 100f;

    [Header("Gradient")]
    [SerializeField] private Gradient temperatureGradient;

    private void Awake()
    {
        if (slider == null)
        {
            slider = GetComponentInChildren<Slider>();
        }

        if (fillImage == null && slider != null)
        {
            fillImage = slider.fillRect != null ? slider.fillRect.GetComponent<Image>() : null;
        }
        // Timer resolution is performed in Start() to allow PlayerTimers to finish registering.

        if (slider != null)
        {
            float resolvedMax = GetMaxValue();
            slider.minValue = minValue;
            slider.maxValue = resolvedMax;
            slider.wholeNumbers = false;
        }
    }

    private void Start()
    {
        if (timer == null && !string.IsNullOrEmpty(timerKey))
        {
            StartCoroutine(ResolveTimerCoroutine());
        }
    }

    private System.Collections.IEnumerator ResolveTimerCoroutine()
    {
        const int maxFrames = 120; // try for up to 2 seconds at 60fps
        int attempts = 0;
        while (attempts < maxFrames && timer == null)
        {
            var playerTimers = FindFirstObjectByType<PlayerTimers>();
            if (playerTimers != null && playerTimers.TrackedTimers != null && playerTimers.TrackedTimers.Count > 0)
            {
                timer = playerTimers.FindTimer(timerKey);
                if (timer != null)
                {
                    // update slider max to match the timer
                    if (slider != null)
                    {
                        slider.maxValue = GetMaxValue();
                    }
                    yield break;
                }
            }

            attempts++;
            yield return null;
        }

        if (timer == null)
        {
            Debug.LogWarning($"[TemperatureGauge] Timer with key '{timerKey}' could not be resolved from PlayerTimers after waiting. Gauge will remain disabled.");
        }
    }

    private float GetMaxValue()
    {
        if (timer == null)
        {
            return maxValue;
        }

        if (!float.IsInfinity(timer.MaxTime) && !float.IsNaN(timer.MaxTime))
        {
            return timer.MaxTime;
        }

        return maxValue;
    }

    private void Update()
    {
        if (timer == null || slider == null)
        {
            return;
        }

        float resolvedMax = GetMaxValue();
        float value = Mathf.Clamp(timer.TimeRemaining, minValue, resolvedMax);
        slider.value = value;

        if (fillImage != null && temperatureGradient != null)
        {
            float t = Mathf.InverseLerp(minValue, resolvedMax, value);
            fillImage.color = temperatureGradient.Evaluate(1f - t);
        }
    }
}
