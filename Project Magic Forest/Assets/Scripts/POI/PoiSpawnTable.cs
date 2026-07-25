using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PoiSpawnTable", menuName = "POI/Poi Spawn Table")]
public class PoiSpawnTable : ScriptableObject
{
    [System.Serializable]
    public struct Entry
    {
        public GameObject prefab;
        public float weight;
    }

    public List<Entry> entries = new List<Entry>();

    public bool HasEntries => entries != null && entries.Count > 0;

    public GameObject GetRandomPrefab()
    {
        if (!HasEntries)
            return null;

        float total = 0f;
        for (int i = 0; i < entries.Count; i++)
        {
            total += Mathf.Max(0f, entries[i].weight);
        }

        if (total <= 0f)
        {
            // fallback: pick first non-null prefab
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].prefab != null)
                    return entries[i].prefab;
            }
            return null;
        }

        float r = Random.value * total;
        float accum = 0f;
        for (int i = 0; i < entries.Count; i++)
        {
            float w = Mathf.Max(0f, entries[i].weight);
            accum += w;
            if (r <= accum)
                return entries[i].prefab;
        }

        return entries[entries.Count - 1].prefab;
    }
}
