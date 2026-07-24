using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{
    [Header("Timer")]

    [SerializeField] protected string timerKey = "";
    [SerializeField] protected float timeRemaining;
    [SerializeField] protected float startDuration = 60f;
    [SerializeField] protected float maxTime = float.PositiveInfinity;
    [SerializeField] private bool startOnAwake = true;

    private bool timerIsRunning;
    private bool hasExpired;
    private readonly List<TimerModifier> additiveModifiers = new List<TimerModifier>();

    private struct TimerModifier
    {
        public string Id;
        public float Value;

        public TimerModifier(string id, float value)
        {
            Id = id;
            Value = value;
        }
    }


    private void Awake()
    {
        if (startOnAwake)
        {
            StartTimer(startDuration);
        }
    }

    private void Update()
    {
        if (timerIsRunning)
        {
            float deltaTime = Time.deltaTime;
            float speedMultiplier = GetEffectiveSpeedMultiplier();
            float depletion = deltaTime * speedMultiplier;
            //Debug.Log($"[Timer] {timerKey} - DeltaTime: {deltaTime}, SpeedMultiplier: {speedMultiplier}, Depletion: {depletion}, TimeRemaining: {timeRemaining}");
            timeRemaining = Mathf.Max(0f, timeRemaining - depletion);

            if (!float.IsInfinity(maxTime) && !float.IsNaN(maxTime))
            {
                timeRemaining = Mathf.Clamp(timeRemaining, 0f, maxTime);
            }

            if(hasExpired && timeRemaining > 0f)
            {
                hasExpired = false;
                OnTimerStart();
            }

            if(!hasExpired)
            {
                OnUpdate();
            }

            if (!hasExpired && timeRemaining <= 0f)
            {
                timeRemaining = 0f;
                hasExpired = true;
                OnTimerEnd();
            }
        }
    }


    public virtual void OnTimerEnd()
    {
    }

    public virtual void OnUpdate()
    {
    }

    public virtual void OnTimerStart()
    {
        timerIsRunning = true;
    }

    public virtual void StartTimer(float duration)
    {
        if (timeRemaining <= 0f)
        {
            timeRemaining = duration;
        }

        timerIsRunning = true;
    }

    public virtual void PauseTimer()
    {
        timerIsRunning = false;
    }

    public virtual void ResumeTimer()
    {
        timerIsRunning = true;
    }

    public virtual void SetTimer(float newTime)
    {
        timeRemaining = Mathf.Max(0f, newTime);
        hasExpired = timeRemaining <= 0f;
    }

    public void SetTimerKey(string newKey)
    {
        timerKey = newKey;
    }

    public bool HasExpired => hasExpired;
    public string TimerKey => timerKey;
    public float TimeRemaining => timeRemaining;
    public float MaxTime => maxTime;

    public virtual void AddTime(float amount)
    {
        timeRemaining = Mathf.Max(0f, timeRemaining + amount);
        if (timeRemaining > 0f && hasExpired)
        {
            hasExpired = false;
        }
    }

    public virtual void AddAdditiveModifier(string id, float value)
    {
        if (string.IsNullOrEmpty(id))
        {
            return;
        }

        RemoveAdditiveModifier(id);
        additiveModifiers.Add(new TimerModifier(id, value));
    }

    public virtual bool RemoveAdditiveModifier(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return false;
        }

        for (int i = additiveModifiers.Count - 1; i >= 0; i--)
        {
            if (additiveModifiers[i].Id == id)
            {
                additiveModifiers.RemoveAt(i);
                return true;
            }
        }

        return false;
    }

    public virtual void ClearModifiers()
    {
        additiveModifiers.Clear();
    }

    public virtual float GetEffectiveSpeedMultiplier()
    {
        float additiveTotal = 0f;
        for (int i = 0; i < additiveModifiers.Count; i++)
        {
            additiveTotal -= additiveModifiers[i].Value;
        }

        return 1f + additiveTotal;
    }
}
