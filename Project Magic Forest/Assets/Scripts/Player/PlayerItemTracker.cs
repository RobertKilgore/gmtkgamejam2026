using UnityEngine;

[RequireComponent(typeof(PlayerInventory))]
public sealed class PlayerItemTracker : MonoBehaviour
{
    [Header("Item Definitions")]
    [SerializeField] private UniqueItemDefinition scarfItem;
    [SerializeField] private UniqueItemDefinition glovesItem;
    [SerializeField] private UniqueItemDefinition jacketItem;
    [SerializeField] private UniqueItemDefinition axeItem;

    [Header("Temperature Bonuses")]
    [SerializeField] private float scarfMaxTimeBonus = 30f;
    [SerializeField] private float glovesMaxTimeBonus = 20f;
    [SerializeField] private float jacketMaxTimeBonus = 40f;

    private PlayerInventory inventory;
    private PlayerTimers playerTimers;
    private TemperatureTimer temperatureTimer;
    private float baseTemperatureMaxTime;

    private void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
        if (inventory == null)
        {
            inventory = GetComponentInChildren<PlayerInventory>(true);
        }

        playerTimers = GetComponent<PlayerTimers>() ?? GetComponentInChildren<PlayerTimers>(true);

        if (inventory != null)
        {
            inventory.ItemAmountChanged += OnItemAmountChanged;
        }
    }

    private void Start()
    {
        ResolveTemperatureTimer();
        RefreshItemState();
    }

    private void ResolveTemperatureTimer()
    {
        if (playerTimers == null)
        {
            playerTimers = GetComponent<PlayerTimers>() ?? GetComponentInChildren<PlayerTimers>(true);
        }

        if (playerTimers != null)
        {
            temperatureTimer = playerTimers.TemperatureTimer;
            if (temperatureTimer == null)
            {
                temperatureTimer = playerTimers.GetComponentInChildren<TemperatureTimer>(true);
            }

            if (temperatureTimer == null)
            {
                temperatureTimer = playerTimers.FindTimer("Temperature") as TemperatureTimer;
            }
        }

        if (temperatureTimer != null)
        {
            baseTemperatureMaxTime = temperatureTimer.MaxTime;
            Debug.Log($"[PlayerItemTracker] Resolved TemperatureTimer '{temperatureTimer.name}' with base max time {baseTemperatureMaxTime}.");
        }
        else
        {
            Debug.LogWarning("[PlayerItemTracker] TemperatureTimer not found. Temperature bonus items will not apply.");
        }
    }

    private void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.ItemAmountChanged -= OnItemAmountChanged;
        }
    }

    private void OnItemAmountChanged(ItemDefinition item, float amount)
    {
        if (item == scarfItem || item == glovesItem || item == jacketItem || item == axeItem)
        {
            RefreshItemState();
        }
    }

    public void RefreshItemState()
    {
        UpdateTemperatureTimerMaxTime();
    }

    private void UpdateTemperatureTimerMaxTime()
    {
        if (temperatureTimer == null)
        {
            ResolveTemperatureTimer();
            if (temperatureTimer == null)
            {
                return;
            }
        }

        float targetMaxTime = baseTemperatureMaxTime;
        if (HasItem(scarfItem))
        {
            targetMaxTime += scarfMaxTimeBonus;
        }

        if (HasItem(glovesItem))
        {
            targetMaxTime += glovesMaxTimeBonus;
        }

        if (HasItem(jacketItem))
        {
            targetMaxTime += jacketMaxTimeBonus;
        }

        float previousMaxTime = temperatureTimer.MaxTime;
        float maxTimeDelta = targetMaxTime - previousMaxTime;

        temperatureTimer.SetMaxTime(targetMaxTime);

        if (maxTimeDelta > 0f)
        {
            temperatureTimer.AddTime(maxTimeDelta);
            Debug.Log($"[PlayerItemTracker] Added {maxTimeDelta} seconds to TemperatureTimer after max time increase.");
        }

        Debug.Log($"[PlayerItemTracker] Set TemperatureTimer max time to {targetMaxTime}. Current time: {temperatureTimer.TimeRemaining}.");
    }

    private bool HasItem(ItemDefinition item)
    {
        return item != null && inventory != null && inventory.Has(item, 1f);
    }

    public bool HasAxe => HasItem(axeItem);

    [ContextMenu("Give Scarf")]
    private void GiveScarf()
    {
        GrantItem(scarfItem);
    }

    [ContextMenu("Give Gloves")]
    private void GiveGloves()
    {
        GrantItem(glovesItem);
    }

    [ContextMenu("Give Jacket")]
    private void GiveJacket()
    {
        GrantItem(jacketItem);
    }

    [ContextMenu("Give Axe")]
    private void GiveAxe()
    {
        GrantItem(axeItem);
    }

    private void GrantItem(ItemDefinition item)
    {
        if (inventory == null)
        {
            Debug.LogWarning("[PlayerItemTracker] Inventory missing. Cannot grant item.");
            return;
        }

        if (item == null)
        {
            Debug.LogWarning("[PlayerItemTracker] Item definition is not assigned.");
            return;
        }

        inventory.Add(item, 1f);
    }
}