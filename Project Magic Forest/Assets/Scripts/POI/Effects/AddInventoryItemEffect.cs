using UnityEngine;

[CreateAssetMenu(menuName = "Magic Forest/POI/Effects/Add Inventory Item")]
public sealed class AddInventoryItemEffect : PoiEffect
{
    [SerializeField] private ItemDefinition item;
    [SerializeField] private float amount = 1f;

    public override bool Apply(PoiContext context)
    {
        return context.Inventory != null && context.Inventory.Add(item, amount);
    }
}
