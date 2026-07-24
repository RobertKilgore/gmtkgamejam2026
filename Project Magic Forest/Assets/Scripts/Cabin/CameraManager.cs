using UnityEngine;

public class CameraManager : MonoBehaviour

{
    public GameObject cabinCamera;
    public GameObject playerCamera;
    public GameObject roof;
     BoxCollider2D bc_roof;
     
  
   


    // Start is called once before the first execution of Update after the MonoBehaviour is created
   
    void Update()
    
    {
        bc_roof = roof.GetComponent<BoxCollider2D>();
        if(bc_roof.enabled == true)
        {
            PlayerCam();
        }
        else
        {
            CabinCam();
        }
        
    }
    void CabinCam()
    {
        cabinCamera.SetActive(true);
        playerCamera.SetActive(false);
    }

    // Update is called once per frame
    void PlayerCam()
    {
        playerCamera.SetActive(true);
        cabinCamera.SetActive(false);
    }
}
