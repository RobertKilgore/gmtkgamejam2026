using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Magic Forest/Inventory/Item Pool")]
public sealed class InventoryItemPoolDefinition : ScriptableObject
{
    [SerializeField] private List<ItemDefinition> possibleItems = new();

    public ItemDefinition GetRandomItem()
    {
        if (possibleItems == null || possibleItems.Count == 0)
        {
            return null;
        }

        List<ItemDefinition> validItems = new();
        for (int i = 0; i < possibleItems.Count; i++)
        {
            if (possibleItems[i] != null)
            {
                validItems.Add(possibleItems[i]);
            }
        }

        if (validItems.Count == 0)
        {
            return null;
        }

        return validItems[Random.Range(0, validItems.Count)];
    }

    public ItemDefinition GetRandomItemNotOwned(PlayerInventory inventory)
    {
        if (possibleItems == null || possibleItems.Count == 0)
        {
            return null;
        }

        List<ItemDefinition> availableItems = new();
        for (int i = 0; i < possibleItems.Count; i++)
        {
            ItemDefinition candidate = possibleItems[i];
            if (candidate == null)
            {
                continue;
            }

            if (inventory == null || !inventory.Has(candidate, 1f))
            {
                availableItems.Add(candidate);
            }
        }

        if (availableItems.Count == 0)
        {
            return null;
        }

        return availableItems[Random.Range(0, availableItems.Count)];
    }
}
