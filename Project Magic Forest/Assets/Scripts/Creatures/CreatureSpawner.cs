using UnityEngine;

public class CreatureSpawner : MonoBehaviour
{
    [Header("Spawner")]
    [SerializeField] private GameObject[] creaturePrefabs;
    [SerializeField] private float spawnInterval = 2.5f;
    [Range(0f, 100f)] [SerializeField] private float spawnChancePercent = 25f;
    [Range(0, 100)] [SerializeField] private int maxActiveCreatures = 5;
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private bool autoStartSpawning = true;

    private float spawnTimer;

    private void Start()
    {
        if (spawnOnStart)
        {
            SpawnCreature();
        }
    }

    private void Update()
    {
        if (!autoStartSpawning || creaturePrefabs == null || creaturePrefabs.Length == 0)
        {
            return;
        }

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            TrySpawnCreature();
        }
    }

    public void TrySpawnCreature()
    {
        if (GetActiveCreatureCount() >= maxActiveCreatures)
        {
            return;
        }

        if (Random.value * 100f > spawnChancePercent)
        {
            return;
        }

        SpawnCreature();
    }

    private int GetActiveCreatureCount()
    {
        return FindObjectsOfType<CameraCreatureMovement>().Length;
    }

    public void SpawnCreature()
    {
        if (creaturePrefabs == null || creaturePrefabs.Length == 0)
        {
            Debug.LogWarning("[CreatureSpawner] No creature prefabs assigned.");
            return;
        }

        GameObject selectedPrefab = creaturePrefabs[Random.Range(0, creaturePrefabs.Length)];
        if (selectedPrefab == null)
        {
            Debug.LogWarning("[CreatureSpawner] One of the creature prefabs is null.");
            return;
        }

        GameObject creature = Instantiate(selectedPrefab, Vector3.zero, Quaternion.identity);
        CameraCreatureMovement movement = creature.GetComponent<CameraCreatureMovement>();
        if (movement != null)
        {
            movement.InitializeFromCamera();
        }
        else
        {
            Debug.LogWarning("[CreatureSpawner] Prefab is missing CameraCreatureMovement.");
        }
    }
}
