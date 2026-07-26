using TMPro;
using UnityEngine;

public class TimerTextDisplay : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private PlayerTimers playerTimers;
    [SerializeField] private string timerKey = "";

    [Header("Text")]
    [SerializeField] private TMP_Text textComponent;

    [Header("Formatting")]
    [SerializeField] private string format = "0";

    [Header("Threshold Gradient")]
    [SerializeField] private float threshold = 10f;
    [SerializeField] private Color lowColor = Color.red;
    [SerializeField] private Color highColor = Color.white;

    private Timer timer;

    private void Awake()
    {
        if (playerTimers == null)
        {
            playerTimers = FindFirstObjectByType<PlayerTimers>();
        }

        if (textComponent == null)
        {
            textComponent = GetComponentInChildren<TMP_Text>();
        }
    }

    private void Start()
    {
        ResolveTimer();
    }

    private void Update()
    {
        if (textComponent == null)
        {
            return;
        }

        if (playerTimers == null)
        {
            playerTimers = FindFirstObjectByType<PlayerTimers>();
            if (playerTimers == null)
            {
                return;
            }
        }

        if (timer == null)
        {
            ResolveTimer();
            if (timer == null)
            {
                textComponent.text = "--";
                return;
            }
        }

        int secondsLeft = Mathf.CeilToInt(timer.TimeRemaining);
        textComponent.text = secondsLeft.ToString(format);

        if (threshold > 0f)
        {
            float t = Mathf.Clamp01(secondsLeft / threshold);
            textComponent.color = Color.Lerp(highColor, lowColor, 1f - t);
        }
    }

    private void ResolveTimer()
    {
        if (timer != null || playerTimers == null || string.IsNullOrEmpty(timerKey))
        {
            return;
        }

        timer = playerTimers.FindTimer(timerKey);
    }
}
