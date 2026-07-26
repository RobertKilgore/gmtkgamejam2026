using UnityEngine;
using System.Collections;

public class CameraCreatureMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float acceleration = 6f;
    [SerializeField] private float collisionCheckRadius = 0.8f;
    [SerializeField] private float avoidanceForce = 0.8f;
    [SerializeField] private bool useSpriteFacing = true;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnPadding = 0.2f;
    [SerializeField] private float targetPadding = 0.15f;
    [SerializeField] private float targetOffsetMin = 0.35f;
    [SerializeField] private float targetOffsetMax = 0.95f;

    [Header("Idle Animation")]
    [SerializeField] private Sprite[] idleSprites = new Sprite[0];
    [SerializeField] private float idleDuration = 2f;
    [SerializeField] private float idleFrameRate = 4f;

    [Header("Off-Camera")]
    [SerializeField] private float offCameraDestroyTime = 15f;

    private Camera mainCamera;
    private Vector3 targetPosition;
    private Vector3 currentVelocity;
    private bool hasInitialized;
    private SpriteRenderer spriteRenderer;
    private float offCameraTimer;
    private bool isIdling;
    private int currentPathPoint;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        if (!hasInitialized)
        {
            return;
        }

        if (mainCamera == null)
        {
            Destroy(gameObject);
            return;
        }

        // Check if on camera
        bool isOnCamera = IsVisibleInCamera();
        
        if (!isOnCamera)
        {
            offCameraTimer += Time.deltaTime;
            if (offCameraTimer >= offCameraDestroyTime)
            {
                Destroy(gameObject);
                return;
            }
        }
        else
        {
            offCameraTimer = 0f;
        }

        // Handle movement or idling
        if (!isIdling)
        {
            Vector3 desiredDirection = (targetPosition - transform.position).normalized;
            Vector3 avoidanceDirection = GetAvoidanceDirection();
            Vector3 finalDirection = (desiredDirection + avoidanceDirection).normalized;

            if (finalDirection.sqrMagnitude < 0.0001f)
            {
                finalDirection = desiredDirection;
            }

            currentVelocity = Vector3.Lerp(currentVelocity, finalDirection * moveSpeed, acceleration * Time.deltaTime);
            transform.position += currentVelocity * Time.deltaTime;

            if (useSpriteFacing)
            {
                UpdateFacing();
            }

            if (ReachedTarget())
            {
                // Start idle animation
                StartCoroutine(PlayIdleAndPickNewTarget());
            }
        }
    }

    private bool IsVisibleInCamera()
    {
        if (mainCamera == null)
        {
            return false;
        }

        Vector3 viewportPoint = mainCamera.WorldToViewportPoint(transform.position);
        return viewportPoint.x >= 0f && viewportPoint.x <= 1f && viewportPoint.y >= 0f && viewportPoint.y <= 1f;
    }

    private IEnumerator PlayIdleAndPickNewTarget()
    {
        isIdling = true;

        // Play idle animation
        if (idleSprites != null && idleSprites.Length > 0 && spriteRenderer != null)
        {
            float frameDuration = 1f / Mathf.Max(1f, idleFrameRate);
            float elapsedTime = 0f;

            while (elapsedTime < idleDuration)
            {
                int frameIndex = (int)((elapsedTime / idleDuration) * idleSprites.Length) % idleSprites.Length;
                if (idleSprites[frameIndex] != null)
                {
                    spriteRenderer.sprite = idleSprites[frameIndex];
                }
                yield return new WaitForSeconds(frameDuration);
                elapsedTime += frameDuration;
            }
        }
        else
        {
            yield return new WaitForSeconds(idleDuration);
        }

        // Pick a new target on screen
        if (mainCamera != null)
        {
            targetPosition = GetRandomOnScreenTarget(mainCamera);
        }

        isIdling = false;
    }

    private Vector3 GetRandomOnScreenTarget(Camera cam)
    {
        Vector2 viewportTarget = new Vector2(
            Random.Range(0.2f, 0.8f),
            Random.Range(0.2f, 0.8f)
        );
        return cam.ViewportToWorldPoint(new Vector3(viewportTarget.x, viewportTarget.y, 10f));
    }

    public void InitializeFromCamera()
    {
        if (hasInitialized)
        {
            return;
        }

        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("[CameraCreatureMovement] No main camera found.");
            return;
        }

        transform.position = GetOffscreenSpawnPosition(mainCamera);
        targetPosition = GetOppositeSideTarget(mainCamera, transform.position);
        hasInitialized = true;
    }

    public void SetTargetPosition(Vector3 newTarget)
    {
        targetPosition = newTarget;
        hasInitialized = true;
    }

    private bool ReachedTarget()
    {
        return Vector2.Distance(transform.position, targetPosition) < 0.1f;
    }

    private Vector3 GetAvoidanceDirection()
    {
        Collider2D[] overlaps = Physics2D.OverlapCircleAll(transform.position, collisionCheckRadius);
        Vector3 avoidance = Vector3.zero;

        foreach (Collider2D hit in overlaps)
        {
            if (hit == null || hit.gameObject == gameObject)
            {
                continue;
            }

            Vector3 delta = transform.position - hit.transform.position;
            if (delta.sqrMagnitude < 0.0001f)
            {
                delta = Random.insideUnitCircle;
            }

            avoidance += delta.normalized;
        }

        if (avoidance.sqrMagnitude > 0.0001f)
        {
            avoidance = avoidance.normalized * avoidanceForce;
        }

        return avoidance;
    }

    private Vector3 GetOffscreenSpawnPosition(Camera cam)
    {
        Vector2 viewportPoint = GetRandomEdgeViewportPoint();
        Vector3 worldPoint = cam.ViewportToWorldPoint(new Vector3(viewportPoint.x, viewportPoint.y, 10f));
        return worldPoint;
    }

    private Vector2 GetRandomEdgeViewportPoint()
    {
        int edge = Random.Range(0, 4);

        switch (edge)
        {
            case 0:
                return new Vector2(-spawnPadding, Random.Range(0f, 1f));
            case 1:
                return new Vector2(1f + spawnPadding, Random.Range(0f, 1f));
            case 2:
                return new Vector2(Random.Range(0f, 1f), -spawnPadding);
            default:
                return new Vector2(Random.Range(0f, 1f), 1f + spawnPadding);
        }
    }

    private Vector3 GetOppositeSideTarget(Camera cam, Vector3 spawnPosition)
    {
        Vector2 viewportSpawn = cam.WorldToViewportPoint(spawnPosition);
        Vector2 targetViewport = new Vector2(
            viewportSpawn.x < 0.5f ? Random.Range(1f + targetPadding, 1.25f) : Random.Range(-0.25f, -targetPadding),
            viewportSpawn.y < 0.5f ? Random.Range(1f + targetPadding, 1.25f) : Random.Range(-0.25f, -targetPadding)
        );

        if (viewportSpawn.x < 0.5f)
        {
            targetViewport.x = Random.Range(1f + targetPadding, 1.25f);
        }
        else
        {
            targetViewport.x = Random.Range(-0.25f, -targetPadding);
        }

        if (viewportSpawn.y < 0.5f)
        {
            targetViewport.y = Random.Range(1f + targetPadding, 1.25f);
        }
        else
        {
            targetViewport.y = Random.Range(-0.25f, -targetPadding);
        }

        return cam.ViewportToWorldPoint(new Vector3(targetViewport.x, targetViewport.y, 10f));
    }

    private void UpdateFacing()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        Vector2 direction = (Vector2)(targetPosition - transform.position);
        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        direction.Normalize();

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            spriteRenderer.flipX = direction.x < 0f;
        }
    }
}
