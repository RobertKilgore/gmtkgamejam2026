using System.Collections.Generic;
using UnityEngine;

public class PoiSpawner : MonoBehaviour
{
    private static readonly List<PoiSpawner> registeredSpawners = new List<PoiSpawner>();

    public static IReadOnlyList<PoiSpawner> RegisteredSpawners => registeredSpawners;

    private void OnEnable()
    {
        if (!registeredSpawners.Contains(this))
            registeredSpawners.Add(this);
    }

    private void OnDisable()
    {
        registeredSpawners.Remove(this);
    }

    [Header("Spawn Table")]
    [SerializeField] private PoiSpawnTable spawnTable;

    [Header("Spawn Settings")]
    [SerializeField] private bool spawningEnabled = true;
    [SerializeField] private bool spawnOnAwake = false;
    [SerializeField] private bool disableAfterSpawn = true;
    [SerializeField] private bool logDebug = true;
    [SerializeField] private float resetSpawnLockDuration = 0.25f;

    [Header("Spawn Position")]
    [SerializeField] private bool useRendererOrColliderCenter = true;

    private bool hasSpawned = false;
    private float spawnLockUntilTime = -1f;

    private void Awake()
    {
        if (!spawningEnabled)
            return;

        if (spawnOnAwake && spawnTable != null && spawnTable.HasEntries)
        {
            SpawnRandomPoiAtPosition(GetSpawnPosition());
            if (disableAfterSpawn) hasSpawned = true;
        }
    }

    // Start() removed: proximity and player tracking are now handled by PlayerPoiSpawnManager.

    // Proximity checks are handled by the central PlayerPoiSpawnManager.

    private Vector3 GetSpawnPosition()
    {
        if (!useRendererOrColliderCenter)
            return transform.position;

        // try collider2D center
        Collider2D col2d = GetComponent<Collider2D>();
        if (col2d != null)
            return col2d.bounds.center;

        Collider col = GetComponent<Collider>();
        if (col != null)
            return col.bounds.center;

        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
            return rend.bounds.center;

        return transform.position;
    }

    public GameObject SpawnRandomPoi()
    {
        if (spawnTable == null || !spawnTable.HasEntries)
        {
            if (logDebug) Debug.LogWarning("[PoiSpawner] No POI prefabs assigned in the spawn table!");
            return null;
        }

        GameObject prefab = spawnTable.GetRandomPrefab();
        if (prefab == null)
        {
            Debug.LogWarning("[PoiSpawner] Spawn table returned null prefab.");
            return null;
        }

        GameObject spawnedPoi = Instantiate(prefab, GetSpawnPosition(), transform.rotation, transform.parent);
        if (logDebug) Debug.Log($"[PoiSpawner] Spawned POI: {spawnedPoi.name}");
        return spawnedPoi;
    }

    /// <summary>
    /// Attempt to spawn via manager or other systems. Respects spawningEnabled, hasSpawned and disableAfterSpawn.
    /// Returns true if a spawn occurred.
    /// </summary>
    public bool TrySpawn()
    {
        if (!spawningEnabled)
        {
            if (logDebug) Debug.Log("[PoiSpawner] TrySpawn blocked: spawning disabled.");
            return false;
        }

        if (Time.time < spawnLockUntilTime)
        {
            if (logDebug) Debug.Log("[PoiSpawner] TrySpawn blocked: spawner is still cooling down from reset.");
            return false;
        }

        if (hasSpawned && disableAfterSpawn)
        {
            if (logDebug) Debug.Log("[PoiSpawner] TrySpawn blocked: already spawned and disabled.");
            return false;
        }

        if (spawnTable == null || !spawnTable.HasEntries)
        {
            if (logDebug) Debug.LogWarning("[PoiSpawner] TrySpawn failed: spawn table empty.");
            return false;
        }

        SpawnRandomPoiAtPosition(GetSpawnPosition());
        if (disableAfterSpawn)
            hasSpawned = true;

        return true;
    }

    public GameObject SpawnRandomPoiAtPosition(Vector3 position)
    {
        if (spawnTable == null || !spawnTable.HasEntries)
        {
            if (logDebug) Debug.LogWarning("[PoiSpawner] No POI prefabs assigned in the spawn table!");
            return null;
        }

        GameObject prefab = spawnTable.GetRandomPrefab();
        if (prefab == null)
        {
            Debug.LogWarning("[PoiSpawner] Spawn table returned null prefab.");
            return null;
        }

        GameObject spawnedPoi = Instantiate(prefab, position, transform.rotation, transform.parent);
        if (logDebug) Debug.Log($"[PoiSpawner] Spawned POI at {position}: {spawnedPoi.name}");
        return spawnedPoi;
    }

    public void SetSpawningEnabled(bool enabled)
    {
        spawningEnabled = enabled;
    }

    public bool IsSpawningEnabled()
    {
        return spawningEnabled;
    }

    public void ResetSpawner()
    {
        spawningEnabled = true;
        hasSpawned = false;
        spawnLockUntilTime = Time.time + resetSpawnLockDuration;
    }

    [ContextMenu("Test Spawn")]
    private void TestSpawn()
    {
        if (!spawningEnabled)
        {
            if (logDebug) Debug.Log("[PoiSpawner] Spawning disabled.");
            return;
        }

        SpawnRandomPoiAtPosition(GetSpawnPosition());
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 pos = GetSpawnPosition();
        Gizmos.DrawWireCube(pos, Vector3.one * 0.5f);
    }
}
