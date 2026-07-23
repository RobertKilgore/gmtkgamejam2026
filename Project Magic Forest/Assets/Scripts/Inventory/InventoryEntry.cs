using System;
using UnityEngine;

[Serializable]
public sealed class InventoryEntry
{
    [SerializeField] private ItemDefinition item;
    [SerializeField] private float amount;

    public InventoryEntry(ItemDefinition item, float amount)
    {
        this.item = item;
        this.amount = amount;
    }

    public ItemDefinition Item => item;
    public float Amount
    {
        get => amount;
        set => amount = value;
    }
}
