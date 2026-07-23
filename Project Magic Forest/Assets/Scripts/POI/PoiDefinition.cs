using System.Collections.Generic;
using UnityEngine;

public enum PoiActivationMode
{
    Manual,
    Proximity
}

[CreateAssetMenu(menuName = "Magic Forest/POI/Definition")]
public sealed class PoiDefinition : ScriptableObject
{
    [SerializeField] private string poiId;
    [SerializeField] private string displayName;
    [SerializeField] private PoiActivationMode activationMode = PoiActivationMode.Manual;
    [SerializeField] private bool oneShot = true;
    [SerializeField] private List<PoiEffect> effects = new();

    public string PoiId => poiId;
    public string DisplayName => displayName;
    public PoiActivationMode ActivationMode => activationMode;
    public bool OneShot => oneShot;
    public IReadOnlyList<PoiEffect> Effects => effects;
}
