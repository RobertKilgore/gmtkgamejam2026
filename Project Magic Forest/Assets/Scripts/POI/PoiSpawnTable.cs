using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PoiSpawnTable", menuName = "POI/Poi Spawn Table")]
public class PoiSpawnTable : ScriptableObject
{
    public enum RarityLevel
    {
        Common = 0,
        Uncommon = 1,
        Rare = 2,
        VeryRare = 3
    }

    [System.Serializable]
    public struct Entry
    {
        public GameObject prefab;
        public RarityLevel rarity;
    }

    [SerializeField] private List<Entry> entries = new List<Entry>();

    [Header("Distance-to-Rarity Curve")]
    [SerializeField] private float minDistanceForRarity = 5f;
    [SerializeField] private float maxDistanceForRarity = 100f;
    [SerializeField] private AnimationCurve rarityDistributionCurve = AnimationCurve.Linear(0, 0, 1, 1);

    public float MinDistanceForRarity => minDistanceForRarity;
    public float MaxDistanceForRarity => maxDistanceForRarity;

    [Header("Debug")]
    [SerializeField] private bool logDebug = true;

    public bool HasEntries => entries != null && entries.Count > 0;

    public GameObject GetRandomPrefab()
    {
        if (!HasEntries)
            return null;

        var validPrefabs = new List<GameObject>();
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].prefab != null)
                validPrefabs.Add(entries[i].prefab);
        }

        if (validPrefabs.Count == 0)
            return null;

        return validPrefabs[Random.Range(0, validPrefabs.Count)];
    }

    public GameObject GetRandomPrefabByDistance(Vector3 cabinPosition, Vector3 spawnerPosition)
    {
        if (!HasEntries)
            return null;

        float distance = Vector3.Distance(cabinPosition, spawnerPosition);
        float normalizedDistance = Mathf.Clamp01((distance - minDistanceForRarity) / (maxDistanceForRarity - minDistanceForRarity));
        float rarityBias = rarityDistributionCurve.Evaluate(normalizedDistance);

        var entriesByRarity = new Dictionary<RarityLevel, List<Entry>>();
        foreach (RarityLevel rarity in System.Enum.GetValues(typeof(RarityLevel)))
            entriesByRarity[rarity] = new List<Entry>();

        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].prefab == null)
                continue;

            entriesByRarity[entries[i].rarity].Add(entries[i]);
        }

        var categoryWeights = new Dictionary<RarityLevel, float>();
        float totalCategoryWeight = 0f;
        foreach (var kvp in entriesByRarity)
        {
            float categoryWeight = kvp.Value.Count > 0 ? GetWeightForRarity(kvp.Key, rarityBias) : 0f;
            categoryWeights[kvp.Key] = categoryWeight;
            totalCategoryWeight += categoryWeight;
        }

        if (totalCategoryWeight <= 0f)
        {
            // Fallback: pick any valid prefab from the full table
            if (logDebug)
                Debug.LogWarning("[PoiSpawnTable] No category weight available; falling back to random prefab from all entries.");

            return GetRandomPrefab();
        }

        if (logDebug)
        {
            string logLine = $"[PoiSpawnTable] Rarity bias={rarityBias:F2}, category chances:";
            foreach (var rarity in System.Enum.GetValues(typeof(RarityLevel)))
            {
                float chance = categoryWeights[(RarityLevel)rarity] / totalCategoryWeight * 100f;
                logLine += $" {(RarityLevel)rarity}={chance:F1}%";
            }
            Debug.Log(logLine);
        }

        float r = Random.value * totalCategoryWeight;
        float accum = 0f;
        RarityLevel selectedRarity = RarityLevel.Common;
        foreach (var kvp in categoryWeights)
        {
            accum += kvp.Value;
            if (r <= accum)
            {
                selectedRarity = kvp.Key;
                break;
            }
        }

        var selectedList = entriesByRarity[selectedRarity];
        if (selectedList == null || selectedList.Count == 0)
        {
            if (logDebug)
                Debug.LogWarning($"[PoiSpawnTable] Selected rarity {selectedRarity} has no entries; falling back to random prefab.");
            return GetRandomPrefab();
        }

        if (logDebug)
            Debug.Log($"[PoiSpawnTable] Selected rarity category: {selectedRarity} with {selectedList.Count} prefabs.");

        int selectedIndex = Random.Range(0, selectedList.Count);
        return selectedList[selectedIndex].prefab;
    }

    private float GetWeightForRarity(RarityLevel rarity, float bias)
    {
        float center;
        switch (rarity)
        {
            case RarityLevel.Common:
                center = 0f;
                break;
            case RarityLevel.Uncommon:
                center = 0.33f;
                break;
            case RarityLevel.Rare:
                center = 0.66f;
                break;
            default:
                center = 1f;
                break;
        }

        float distanceFromCenter = Mathf.Abs(bias - center);
        float weight = 1f - distanceFromCenter;
        weight = Mathf.Clamp01(weight);

        // Keep a nonzero minimum chance so rarer categories can still appear outside their ideal range.
        return Mathf.Lerp(0.05f, 1f, weight);
    }
}
