using UnityEngine;

public class DiseaseTimer : Timer
{
    [Header("Disease Timer")]
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

        if (playerTimers != null && playerTimers.gameObject != null)
        {
            Destroy(playerTimers.gameObject);
        }
    }
}
