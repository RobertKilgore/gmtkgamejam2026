using UnityEngine;

public class playerMovement : MonoBehaviour
{
    public float playerSpeed = 5f;
    public Rigidbody2D rb;
    public SpriteRenderer spriteRenderer;

    [Header("Walk Animation")]
    [SerializeField] private Sprite[] walkUpSprites = new Sprite[4];
    [SerializeField] private Sprite[] walkDownSprites = new Sprite[4];
    [SerializeField] private Sprite[] walkLeftSprites = new Sprite[4];
    [SerializeField] private Sprite[] walkRightSprites = new Sprite[4];
    [SerializeField] private Sprite idleUpSprite;
    [SerializeField] private Sprite idleDownSprite;
    [SerializeField] private Sprite idleLeftSprite;
    [SerializeField] private Sprite idleRightSprite;
    [SerializeField] private float walkFrameRate = 10f;
    [SerializeField] private bool useAnimator = false;
    [SerializeField] private Animator animator;

    private float baseSpeed;
    private Vector2 lastDirection = Vector2.down;
    private float walkTimer;
    private int walkFrame;

    private void Awake()
    {
        baseSpeed = playerSpeed;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    private void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector2 inputDirection = new Vector2(horizontal, vertical);
        bool isMoving = inputDirection.sqrMagnitude > 0f;

        if (isMoving)
        {
            inputDirection.Normalize();
            lastDirection = inputDirection;
        }

        rb.linearVelocity = inputDirection * playerSpeed;
        UpdateAnimation(isMoving, inputDirection);
    }

    private void UpdateAnimation(bool isMoving, Vector2 inputDirection)
    {
        if (useAnimator && animator != null)
        {
            animator.SetBool("IsMoving", isMoving);
            animator.SetFloat("MoveX", lastDirection.x);
            animator.SetFloat("MoveY", lastDirection.y);
            return;
        }

        if (spriteRenderer == null)
        {
            return;
        }

        if (isMoving)
        {
            Sprite[] currentWalkSprites = GetCurrentWalkSprites();
            if (currentWalkSprites != null && currentWalkSprites.Length > 0)
            {
                walkTimer += Time.deltaTime;
                float frameDuration = 1f / Mathf.Max(1f, walkFrameRate);
                if (walkTimer >= frameDuration)
                {
                    walkTimer -= frameDuration;
                    walkFrame = (walkFrame + 1) % currentWalkSprites.Length;
                }

                spriteRenderer.sprite = currentWalkSprites[walkFrame];
                return;
            }
        }

        Sprite idleSprite = GetCurrentIdleSprite();
        if (idleSprite != null)
        {
            spriteRenderer.sprite = idleSprite;
            return;
        }

        UpdateSpriteFacing();
    }

    private void UpdateSpriteFacing()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        Sprite idleSprite = GetCurrentIdleSprite();
        if (idleSprite != null)
        {
            spriteRenderer.sprite = idleSprite;
            return;
        }

        if (Mathf.Abs(lastDirection.y) > Mathf.Abs(lastDirection.x))
        {
            if (lastDirection.y > 0f)
            {
                spriteRenderer.sprite = idleUpSprite;
            }
            else
            {
                spriteRenderer.sprite = idleDownSprite;
            }
        }
        else
        {
            if (lastDirection.x > 0f)
            {
                spriteRenderer.sprite = idleRightSprite;
            }
            else if (lastDirection.x < 0f)
            {
                spriteRenderer.sprite = idleLeftSprite;
            }
        }
    }

    private Sprite[] GetCurrentWalkSprites()
    {
        if (Mathf.Abs(lastDirection.y) > Mathf.Abs(lastDirection.x))
        {
            return lastDirection.y > 0f ? walkUpSprites : walkDownSprites;
        }

        return lastDirection.x > 0f ? walkRightSprites : walkLeftSprites;
    }

    private Sprite GetCurrentIdleSprite()
    {
        if (Mathf.Abs(lastDirection.y) > Mathf.Abs(lastDirection.x))
        {
            return lastDirection.y > 0f ? idleUpSprite : idleDownSprite;
        }

        return lastDirection.x > 0f ? idleRightSprite : idleLeftSprite;
    }

    public void MultiplySpeed(float multiplier)
    {
        playerSpeed = baseSpeed * multiplier;
    }

    public void ResetSpeed()
    {
        playerSpeed = baseSpeed;
    }
}
