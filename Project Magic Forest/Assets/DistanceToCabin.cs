using UnityEngine;

public class DistanceToCabin : MonoBehaviour
{
    [SerializeField] public GameObject player;
    [SerializeField] public GameObject cabin;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
  
    // Update is called once per frame
    void Update()
    {
        float distance = Vector3.Distance(player.transform.position, cabin.transform.position);
        Debug.Log("Distance to Cabin: " + distance);
    }
}
