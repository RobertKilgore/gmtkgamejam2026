using System;
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
    [SerializeField] private float duration = -1f; // -1 means infinite, >= 0 means deactivate after this many seconds
    [SerializeField] private bool startActive = true;

    private PlayerTimers playerTimers;
    private bool isActive;
    private float activeTimer = 0f;

    public event Action OnDeactivate;

    private void Awake()
    {
        isActive = startActive;
    }

    private void Update()
    {
        if (!isActive)
            return;

        // Check for duration-based deactivation
        if (duration >= 0f)
        {
            activeTimer += Time.deltaTime;
            if (activeTimer >= duration)
            {
                Deactivate();
                return;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!applyOnEnter || !isActive)
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
    }

    private void RemoveModifier()
    {
        if (playerTimers == null || string.IsNullOrEmpty(timerName) || string.IsNullOrEmpty(modifierId))
        {
            return;
        }

        playerTimers.RemoveModifierFromTimer(timerName, modifierId);
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

    /// <summary>
    /// Activate or deactivate the modifier area's internal logic.
    /// </summary>
    public void SetAreaActive(bool active)
    {
        if (isActive == active)
            return;

        isActive = active;
        activeTimer = 0f;

        if (!isActive)
        {
            RemoveModifier();
            playerTimers = null;
            return;
        }

        if (playerTimers != null)
        {
            ApplyModifier();
            return;
        }

        Collider2D areaCollider = GetComponent<Collider2D>();
        if (areaCollider == null)
        {
            return;
        }

        Collider2D[] overlaps = new Collider2D[8];
        ContactFilter2D contactFilter = new ContactFilter2D();
        int overlapCount = Physics2D.OverlapCollider(areaCollider, contactFilter, overlaps);

        for (int i = 0; i < overlapCount; i++)
        {
            if (TryGetPlayerTimers(overlaps[i], out PlayerTimers timers))
            {
                playerTimers = timers;
                ApplyModifier();
                break;
            }
        }
    }

    /// <summary>
    /// Deactivate the area and remove the modifier from the player if active
    /// </summary>
    public void Deactivate()
    {
        if (!isActive)
            return;

        SetAreaActive(false);
        OnDeactivate?.Invoke();
        Debug.Log($"[TimerModifierArea] Area deactivated. Removed modifier '{modifierId}'");
    }

    /// <summary>
    /// Reset the area for reuse (must be manually activated after)
    /// </summary>
    public void Reset()
    {
        activeTimer = 0f;
        if (playerTimers != null)
        {
            RemoveModifier();
            playerTimers = null;
        }
        isActive = false;
    }
}
