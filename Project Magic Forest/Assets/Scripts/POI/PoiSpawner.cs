using UnityEngine;

public class PoiSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] poiPrefabs;
    [SerializeField] private bool spawnOnAwake = false;

    private void Awake()
    {
        if (spawnOnAwake && poiPrefabs.Length > 0)
        {
            SpawnRandomPoi();
        }
    }

    public GameObject SpawnRandomPoi()
    {
        if (poiPrefabs.Length == 0)
        {
            Debug.LogWarning("[PoiSpawner] No POI prefabs assigned!");
            return null;
        }

        GameObject randomPrefab = poiPrefabs[Random.Range(0, poiPrefabs.Length)];
        GameObject spawnedPoi = Instantiate(randomPrefab, transform.position, transform.rotation, transform.parent);
        
        Debug.Log($"[PoiSpawner] Spawned POI: {spawnedPoi.name}");
        return spawnedPoi;
    }

    public GameObject SpawnRandomPoiAtPosition(Vector3 position)
    {
        if (poiPrefabs.Length == 0)
        {
            Debug.LogWarning("[PoiSpawner] No POI prefabs assigned!");
            return null;
        }

        GameObject randomPrefab = poiPrefabs[Random.Range(0, poiPrefabs.Length)];
        GameObject spawnedPoi = Instantiate(randomPrefab, position, transform.rotation, transform.parent);
        
        Debug.Log($"[PoiSpawner] Spawned POI at {position}: {spawnedPoi.name}");
        return spawnedPoi;
    }
}
