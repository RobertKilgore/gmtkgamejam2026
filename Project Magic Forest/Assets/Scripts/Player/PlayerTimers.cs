using System.Collections.Generic;
using UnityEngine;

public class PlayerTimers : MonoBehaviour
{
    [Header("Player Timers")]
    [SerializeField] private TemperatureTimer temperatureTimer;
    [SerializeField] private FoodTimer foodTimer;
    [SerializeField] private SleepTimer sleepTimer;
    [SerializeField] private HeatTimer heatTimer;
    [SerializeField] private InjuryTimer injuryTimer;
    [SerializeField] private DiseaseTimer diseaseTimer;

    private readonly List<Timer> trackedTimers = new List<Timer>();

    private void Awake()
    {
        if (temperatureTimer == null)
        {
            temperatureTimer = GetComponent<TemperatureTimer>();
        }

        if (foodTimer == null)
        {
            foodTimer = GetComponentInChildren<FoodTimer>(true);
        }

        if (sleepTimer == null)
        {
            sleepTimer = GetComponentInChildren<SleepTimer>(true);
        }

        if (heatTimer == null)
        {
            heatTimer = GetComponentInChildren<HeatTimer>(true);
        }

        if (injuryTimer == null)
        {
            injuryTimer = GetComponentInChildren<InjuryTimer>(true);
        }

        if (diseaseTimer == null)
        {
            diseaseTimer = GetComponentInChildren<DiseaseTimer>(true);
        }

        Timer[] timers = GetComponentsInChildren<Timer>(true);
        for (int i = 0; i < timers.Length; i++)
        {
            RegisterTimer(timers[i]);
        }
    }

    public void RegisterTimer(Timer timer)
    {
        if (timer == null || trackedTimers.Contains(timer))
        {
            return;
        }

        trackedTimers.Add(timer);

        if (temperatureTimer == null && timer is TemperatureTimer temperature)
        {
            temperatureTimer = temperature;
        }

        if (foodTimer == null && timer is FoodTimer food)
        {
            foodTimer = food;
        }

        if (sleepTimer == null && timer is SleepTimer sleep)
        {
            sleepTimer = sleep;
        }

        if (heatTimer == null && timer is HeatTimer heat)
        {
            heatTimer = heat;
        }

        if (injuryTimer == null && timer is InjuryTimer injury)
        {
            injuryTimer = injury;
        }
         
        if (diseaseTimer == null && timer is DiseaseTimer disease)
        {
            diseaseTimer = disease;
        }


    }

    public void AddModifierToTimer(string timerKey, string modifierId, float value)
    {
        Timer timer = FindTimer(timerKey);
        if (timer != null)
        {
            Debug.Log($"[PlayerTimers] Adding modifier '{modifierId}' with value {value} to timer '{timerKey}'.");
            timer.AddAdditiveModifier(modifierId, value);
        } else
        {
            Debug.LogWarning($"[PlayerTimers] Timer '{timerKey}' not found. Cannot add modifier '{modifierId}'.");
        }
    }

    public void RemoveModifierFromTimer(string timerKey, string modifierId)
    {
        Timer timer = FindTimer(timerKey);
        if (timer != null)
        {
            timer.RemoveAdditiveModifier(modifierId);
        }
    }

    public Timer FindTimer(string timerKey)
    {
        if (string.IsNullOrEmpty(timerKey))
        {
            return null;
        }

        for (int i = 0; i < trackedTimers.Count; i++)
        {
            Timer timer = trackedTimers[i];
            Debug.Log($"[PlayerTimers] Checking timer '{timer.name}' against key '{timerKey}'.");
            if (timer != null && timer.TimerKey == timerKey)
            {
                return timer;
            }
        }

        return null;
    }

    public TemperatureTimer TemperatureTimer => temperatureTimer;
    public FoodTimer FoodTimer => foodTimer;
    public SleepTimer SleepTimer => sleepTimer;
    public HeatTimer HeatTimer => heatTimer;
    public InjuryTimer InjuryTimer => injuryTimer;
    public DiseaseTimer DiseaseTimer => diseaseTimer;
    public IReadOnlyList<Timer> TrackedTimers => trackedTimers;
}
