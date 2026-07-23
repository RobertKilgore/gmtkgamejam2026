using UnityEngine;

public class playerMovement : MonoBehaviour
{

    public float playerSpeed = 5;
    public Rigidbody2D rb;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        rb.linearVelocity = new Vector2(horizontal, vertical) * playerSpeed;
    }
}
