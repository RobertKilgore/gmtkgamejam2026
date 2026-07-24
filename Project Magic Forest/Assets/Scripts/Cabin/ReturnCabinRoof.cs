
using UnityEngine;

public class ReturnCabinRoof : MonoBehaviour
{
    public GameObject roof;
    public GameObject outdoorDimmer;
    SpriteRenderer sr_roof;
    BoxCollider2D bc_roof;
  
    

    void OnTriggerEnter2D(Collider2D other)
    {
        
        sr_roof = roof.GetComponent<SpriteRenderer>();
       sr_roof.enabled = true;
       bc_roof = roof.GetComponent<BoxCollider2D>();
       bc_roof.enabled = true;
       outdoorDimmer.SetActive(false);
    }
}


