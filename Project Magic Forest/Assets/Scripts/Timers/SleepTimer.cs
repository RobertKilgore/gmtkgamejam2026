using UnityEngine;

public class SleepTimer : Timer
{
    [Header("Sleep Timer")]
    [SerializeField] private PlayerTimers playerTimers;

    private void Reset()
    {
        startDuration = 10f;
    }

    public override void OnTimerEnd()
    {
        if (playerTimers == null)
        {
            playerTimers = GetComponentInParent<PlayerTimers>();
        }

        // Placeholder for future sleep effect logic.
    }
}
