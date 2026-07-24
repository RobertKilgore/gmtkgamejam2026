using UnityEngine;

[CreateAssetMenu(menuName = "Magic Forest/POI/Effects/Spawn Modifier Zone")]
public sealed class SpawnModifierZoneEffect : PoiEffect
{
    [SerializeField] private string modifierZoneName = "ModifierZone";

    public override bool Apply(PoiContext context)
    {
        if (context.PoiBehaviour == null)
        {
            Debug.LogWarning("[SpawnModifierZoneEffect] PoiBehaviour not found in context!");
            return false;
        }

        // Look for TimerModifierArea as a child with the specified name
        Transform zoneTransform = context.PoiBehaviour.transform.Find(modifierZoneName);
        if (zoneTransform == null)
        {
            Debug.LogWarning($"[SpawnModifierZoneEffect] Modifier zone '{modifierZoneName}' not found as child of POI!");
            return false;
        }

        TimerModifierArea modifierArea = zoneTransform.GetComponent<TimerModifierArea>();
        if (modifierArea == null)
        {
            Debug.LogWarning($"[SpawnModifierZoneEffect] TimerModifierArea component not found on '{modifierZoneName}'!");
            return false;
        }

        // Activate the zone
        modifierArea.SetAreaActive(true);
        zoneTransform.gameObject.SetActive(true);
        
        Debug.Log($"[SpawnModifierZoneEffect] Activated modifier zone '{modifierZoneName}'");
        return true;
    }
}
