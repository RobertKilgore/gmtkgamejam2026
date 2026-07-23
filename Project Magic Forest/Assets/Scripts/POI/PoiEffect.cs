using UnityEngine;

public abstract class PoiEffect : ScriptableObject
{
    public abstract bool Apply(PoiContext context);
}
