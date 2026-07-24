using UnityEngine;

public abstract class ItemDefinition : ScriptableObject
{
    [SerializeField] private string itemId;
    [SerializeField] private string displayName;

    public string ItemId => itemId;
    public string DisplayName => displayName;
    public abstract float MaximumStack { get; }
}

[CreateAssetMenu(menuName = "Magic Forest/Inventory/Unique Item")]
public sealed class UniqueItemDefinition : ItemDefinition
{
    public override float MaximumStack => 1f;
}


[CreateAssetMenu(menuName = "Magic Forest/Inventory/Resource Item")]
public sealed class ResourceItemDefinition : ItemDefinition
{
    [SerializeField] private string unitName = "units";
    [SerializeField] private float maximumStack = 1000f;

    public string UnitName => unitName;
    public override float MaximumStack => Mathf.Max(1f, maximumStack);
}
