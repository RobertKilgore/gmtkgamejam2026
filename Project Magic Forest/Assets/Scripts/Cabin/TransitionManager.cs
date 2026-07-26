using UnityEngine;

public class TransitionManager : MonoBehaviour
{
    public GameObject exitBlocker;
    public float checker;
    public CameraManager cameraManager;
    public MapResetManager resetManager;
    public bool transitionRunning;
    public bool transitionReady;
    BoxCollider2D _exitBlocker;
    public GameObject SnowIn;
    public GameObject SnowOut;

    private GameObject cabinCam;
    private GameObject playerCam;

    private void Start()
    {
        _exitBlocker = exitBlocker.GetComponent<BoxCollider2D>();
        _exitBlocker.enabled = false;
        checker = 1;

        if (cameraManager == null)
        {
            cameraManager = FindFirstObjectByType<CameraManager>(FindObjectsInactive.Include);
        }

        if (resetManager == null)
        {
            resetManager = FindFirstObjectByType<MapResetManager>(FindObjectsInactive.Include);
        }
    }

    void Update()
    {
        if (cameraManager == null)
        {
            return;
        }

        if (cabinCam == null)
        {
            cabinCam = cameraManager.GetCabinCamera();
        }

        if (playerCam == null)
        {
            playerCam = cameraManager.GetPlayerCamera();
        }

        if (cabinCam != null && cabinCam.activeSelf && !transitionRunning && transitionReady)
        {
            checker = 2;
            StartTransition();
        }

        if (playerCam != null && playerCam.activeSelf)
        {
            transitionReady = true;
        }
    }

    void StartTransition()
    {
        _exitBlocker.enabled = true;
        checker = 3;
        transitionRunning = true;
        transitionReady = false;
        SnowIn.SetActive(true);
        Invoke(nameof(WorldRandomizer), 2f);
    }

    void WorldRandomizer()
    {
        Debug.Log("Works");
        SnowOut.SetActive(true);
        SnowIn.SetActive(false);
        Invoke(nameof(TransitionEnd), 2f);
        OnTransition();
    }

    void TransitionEnd()
    {
        transitionRunning = false;
        _exitBlocker.enabled = false;
        SnowOut.SetActive(false);
    }

    void OnTransition()
    {
        if (resetManager != null)
        {
            resetManager.ResetMap();
        }
        else
        {
            Debug.LogWarning("[TransitionManager] Reset manager is not assigned.");
        }
    }
}