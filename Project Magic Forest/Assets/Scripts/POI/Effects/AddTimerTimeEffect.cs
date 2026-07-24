using UnityEngine;

[CreateAssetMenu(menuName = "Magic Forest/POI/Effects/Add Timer Time")]
public sealed class AddTimerTimeEffect : PoiEffect
{
    [SerializeField] private string timerKey = "";
    [SerializeField] private float timeToAdd = 0f;

    public override bool Apply(PoiContext context)
    {
        if (context.PlayerTimers == null)
        {
            Debug.LogWarning("[AddTimerTimeEffect] PlayerTimers is null!");
            return false;
        }

        if (string.IsNullOrEmpty(timerKey))
        {
            Debug.LogWarning("[AddTimerTimeEffect] Timer key is empty!");
            return false;
        }

        Timer timer = context.PlayerTimers.FindTimer(timerKey);
        if (timer == null)
        {
            Debug.LogWarning($"[AddTimerTimeEffect] Timer '{timerKey}' not found!");
            return false;
        }

        timer.AddTime(timeToAdd);
        Debug.Log($"[AddTimerTimeEffect] Added {timeToAdd} seconds to timer '{timerKey}' (new time: {timer.TimeRemaining}s)");
        return true;
    }
}
