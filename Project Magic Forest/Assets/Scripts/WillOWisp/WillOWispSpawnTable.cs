using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WillOWispSpawnTable", menuName = "Magic Forest/Will O Wisp/Spawn Table")]
public class WillOWispSpawnTable : ScriptableObject
{
    public enum BehaviorType
    {
        FollowPlayer,
        SpawnPoi
    }

    public enum RarityLevel
    {
        Common,
        Uncommon,
        Rare,
        VeryRare
    }

    [System.Serializable]
    public struct Entry
    {
        public GameObject prefab;
        public BehaviorType behavior;
        public RarityLevel rarity;
    }

    [SerializeField] private List<Entry> entries = new List<Entry>();

    [Header("Distance-to-Rarity")]
    [SerializeField] private float minDistanceForRarity = 5f;
    [SerializeField] private float maxDistanceForRarity = 100f;
    [SerializeField] private AnimationCurve rarityBiasCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    [SerializeField, Range(0.01f, 1f)] private float minimumRarityWeight = 0.05f;

    public bool HasEntries => entries != null && entries.Count > 0;

    public GameObject GetPrefab(BehaviorType behavior, Vector3 cabinPosition, Vector3 spawnPosition)
    {
        if (!HasEntries)
            return null;

        float distance = Vector3.Distance(cabinPosition, spawnPosition);
        float normalizedDistance = Mathf.Clamp01((distance - minDistanceForRarity) / Mathf.Max(0.0001f, maxDistanceForRarity - minDistanceForRarity));
        float bias = rarityBiasCurve.Evaluate(normalizedDistance);

        List<(Entry entry, float weight)> weightedEntries = new List<(Entry, float)>();
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].prefab == null || entries[i].behavior != behavior)
                continue;

            float weight = GetWeightForRarity(entries[i].rarity, bias);
            if (weight > 0f)
                weightedEntries.Add((entries[i], weight));
        }

        if (weightedEntries.Count == 0)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].prefab != null && entries[i].behavior == behavior)
                    weightedEntries.Add((entries[i], 1f));
            }
        }

        if (weightedEntries.Count == 0)
            return null;

        float total = 0f;
        for (int i = 0; i < weightedEntries.Count; i++)
            total += weightedEntries[i].weight;

        if (total <= 0f)
            return weightedEntries[Random.Range(0, weightedEntries.Count)].entry.prefab;

        float roll = Random.value * total;
        float accumulator = 0f;
        for (int i = 0; i < weightedEntries.Count; i++)
        {
            accumulator += weightedEntries[i].weight;
            if (roll <= accumulator)
                return weightedEntries[i].entry.prefab;
        }

        return weightedEntries[weightedEntries.Count - 1].entry.prefab;
    }

    private float GetWeightForRarity(RarityLevel rarity, float bias)
    {
        float center;
        switch (rarity)
        {
            case RarityLevel.Uncommon:
                center = 0.33f;
                break;
            case RarityLevel.Rare:
                center = 0.66f;
                break;
            case RarityLevel.VeryRare:
                center = 1f;
                break;
            default:
                center = 0f;
                break;
        }

        float distanceFromCenter = Mathf.Abs(bias - center);
        float baseWeight = 1f - distanceFromCenter;
        float weight = Mathf.Clamp01(baseWeight);
        return Mathf.Lerp(minimumRarityWeight, 1f, weight);
    }
}
