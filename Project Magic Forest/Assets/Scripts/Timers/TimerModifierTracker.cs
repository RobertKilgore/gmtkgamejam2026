using System;
using System.Collections.Generic;
using UnityEngine;

public class TimerModifierTracker : MonoBehaviour
{
    private struct TrackedModifier
    {
        public string TimerKey;
        public string ModifierId;
        public float Duration;
        public float ElapsedTime;
    }

    private List<TrackedModifier> trackedModifiers = new List<TrackedModifier>();
    private PlayerTimers playerTimers;

    public event Action<string, string> OnModifierRemoved;

    private void Awake()
    {
        playerTimers = GetComponent<PlayerTimers>();
        if (playerTimers == null)
        {
            playerTimers = GetComponentInChildren<PlayerTimers>(true);
        }
    }

    private void Update()
    {
        // Process tracked modifiers
        for (int i = trackedModifiers.Count - 1; i >= 0; i--)
        {
            TrackedModifier modifier = trackedModifiers[i];
            modifier.ElapsedTime += Time.deltaTime;

            if (modifier.ElapsedTime >= modifier.Duration)
            {
                RemoveTrackedModifier(i);
            }
            else
            {
                trackedModifiers[i] = modifier;
            }
        }
    }

    /// <summary>
    /// Add a modifier with automatic removal after a duration
    /// </summary>
    public void AddTemporaryModifier(string timerKey, string modifierId, float modifierValue, float duration)
    {
        if (playerTimers == null)
        {
            Debug.LogWarning("[TimerModifierTracker] PlayerTimers not found!");
            return;
        }

        if (string.IsNullOrEmpty(timerKey) || string.IsNullOrEmpty(modifierId))
        {
            Debug.LogWarning("[TimerModifierTracker] Timer key or modifier ID is empty!");
            return;
        }

        // Add the modifier
        playerTimers.AddModifierToTimer(timerKey, modifierId, modifierValue);

        // Track it for removal
        if (duration > 0f)
        {
            trackedModifiers.Add(new TrackedModifier
            {
                TimerKey = timerKey,
                ModifierId = modifierId,
                Duration = duration,
                ElapsedTime = 0f
            });

            Debug.Log($"[TimerModifierTracker] Added temporary modifier '{modifierId}' to timer '{timerKey}' for {duration}s");
        }
        else
        {
            Debug.Log($"[TimerModifierTracker] Added permanent modifier '{modifierId}' to timer '{timerKey}'");
        }
    }

    /// <summary>
    /// Manually remove a tracked modifier
    /// </summary>
    public void RemoveTrackedModifier(string timerKey, string modifierId)
    {
        for (int i = trackedModifiers.Count - 1; i >= 0; i--)
        {
            if (trackedModifiers[i].TimerKey == timerKey && trackedModifiers[i].ModifierId == modifierId)
            {
                RemoveTrackedModifier(i);
                return;
            }
        }
    }

    private void RemoveTrackedModifier(int index)
    {
        TrackedModifier modifier = trackedModifiers[index];
        trackedModifiers.RemoveAt(index);

        if (playerTimers != null)
        {
            playerTimers.RemoveModifierFromTimer(modifier.TimerKey, modifier.ModifierId);
            OnModifierRemoved?.Invoke(modifier.TimerKey, modifier.ModifierId);
            Debug.Log($"[TimerModifierTracker] Removed modifier '{modifier.ModifierId}' from timer '{modifier.TimerKey}'");
        }
    }

    public void ClearAllTrackedModifiers()
    {
        for (int i = trackedModifiers.Count - 1; i >= 0; i--)
        {
            RemoveTrackedModifier(i);
        }
    }
}
