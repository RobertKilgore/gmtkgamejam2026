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

    private void Update()
    {
        if (playerTimers == null || textComponent == null)
        {
            return;
        }

        
        if (timer == null)
        {
            timer = playerTimers.FindTimer(timerKey);
            textComponent.text = "--";
            return;
        }

        int secondsLeft = Mathf.CeilToInt(timer.TimeRemaining);
        textComponent.text = secondsLeft.ToString(format);

        if (threshold > 0f)
        {
            float t = Mathf.Clamp01(secondsLeft / threshold);
            textComponent.color = Color.Lerp(highColor, lowColor, 1f - t);
        }
    }
}
