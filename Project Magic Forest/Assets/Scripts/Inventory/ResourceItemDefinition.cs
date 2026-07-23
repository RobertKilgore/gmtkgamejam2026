using UnityEngine;

[CreateAssetMenu(menuName = "Magic Forest/Inventory/Resource Item")]
public sealed class ResourceItemDefinition : ItemDefinition
{
    [SerializeField] private string unitName = "units";
    [SerializeField] private float maximumStack = 1000f;

    public string UnitName => unitName;
    public override float MaximumStack => Mathf.Max(1f, maximumStack);
}
