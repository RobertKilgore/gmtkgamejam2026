using UnityEngine;

public class RoofScript : MonoBehaviour
{
    SpriteRenderer sr_roof;
    BoxCollider2D[] bc_roof;
    public GameObject roof;
    public GameObject door;
   

    void OnTriggerEnter2D(Collider2D other)
    {

       sr_roof = roof.GetComponent<SpriteRenderer>();
       sr_roof.enabled = false;
       bc_roof = roof.GetComponents<BoxCollider2D>();
       bc_roof[0].enabled = false;
       bc_roof = roof.GetComponents<BoxCollider2D>();
       bc_roof[1].enabled = false;
       bc_roof = roof.GetComponents<BoxCollider2D>();
       bc_roof[2].enabled = false;
       bc_roof = roof.GetComponents<BoxCollider2D>();
       bc_roof[3].enabled = false;
       Debug.Log("1");
       
    }
    public void OnAnimationEnd()
    {
        
    }
}
