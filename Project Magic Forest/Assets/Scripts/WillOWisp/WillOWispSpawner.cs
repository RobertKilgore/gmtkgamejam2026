using UnityEngine;

public class WillOWispSpawner : MonoBehaviour
{
    [Header("Wisp Prefabs")]
    [SerializeField] private GameObject followWispPrefab;
    [SerializeField] private GameObject spawnPoiWispPrefab;

    [Header("References")]
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject cabin;
    [SerializeField] private Camera mainCamera;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 15f;
    [SerializeField, Range(0f, 100f)] private float followWispSpawnChancePercent = 35f;
    [SerializeField, Range(0f, 100f)] private float poiWispSpawnChancePercent = 35f;
    [SerializeField] private float offscreenMargin = 0.2f;
    [SerializeField] private float spawnDepth = 10f;
    [SerializeField] private bool logDebug = true;

    private float nextSpawnTime;

    private void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Time.time < nextSpawnTime)
            return;

        nextSpawnTime = Time.time + spawnInterval;
        TrySpawnWillOWisp();
    }

    private void TrySpawnWillOWisp()
    {
        if (player == null || mainCamera == null || cabin == null)
            return;

        if (!WillOWisp.IsActive(WillOWisp.BehaviorType.FollowPlayer) && Random.value * 100f <= followWispSpawnChancePercent)
            SpawnWisp(WillOWisp.BehaviorType.FollowPlayer);

        if (!WillOWisp.IsActive(WillOWisp.BehaviorType.SpawnPoi) && Random.value * 100f <= poiWispSpawnChancePercent)
            SpawnWisp(WillOWisp.BehaviorType.SpawnPoi);
    }

    private void SpawnWisp(WillOWisp.BehaviorType behavior)
    {
        Vector3 spawnPosition = GetOffscreenSpawnPosition();
        if (spawnPosition == Vector3.zero)
            return;

        GameObject prefab = behavior == WillOWisp.BehaviorType.FollowPlayer ? followWispPrefab : spawnPoiWispPrefab;
        if (prefab == null)
            return;

        GameObject wispInstance = Instantiate(prefab, spawnPosition, Quaternion.identity);
        WillOWisp wisp = wispInstance.GetComponent<WillOWisp>();
        if (wisp != null && cabin != null)
        {
            wisp.SetCabinTransform(cabin.transform);
        }

        if (logDebug)
            Debug.Log($"[WillOWispSpawner] Spawned {behavior} will-o-wisp at {spawnPosition}");
    }

    private Vector3 GetOffscreenSpawnPosition()
    {
        if (mainCamera == null || player == null)
            return Vector3.zero;

        Vector3 playerPos = player.transform.position;
        Vector3 playerVelocity = Vector3.zero;
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
            playerVelocity = rb.linearVelocity;

        playerMovement movement = player.GetComponent<playerMovement>() ?? player.GetComponentInChildren<playerMovement>(true);
        Vector3 facingDirection = GetPlayerFacingDirection(playerVelocity, movement);
        Vector3 viewportPlayer = mainCamera.WorldToViewportPoint(playerPos);

        Vector2 direction = new Vector2(facingDirection.x, facingDirection.y);
        if (direction.sqrMagnitude < 0.01f)
            direction = new Vector2(1f, 0f);
        else
            direction.Normalize();

        Vector2 offscreenPoint = new Vector2(
            Mathf.Clamp(viewportPlayer.x + direction.x * (1f + offscreenMargin), -offscreenMargin, 1f + offscreenMargin),
            Mathf.Clamp(viewportPlayer.y + direction.y * (1f + offscreenMargin), -offscreenMargin, 1f + offscreenMargin)
        );

        return mainCamera.ViewportToWorldPoint(new Vector3(offscreenPoint.x, offscreenPoint.y, spawnDepth));
    }

    private Vector3 GetPlayerFacingDirection(Vector3 playerVelocity, playerMovement movement)
    {
        if (movement != null && movement.LastDirection.sqrMagnitude > 0.01f)
            return new Vector3(movement.LastDirection.x, movement.LastDirection.y, 0f).normalized;

        if (playerVelocity.sqrMagnitude > 0.01f)
            return playerVelocity.normalized;

        Vector3 facing = player.transform.right;
        if (facing.sqrMagnitude > 0.01f)
            return facing.normalized;

        return Vector3.right;
    }
}