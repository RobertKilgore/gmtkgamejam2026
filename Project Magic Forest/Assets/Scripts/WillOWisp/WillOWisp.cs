using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class WillOWisp : MonoBehaviour
{
    public enum BehaviorType
    {
        FollowPlayer,
        SpawnPoi
    }

    [Header("Behavior")]
    [SerializeField] private BehaviorType behavior = BehaviorType.FollowPlayer;
    [SerializeField] private GameObject specialPoiPrefab;
    [SerializeField] private float poiSpawnerSearchDistance = 20f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float acceleration = 8f;
    [SerializeField] private float minMoveSpeed = 1.5f;
    [SerializeField] private float maxMoveSpeed = 4f;
    [SerializeField] private float speedChangeDuration = 1.5f;
    [SerializeField] private float movementSlopAmplitude = 0.5f;
    [SerializeField] private float movementSlopFrequency = 0.75f;
    [SerializeField] private float collisionAvoidanceRadius = 0.8f;
    [SerializeField] private float avoidanceForce = 1.1f;

    [Header("Idle")]
    [SerializeField] private float idleRadius = 0.65f;
    [SerializeField] private float idleRotationSpeed = 90f;
    [SerializeField] private float idleMoveSpeed = 1.75f;
    [SerializeField] private float playerDetectionDistance = 8f;
    [SerializeField] private float cabinStopDistance = 1.2f;

    [Header("Animation")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] animationSprites = new Sprite[0];
    [SerializeField] private float animationFrameRate = 10f;

    [Header("Off-Camera")]
    [SerializeField] private float offCameraLifetime = 30f;
    [SerializeField] private float wakeDistanceFromPlayer = 10f;

    [Header("Despawn")]
    [SerializeField] private float finalTargetDespawnDistance = 1.2f;
    [SerializeField] private float fadeOutDuration = 2f;

    [Header("Debug")]
    [SerializeField] private bool drawTargetGizmos = true;
    [SerializeField] private Color targetGizmoColor = Color.cyan;

    private float animationTimer;
    private int animationFrameIndex;
    private bool isFadingOut;
    private float fadeTimer;
    private Vector3 finalTargetPosition;
    private bool hasFinalTarget;
    private Vector3 currentTarget;
    private Color originalColor;
    private Light2D wispLight;
    private float originalLightIntensity;
    private float currentMoveSpeed;
    private float targetMoveSpeed;
    private PoiSpawner selectedPoiSpawner;

    private bool isAwakeOnCamera = false;

    private enum WispState
    {
        Idle,
        MovingToTarget
    }

    private WispState currentState = WispState.Idle;
    private bool hasLoggedMovingToward;

    private static WillOWisp activeFollowWisp;
    private static WillOWisp activePoiWisp;

    private Transform playerTransform;
    private Transform cabinTransform;
    private Camera mainCamera;
    private Rigidbody2D playerRigidbody;
    private Vector3 idleCenter;
    private float idleAngle;
    private bool isIdling = true;
    private float offCameraTimer;
    private bool hasTriggeredSpecialSpawn;
    private Vector3 currentVelocity;
    private float slopNoiseSeed;

    public BehaviorType CurrentBehavior => behavior;

    private void Awake()
    {
        RegisterActiveWisp();
    }

    private void Start()
    {
        mainCamera = Camera.main;
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        playerRigidbody = playerTransform?.GetComponent<Rigidbody2D>();
        if (cabinTransform == null)
            cabinTransform = GameObject.FindWithTag("Cabin")?.transform;

        idleCenter = transform.position;

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        wispLight = GetComponentInChildren<Light2D>();
        if (wispLight != null)
            originalLightIntensity = wispLight.intensity;

        currentMoveSpeed = Mathf.Clamp(moveSpeed, minMoveSpeed, maxMoveSpeed);
        targetMoveSpeed = Random.Range(minMoveSpeed, maxMoveSpeed);
        slopNoiseSeed = Random.value * 1000f;
    }

    public void SetCabinTransform(Transform cabin)
    {
        if (cabin != null)
        {
            cabinTransform = cabin;
        }
    }

    public void SetFinalTarget(Vector3 targetPosition)
    {
        finalTargetPosition = targetPosition;
        hasFinalTarget = true;
    }

    private void OnDestroy()
    {
        ClearActiveWisp();
    }

    private void Update()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        UpdateSpeedChaos();

        bool offCamera = IsOffCamera();
        if (offCamera)
        {
            offCameraTimer += Time.deltaTime;
            if (offCameraTimer >= offCameraLifetime)
            {
                Destroy(gameObject);
                return;
            }
        }
        else
        {
            offCameraTimer = 0f;
        }

        if (offCamera && playerTransform != null && Vector3.Distance(transform.position, playerTransform.position) > wakeDistanceFromPlayer)
        {
            PerformIdleMotion();
            UpdateAnimation();
            return;
        }

        if (isFadingOut)
        {
            PerformIdleMotion();
            UpdateFadeAndDespawn();
            UpdateAnimation();
            return;
        }

        if (behavior == BehaviorType.SpawnPoi && hasFinalTarget)
        {
            currentTarget = finalTargetPosition;
            if (currentState != WispState.MovingToTarget)
                EnterMovingState();

            MoveToward(finalTargetPosition);
            UpdateFadeAndDespawn();
            UpdateAnimation();
            return;
        }

        if (behavior == BehaviorType.FollowPlayer)
        {
            UpdateFollowPlayerBehavior();
        }
        else
        {
            UpdateSpawnPoiBehavior();
        }

        UpdateFadeAndDespawn();
        UpdateAnimation();
    }

    private void UpdateFollowPlayerBehavior()
    {
        if (playerTransform == null)
        {
            EnterIdleState();
            PerformIdleMotion();
            return;
        }

        float playerDistance = Vector3.Distance(transform.position, playerTransform.position);
        

        if (currentState == WispState.Idle)
        {
            PerformIdleMotion();

            if (playerDistance <= playerDetectionDistance && cabinTransform != null)
            {
                SetFinalTarget(cabinTransform.position);
                currentTarget = finalTargetPosition;
                EnterMovingState();
            }

            return;
        }

        if (currentState == WispState.MovingToTarget && hasFinalTarget)
        {
            currentTarget = finalTargetPosition;
            MoveToward(finalTargetPosition);
        }
        else
        {
            EnterIdleState();
        }
    }

    private void UpdateSpawnPoiBehavior()
    {
        if (!hasTriggeredSpecialSpawn)
        {
            if (playerTransform != null)
            {
                float playerDistance = Vector3.Distance(transform.position, playerTransform.position);
                

                if (playerDistance <= playerDetectionDistance)
                {
                    hasTriggeredSpecialSpawn = true;
                    TrySpawnSpecialPoi();
                    if (hasFinalTarget)
                    {
                        EnterMovingState();
                        currentTarget = finalTargetPosition;
                        MoveToward(finalTargetPosition);
                    }
                    else
                    {
                        PerformIdleMotion();
                    }
                }
                else
                {
                    PerformIdleMotion();
                }
            }
            return;
        }

        if (hasFinalTarget)
        {
            currentTarget = finalTargetPosition;
            if (currentState != WispState.MovingToTarget)
                EnterMovingState();

            MoveToward(finalTargetPosition);
        }
        else
        {
            EnterIdleState();
        }
    }

    private Vector3 UpdateIdleMotion()
    {
        idleAngle += idleRotationSpeed * Time.deltaTime;
        if (idleAngle >= 360f)
            idleAngle -= 360f;

        float radians = idleAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f) * idleRadius;
        return idleCenter + offset;
    }

    private void PerformIdleMotion()
    {
        Vector3 idlePosition = UpdateIdleMotion();
        currentTarget = idlePosition;
        transform.position = Vector3.MoveTowards(transform.position, idlePosition, idleMoveSpeed * Time.deltaTime);
    }

    private void EnterIdleState()
    {
        if (currentState != WispState.Idle)
        {
            currentState = WispState.Idle;
            hasFinalTarget = false;
            isFadingOut = false;
            fadeTimer = 0f;
            idleCenter = transform.position;
            hasLoggedMovingToward = false;
        }
    }

    private void StartFadeIdle()
    {
        if (!isFadingOut)
        {
            isFadingOut = true;
            currentState = WispState.Idle;
            idleCenter = transform.position;
            idleAngle = 0f;
            hasLoggedMovingToward = false;
        }
    }

    private void EnterMovingState()
    {
        if (currentState != WispState.MovingToTarget)
        {
            currentState = WispState.MovingToTarget;
            hasLoggedMovingToward = true;
        }
    }

    private void MoveToward(Vector3 targetPosition)
    {
        if (currentState == WispState.MovingToTarget)
        {
            float targetDistance = Vector3.Distance(transform.position, targetPosition);

            Vector3 desiredVector = targetPosition - transform.position;
            if (desiredVector.sqrMagnitude < 0.0001f)
                return;

            if (isFadingOut)
            {
                PerformIdleMotion();
                return;
            }

            float noiseX = Mathf.PerlinNoise(Time.time * movementSlopFrequency + slopNoiseSeed, slopNoiseSeed) * 2f - 1f;
            float noiseY = Mathf.PerlinNoise(slopNoiseSeed, Time.time * movementSlopFrequency + slopNoiseSeed * 1.5f) * 2f - 1f;
            Vector3 slopOffset = new Vector3(noiseX, noiseY, 0f) * movementSlopAmplitude;
            Vector3 slopDirection = (desiredVector.normalized + slopOffset).normalized;

            currentVelocity = Vector3.Lerp(currentVelocity, slopDirection * currentMoveSpeed, acceleration * Time.deltaTime);
            transform.position += currentVelocity * Time.deltaTime;
            return;
        }

        Vector3 desiredDirection = (targetPosition - transform.position).normalized;
        Vector3 avoidance = GetAvoidanceDirection();
        Vector3 finalDirection = (desiredDirection + avoidance).normalized;

        if (finalDirection.sqrMagnitude < 0.0001f)
            finalDirection = desiredDirection;

        currentVelocity = Vector3.Lerp(currentVelocity, finalDirection * currentMoveSpeed, acceleration * Time.deltaTime);
        transform.position += currentVelocity * Time.deltaTime;
    }

    private void UpdateFadeAndDespawn()
    {
        if (spriteRenderer == null)
            return;

        if (!hasFinalTarget)
            return;

        float distance = Vector3.Distance(transform.position, finalTargetPosition);
        if (distance <= finalTargetDespawnDistance && !isFadingOut)
            StartFadeIdle();

        if (!isFadingOut)
            return;

        fadeTimer += Time.deltaTime;
        float alpha = Mathf.Lerp(originalColor.a, 0f, fadeTimer / Mathf.Max(0.01f, fadeOutDuration));
        Color currentColor = originalColor;
        currentColor.a = alpha;
        spriteRenderer.color = currentColor;

        if (wispLight != null)
        {
            wispLight.intensity = Mathf.Lerp(originalLightIntensity, 0f, fadeTimer / Mathf.Max(0.01f, fadeOutDuration));
        }

        if (alpha <= 0.01f)
            Destroy(gameObject);
    }

    private void UpdateAnimation()
    {
        if (spriteRenderer == null)
            return;

        if (animationSprites == null || animationSprites.Length == 0)
            return;

        animationTimer += Time.deltaTime;
        float frameDuration = 1f / Mathf.Max(1f, animationFrameRate);
        while (animationTimer >= frameDuration)
        {
            animationTimer -= frameDuration;
            animationFrameIndex = (animationFrameIndex + 1) % animationSprites.Length;
        }

        spriteRenderer.sprite = animationSprites[animationFrameIndex];
    }

    private Vector3 GetAvoidanceDirection()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, collisionAvoidanceRadius);
        Vector3 avoidance = Vector3.zero;

        foreach (Collider2D hit in hits)
        {
            if (hit == null || hit.gameObject == gameObject)
                continue;

            Vector3 delta = transform.position - hit.transform.position;
            if (delta.sqrMagnitude < 0.0001f)
            {
                delta = Random.insideUnitCircle;
            }

            avoidance += delta.normalized;
        }

        if (avoidance.sqrMagnitude > 0.0001f)
            avoidance = avoidance.normalized * avoidanceForce;

        return avoidance;
    }

    private void UpdateSpeedChaos()
    {
        if (Mathf.Approximately(currentMoveSpeed, targetMoveSpeed))
        {
            targetMoveSpeed = Random.Range(minMoveSpeed, maxMoveSpeed);
        }

        float speedDelta = (maxMoveSpeed - minMoveSpeed) / Mathf.Max(0.01f, speedChangeDuration);
        currentMoveSpeed = Mathf.MoveTowards(currentMoveSpeed, targetMoveSpeed, speedDelta * Time.deltaTime);
    }

    private bool IsOffCamera()
    {
        if (mainCamera == null)
            return false;

        Vector3 viewportPoint = mainCamera.WorldToViewportPoint(transform.position);
        return viewportPoint.x < 0f || viewportPoint.x > 1f || viewportPoint.y < 0f || viewportPoint.y > 1f || viewportPoint.z < 0f;
    }

    private void OnDrawGizmos()
    {
        if (!drawTargetGizmos)
            return;

        Gizmos.color = targetGizmoColor;
        Gizmos.DrawWireSphere(transform.position, 0.1f);
        Gizmos.DrawLine(transform.position, currentTarget);
        Gizmos.DrawWireSphere(currentTarget, 0.15f);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, playerDetectionDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, wakeDistanceFromPlayer);

        if (hasFinalTarget)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(finalTargetPosition, finalTargetDespawnDistance);
        }

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, poiSpawnerSearchDistance);
    }

    private void TrySpawnSpecialPoi()
    {
        if (specialPoiPrefab == null)
            return;

        PoiSpawner[] spawners = Object.FindObjectsByType<PoiSpawner>(FindObjectsSortMode.None);
        if (spawners == null || spawners.Length == 0)
            return;

        var nearby = new System.Collections.Generic.List<PoiSpawner>();
        for (int i = 0; i < spawners.Length; i++)
        {
            if (Vector3.Distance(transform.position, spawners[i].transform.position) <= poiSpawnerSearchDistance)
                nearby.Add(spawners[i]);
        }

        if (nearby.Count == 0)
            return;

        PoiSpawner selected = nearby[Random.Range(0, nearby.Count)];
        if (selected != null)
        {
            selectedPoiSpawner = selected;
            selected.SpawnPrefab(specialPoiPrefab);
            selected.SetSpawningEnabled(false);
            SetFinalTarget(selected.transform.position);
        }
    }

    private void RegisterActiveWisp()
    {
        if (behavior == BehaviorType.FollowPlayer)
        {
            if (activeFollowWisp != null && activeFollowWisp != this)
                Destroy(activeFollowWisp.gameObject);
            activeFollowWisp = this;
        }
        else
        {
            if (activePoiWisp != null && activePoiWisp != this)
                Destroy(activePoiWisp.gameObject);
            activePoiWisp = this;
        }
    }

    private void ClearActiveWisp()
    {
        if (behavior == BehaviorType.FollowPlayer && activeFollowWisp == this)
            activeFollowWisp = null;

        if (behavior == BehaviorType.SpawnPoi && activePoiWisp == this)
            activePoiWisp = null;
    }

    public static bool IsActive(BehaviorType behavior)
    {
        return behavior == BehaviorType.FollowPlayer ? activeFollowWisp != null : activePoiWisp != null;
    }
}
