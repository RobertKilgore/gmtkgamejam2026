using UnityEngine;

public class InjuryTimer : Timer
{
    [Header("Injury Timer")]
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

        // Placeholder for future injury start effect logic.
    }

    public override void OnTimerEnd()
    {
        if (playerTimers == null)
        {
            playerTimers = GetComponentInParent<PlayerTimers>();
        }

        // Placeholder for future injury end effect logic.
    }
}
