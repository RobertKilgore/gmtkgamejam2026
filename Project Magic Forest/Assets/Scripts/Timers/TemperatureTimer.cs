using UnityEngine;

public class TemperatureTimer : Timer
{
    [Header("Temperature Timer")]
    [SerializeField] private GameObject playerToKill;

    public override void OnTimerEnd()
    {
        // GameObject target = playerToKill != null ? playerToKill : gameObject;

        // if (target != null)
        // {
        //     Destroy(target);
        // }
    }
}
