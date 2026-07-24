using UnityEngine;

public class playerMovement : MonoBehaviour
{
    public float playerSpeed = 5f;
    public Rigidbody2D rb;
    public SpriteRenderer spriteRenderer;

    [Header("Directional Sprites")]
    [SerializeField] private Sprite upSprite;
    [SerializeField] private Sprite downSprite;
    [SerializeField] private Sprite leftSprite;
    [SerializeField] private Sprite rightSprite;

    private float baseSpeed;
    private Vector2 lastDirection = Vector2.down;

    private void Awake()
    {
        baseSpeed = playerSpeed;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    private void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector2 inputDirection = new Vector2(horizontal, vertical);
        if (inputDirection.sqrMagnitude > 0f)
        {
            inputDirection.Normalize();
            lastDirection = inputDirection;
        }

        rb.linearVelocity = inputDirection * playerSpeed;
        UpdateSpriteFacing();
    }

    private void UpdateSpriteFacing()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        if (Mathf.Abs(lastDirection.y) > Mathf.Abs(lastDirection.x))
        {
            if (lastDirection.y > 0f)
            {
                spriteRenderer.sprite = upSprite ?? downSprite;
            }
            else
            {
                spriteRenderer.sprite = downSprite ?? upSprite;
            }
        }
        else
        {
            if (lastDirection.x > 0f)
            {
                spriteRenderer.sprite = rightSprite ?? leftSprite;
            }
            else if (lastDirection.x < 0f)
            {
                spriteRenderer.sprite = leftSprite ?? rightSprite;
            }
        }
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
