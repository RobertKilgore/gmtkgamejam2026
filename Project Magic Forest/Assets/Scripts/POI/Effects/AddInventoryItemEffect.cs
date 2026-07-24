using UnityEngine;

[CreateAssetMenu(menuName = "Magic Forest/POI/Effects/Add Inventory Item")]
public sealed class AddInventoryItemEffect : PoiEffect
{
    [SerializeField] private ItemDefinition item;
    [SerializeField] private InventoryItemPoolDefinition itemPool;
    [SerializeField] private bool useRandomItem = false;
    [SerializeField] private float amount = 1f;

    public override bool Apply(PoiContext context)
    {
        if (context.Inventory == null)
        {
            return false;
        }

        ItemDefinition itemToAdd = useRandomItem ? GetRandomItem(context) : item;
        if (itemToAdd == null)
        {
            Debug.LogWarning("[AddInventoryItemEffect] No item selected or available in pool.");
            return false;
        }

        return context.Inventory.Add(itemToAdd, amount);
    }

    private ItemDefinition GetRandomItem(PoiContext context)
    {
        if (itemPool != null)
        {
            return itemPool.GetRandomItemNotOwned(context.Inventory);
        }

        if (item != null && (context.Inventory == null || !context.Inventory.Has(item, 1f)))
        {
            return item;
        }

        return null;
    }
}
