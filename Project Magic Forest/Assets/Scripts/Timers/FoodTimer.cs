using UnityEngine;

public class FoodTimer : Timer
{
    [Header("Food Timer")]
    [SerializeField] private PlayerTimers playerTimers;
    [SerializeField] private string targetTimerName = "Temperature";
    [SerializeField] private string modifierId = "food";
    [SerializeField] private float temperatureModifierPerSecond = -1f;

    private void Reset()
    {
        startDuration = 5f;
    }

    public override void OnTimerEnd()
    {
        if (playerTimers == null)
        {
            playerTimers = GetComponentInParent<PlayerTimers>();
        }

        if (playerTimers != null)
        {
            playerTimers.AddModifierToTimer(targetTimerName, modifierId, temperatureModifierPerSecond);
        }
    }
}
