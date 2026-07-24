using UnityEngine;

public class HeatTimer : Timer
{
    [Header("Heat Timer")]
    [SerializeField] private PlayerTimers playerTimers;

    private void Reset()
    {
        startDuration = 10f;
    }

    public override void OnTimerStart()
    {
        base.OnTimerStart();

        if (playerTimers == null)
        {
            playerTimers = GetComponentInParent<PlayerTimers>();
        }

        // Placeholder for future heat start effect logic.
    }

    public override void OnTimerEnd()
    {
        if (playerTimers == null)
        {
            playerTimers = GetComponentInParent<PlayerTimers>();
        }

        // Placeholder for future heat end effect logic.
    }
}
