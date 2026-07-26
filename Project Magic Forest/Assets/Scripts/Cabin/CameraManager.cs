using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private GameObject cabinCamera;
    [SerializeField] private GameObject playerCamera;

    public GameObject GetCabinCamera()
    {
        return cabinCamera;
    }

    public GameObject GetPlayerCamera()
    {
        return playerCamera;
    }

    public bool IsCabinCameraActive()
    {
        return cabinCamera != null && cabinCamera.activeSelf;
    }

    public bool IsPlayerCameraActive()
    {
        return playerCamera != null && playerCamera.activeSelf;
    }

    public void ActivateCabinCamera()
    {
        if (cabinCamera != null)
            cabinCamera.SetActive(true);
        if (playerCamera != null)
            playerCamera.SetActive(false);
    }

    public void ActivatePlayerCamera()
    {
        if (playerCamera != null)
            playerCamera.SetActive(true);
        if (cabinCamera != null)
            cabinCamera.SetActive(false);
    }
}

