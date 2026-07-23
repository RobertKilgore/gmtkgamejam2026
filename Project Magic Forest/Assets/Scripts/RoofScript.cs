using UnityEngine;

public class RoofScript : MonoBehaviour
{
    public GameObject roof;
    public GameObject door;

    void OnTriggerEnter2D(Collider2D other)
    {
        roof.SetActive(false);
    }
}
