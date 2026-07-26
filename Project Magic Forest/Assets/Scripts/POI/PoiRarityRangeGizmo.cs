using UnityEngine;

[ExecuteAlways]
public class PoiRarityRangeGizmo : MonoBehaviour
{
    [Header("Source")]
    public PoiSpawnTable spawnTable;
    public bool useSpawnTableSettings = true;

    [Header("Manual Settings")]
    public float minRarityDistance = 5f;
    public float maxRarityDistance = 100f;

    [Header("Gizmo")]
    public Color minDistanceColor = Color.green;
    public Color maxDistanceColor = Color.yellow;
    public Color labelColor = Color.white;
    public bool drawLabels = true;

    private void OnDrawGizmos()
    {
        float minDistance = minRarityDistance;
        float maxDistance = maxRarityDistance;

        if (useSpawnTableSettings && spawnTable != null)
        {
            minDistance = spawnTable.MinDistanceForRarity;
            maxDistance = spawnTable.MaxDistanceForRarity;
        }

        if (minDistance < 0f) minDistance = 0f;
        if (maxDistance < minDistance) maxDistance = minDistance;

        Vector3 position = transform.position;

        Gizmos.color = minDistanceColor;
        Gizmos.DrawWireSphere(position, minDistance);

        Gizmos.color = maxDistanceColor;
        Gizmos.DrawWireSphere(position, maxDistance);

#if UNITY_EDITOR
        if (drawLabels)
        {
            UnityEditor.Handles.color = labelColor;
            UnityEditor.Handles.Label(position + Vector3.up * minDistance, $"Min rarity = {minDistance:F1}");
            UnityEditor.Handles.Label(position + Vector3.up * maxDistance, $"Max rarity = {maxDistance:F1}");
        }
#endif
    }
}
