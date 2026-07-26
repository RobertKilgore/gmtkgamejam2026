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

    public bool HasEntries => entries != null && entries.Count > 0;

    public GameObject GetRandomPrefab()
    {
        if (!HasEntries)
            return null;

        // Fallback: pick first non-null prefab
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].prefab != null)
                return entries[i].prefab;
        }
        return null;
    }

    public GameObject GetRandomPrefabByDistance(Vector3 cabinPosition, Vector3 spawnerPosition)
    {
        if (!HasEntries)
            return null;

        float distance = Vector3.Distance(cabinPosition, spawnerPosition);
        float normalizedDistance = Mathf.Clamp01((distance - minDistanceForRarity) / (maxDistanceForRarity - minDistanceForRarity));
        float rarityBias = rarityDistributionCurve.Evaluate(normalizedDistance);

        // Build a weighted list of entries based on rarity bias.
        List<(Entry entry, float weight)> weightedEntries = new List<(Entry, float)>();

        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].prefab == null)
                continue;

            float rarityWeight = GetWeightForRarity(entries[i].rarity, rarityBias);
            if (rarityWeight <= 0f)
                continue;

            weightedEntries.Add((entries[i], rarityWeight));
        }

        if (weightedEntries.Count == 0)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].prefab != null)
                    weightedEntries.Add((entries[i], 1f));
            }
        }

        float totalWeight = 0f;
        for (int i = 0; i < weightedEntries.Count; i++)
            totalWeight += weightedEntries[i].weight;

        if (totalWeight <= 0f)
            return null;

        float r = Random.value * totalWeight;
        float accum = 0f;
        for (int i = 0; i < weightedEntries.Count; i++)
        {
            accum += weightedEntries[i].weight;
            if (r <= accum)
                return weightedEntries[i].entry.prefab;
        }

        return weightedEntries[weightedEntries.Count - 1].entry.prefab;
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
