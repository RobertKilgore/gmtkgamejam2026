using UnityEngine;

public class playerMovement : MonoBehaviour

{

    public float playerSpeed = 5;
    public Rigidbody2D rb;

    private float baseSpeed;

    void Awake()
    {
        baseSpeed = playerSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        rb.linearVelocity = new Vector2(horizontal, vertical) * playerSpeed;
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
