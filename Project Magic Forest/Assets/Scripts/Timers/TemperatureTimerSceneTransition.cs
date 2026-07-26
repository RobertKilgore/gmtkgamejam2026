using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class TemperatureTimerSceneTransition : MonoBehaviour
{
    [Header("Timer References")]
    [Tooltip("Optional reference to the player timers container. Used to resolve the temperature timer if not assigned directly.")]
    [SerializeField] private PlayerTimers playerTimers;

    [Tooltip("The temperature timer that triggers the scene transition when it expires.")]
    [SerializeField] private TemperatureTimer temperatureTimer;

    [Header("Scene Transition")]
    [Tooltip("The exact scene name to load when the temperature timer reaches zero. Leave empty to use SceneFlowManager's end scene.")]
    [SerializeField] private string sceneName = "";

    [Tooltip("If true, SceneFlowManager will be used to perform the load. If false, SceneManager.LoadScene will be used.")]
    [SerializeField] private bool useSceneFlowManager = true;

    private bool hasTransitioned;

    private void Awake()
    {
        if (temperatureTimer == null)
        {
            temperatureTimer = ResolveTemperatureTimer();
        }
    }

    private void Update()
    {
        if (hasTransitioned || temperatureTimer == null)
        {
            return;
        }

        if (temperatureTimer.HasExpired)
        {
            TransitionToScene();
        }
    }

    private TemperatureTimer ResolveTemperatureTimer()
    {
        if (playerTimers != null)
        {
            return playerTimers.TemperatureTimer;
        }

        TemperatureTimer timer = GetComponent<TemperatureTimer>();
        if (timer != null)
        {
            return timer;
        }

        timer = GetComponentInChildren<TemperatureTimer>(true);
        if (timer != null)
        {
            return timer;
        }

        return FindFirstObjectByType<TemperatureTimer>(FindObjectsInactive.Include);
    }

    private void TransitionToScene()
    {
        hasTransitioned = true;

        if (useSceneFlowManager)
        {
            SceneFlowManager sceneFlowManager = SceneFlowManager.Instance ?? FindFirstObjectByType<SceneFlowManager>(FindObjectsInactive.Include);
            if (sceneFlowManager != null)
            {
                if (!string.IsNullOrWhiteSpace(sceneName))
                {
                    sceneFlowManager.LoadScene(sceneName);
                    return;
                }

                sceneFlowManager.LoadEndScene();
                return;
            }

            Debug.LogWarning("[TemperatureTimerSceneTransition] SceneFlowManager not found. Falling back to SceneManager.LoadScene.");
        }

        if (!string.IsNullOrWhiteSpace(sceneName))
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            return;
        }

        Debug.LogWarning("[TemperatureTimerSceneTransition] No scene name assigned and SceneFlowManager is unavailable. Cannot transition to scene.");
    }
}
