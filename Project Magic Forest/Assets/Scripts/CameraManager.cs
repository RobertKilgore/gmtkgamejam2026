using UnityEngine;

public class CameraManager : MonoBehaviour

{
    public GameObject cabinCamera;
    public GameObject playerCamera;
    public GameObject roof;
   


    // Start is called once before the first execution of Update after the MonoBehaviour is created
   
    void Update()
    
    {
        if(roof.activeSelf == true)
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
