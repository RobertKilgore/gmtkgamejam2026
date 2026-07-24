using UnityEngine;

[CreateAssetMenu(menuName = "Magic Forest/POI/Effects/Add Timer Modifier")]
public sealed class AddTimerModifierEffect : PoiEffect
{
    [SerializeField] private string timerKey = "";
    [SerializeField] private string modifierId = "";
    [SerializeField] private float modifierValue = 0f;
    [SerializeField] private float duration = -1f; // -1 means permanent, >= 0 means remove after this many seconds

    public override bool Apply(PoiContext context)
    {
        if (context.PlayerTimers == null)
        {
            Debug.LogWarning("[AddTimerModifierEffect] PlayerTimers is null!");
            return false;
        }

        if (string.IsNullOrEmpty(timerKey) || string.IsNullOrEmpty(modifierId))
        {
            Debug.LogWarning("[AddTimerModifierEffect] Timer key or modifier ID is empty!");
            return false;
        }

        // If duration is set, use the tracker system
        if (duration >= 0f)
        {
            TimerModifierTracker tracker = context.Player?.GetComponent<TimerModifierTracker>();
            if (tracker == null && context.Player != null)
            {
                tracker = context.Player.GetComponentInChildren<TimerModifierTracker>(true);
            }

            if (tracker != null)
            {
                tracker.AddTemporaryModifier(timerKey, modifierId, modifierValue, duration);
                Debug.Log($"[AddTimerModifierEffect] Added temporary modifier '{modifierId}' (value: {modifierValue}) to timer '{timerKey}' for {duration}s");
                return true;
            }
            else
            {
                Debug.LogWarning("[AddTimerModifierEffect] TimerModifierTracker not found, applying permanent modifier instead!");
            }
        }

        // Fall back to permanent modifier
        context.PlayerTimers.AddModifierToTimer(timerKey, modifierId, modifierValue);
        Debug.Log($"[AddTimerModifierEffect] Added permanent modifier '{modifierId}' (value: {modifierValue}) to timer '{timerKey}'");
        return true;
    }
}
