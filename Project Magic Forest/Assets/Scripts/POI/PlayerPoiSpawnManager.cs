using UnityEngine;

/// <summary>
/// Manager that queries nearby spawners around the player using a single physics call
/// and triggers their TrySpawn() method. Uses OverlapCircleNonAlloc to avoid allocations.
/// </summary>
public class PlayerPoiSpawnManager : MonoBehaviour
{
    public float checkInterval = 0.25f;
    public float checkRadius = 10f;
    public LayerMask spawnerLayer;
    public string playerTag = "Player";
    public bool autoDetectSpawners = true;
    public bool logDebug = true;

    private float nextCheck;
    private Transform player;
    private readonly Collider2D[] results = new Collider2D[128];

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag(playerTag);
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag(playerTag);
            if (p != null) player = p.transform; else return;
        }

        if (Time.time < nextCheck) return;
        nextCheck = Time.time + checkInterval;

        // Use the registered spawner list to avoid deprecated FindObjectsOfType and OverlapCircleNonAlloc.
        var spawners = PoiSpawner.RegisteredSpawners;
        if (logDebug) Debug.Log($"[PlayerPoiSpawnManager] Registered spawners count: {spawners.Count}");
        float sqr = checkRadius * checkRadius;
        for (int i = 0; i < spawners.Count; i++)
        {
            var s = spawners[i];
            if (s == null) continue;
            if (Vector3.SqrMagnitude(s.transform.position - player.position) <= sqr)
            {
                if (logDebug) Debug.Log($"[PlayerPoiSpawnManager] Spawner in range: {s.name}");
                s.TrySpawn();
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        if (player != null)
            Gizmos.DrawWireSphere(player.position, checkRadius);
    }
}
