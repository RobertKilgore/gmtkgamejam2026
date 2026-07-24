using UnityEngine;
using UnityEngine.UI;

public class TemperatureGauge : MonoBehaviour
{
    [Header("Timer")]
    [SerializeField] private Timer timer;

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

        if (timer == null)
        {
            timer = GetComponentInParent<Timer>();
        }

        if (slider != null)
        {
            float resolvedMax = GetMaxValue();
            slider.minValue = minValue;
            slider.maxValue = resolvedMax;
            slider.wholeNumbers = false;
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
