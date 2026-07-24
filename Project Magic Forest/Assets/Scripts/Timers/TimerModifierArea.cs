using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TimerModifierArea : MonoBehaviour
{
    [Header("Modifier Area")]
    [SerializeField] private string timerName = "";
    [SerializeField] private string modifierId = "";
    [SerializeField] private float modifierValue = 0f;
    [SerializeField] private bool applyOnEnter = true;
    [SerializeField] private bool removeOnExit = true;

    private PlayerTimers playerTimers;
    private bool isActive;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!applyOnEnter)
        {
            return;
        }

        if (TryGetPlayerTimers(other, out PlayerTimers timers))
        {
            playerTimers = timers;
            ApplyModifier();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!removeOnExit)
        {
            return;
        }

        if (TryGetPlayerTimers(other, out PlayerTimers timers) && timers == playerTimers)
        {
            RemoveModifier();
            playerTimers = null;
        }
    }

    private void ApplyModifier()
    {
        if (playerTimers == null || string.IsNullOrEmpty(timerName) || string.IsNullOrEmpty(modifierId))
        {
            return;
        }

        playerTimers.AddModifierToTimer(timerName, modifierId, modifierValue);
        isActive = true;
    }

    private void RemoveModifier()
    {
        if (playerTimers == null || string.IsNullOrEmpty(timerName) || string.IsNullOrEmpty(modifierId))
        {
            return;
        }

        playerTimers.RemoveModifierFromTimer(timerName, modifierId);
        isActive = false;
    }

    private bool TryGetPlayerTimers(Collider2D other, out PlayerTimers timers)
    {
        timers = other.GetComponentInParent<PlayerTimers>();
        if (timers == null)
        {
            timers = other.GetComponent<PlayerTimers>();
        }

        return timers != null;
    }
}
