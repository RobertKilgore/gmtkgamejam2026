using UnityEngine;

public class RoofScript : MonoBehaviour
{
    SpriteRenderer sr_roof;
    BoxCollider2D bc_roof;
    public GameObject roof;
    public GameObject door;
    

    void OnTriggerEnter2D(Collider2D other)
    {
       sr_roof = roof.GetComponent<SpriteRenderer>();
       sr_roof.enabled = false;
       bc_roof = roof.GetComponent<BoxCollider2D>();
       bc_roof.enabled = false;
    }
}
