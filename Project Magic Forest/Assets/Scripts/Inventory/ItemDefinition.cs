using UnityEngine;

public abstract class ItemDefinition : ScriptableObject
{
    [SerializeField] private string itemId;
    [SerializeField] private string displayName;

    public string ItemId => itemId;
    public string DisplayName => displayName;
    public abstract float MaximumStack { get; }
}
