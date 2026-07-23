using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class PlayerInventory : MonoBehaviour
{
    [SerializeField] private List<InventoryEntry> startingItems = new();
    private readonly List<InventoryEntry> items = new();

    public event Action<ItemDefinition, float> ItemAmountChanged;

    private void Awake()
    {
        foreach (InventoryEntry startingItem in startingItems)
        {
            if (startingItem.Item != null && startingItem.Amount > 0f)
            {
                Add(startingItem.Item, startingItem.Amount);
            }
        }
    }

    public float GetAmount(ItemDefinition item)
    {
        InventoryEntry entry = FindEntry(item);
        return entry == null ? 0f : entry.Amount;
    }

    public bool Has(ItemDefinition item, float amount = 1f)
    {
        return item != null && amount >= 0f && GetAmount(item) >= amount;
    }

    public bool Add(ItemDefinition item, float amount)
    {
        if (item == null || amount <= 0f)
        {
            return false;
        }

        InventoryEntry entry = FindEntry(item);
        if (entry == null)
        {
            entry = new InventoryEntry(item, 0f);
            items.Add(entry);
        }

        float previousAmount = entry.Amount;
        entry.Amount = Mathf.Min(previousAmount + amount, item.MaximumStack);
        float addedAmount = entry.Amount - previousAmount;

        if (addedAmount <= 0f)
        {
            return false;
        }

        Debug.Log($"[Inventory] Added {addedAmount} {item.DisplayName}. New amount: {entry.Amount}");
        ItemAmountChanged?.Invoke(item, entry.Amount);
        return true;
    }

    public bool Remove(ItemDefinition item, float amount)
    {
        if (item == null || amount <= 0f)
        {
            return false;
        }

        InventoryEntry entry = FindEntry(item);
        if (entry == null || entry.Amount < amount)
        {
            if (item is UniqueItemDefinition)
            {
                throw new System.InvalidOperationException(
                    $"Attempted to remove {amount} of unique item '{item.DisplayName}', but none were found in inventory.");
            }
            return false;
        }

        entry.Amount -= amount;
        if (entry.Amount <= 0f)
        {
            items.Remove(entry);
            Debug.Log($"[Inventory] Removed all {item.DisplayName}.");
        }
        else
        {
            Debug.Log($"[Inventory] Removed {amount} {item.DisplayName}. Remaining: {entry.Amount}");
        }

        ItemAmountChanged?.Invoke(item, entry.Amount);
        return true;
    }

    public bool TryConsume(ItemDefinition item, float amount)
    {
        return Remove(item, amount);
    }

    private InventoryEntry FindEntry(ItemDefinition item)
    {
        for (int index = 0; index < items.Count; index++)
        {
            if (items[index].Item == item)
            {
                return items[index];
            }
        }

        return null;
    }

}
