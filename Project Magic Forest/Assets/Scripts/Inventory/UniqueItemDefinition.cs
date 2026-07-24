using UnityEngine;

[CreateAssetMenu(menuName = "Magic Forest/Inventory/Unique Item")]
public sealed class UniqueItemDefinition : ItemDefinition
{
    public override float MaximumStack => 1f;
}
