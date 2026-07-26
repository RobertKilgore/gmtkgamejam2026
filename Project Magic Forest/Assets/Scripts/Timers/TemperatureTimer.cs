using UnityEngine;

public class TemperatureTimer : Timer
{
    [Header("Temperature Timer")]
    [SerializeField] private GameObject playerToKill;

    public override void OnTimerEnd()
    {
        SceneFlowManager sceneFlowManager = SceneFlowManager.Instance ?? FindFirstObjectByType<SceneFlowManager>();
        if (sceneFlowManager != null)
        {
            sceneFlowManager.LoadStartScene();
            return;
        }

        Debug.LogWarning("[TemperatureTimer] SceneFlowManager not found. Could not load title screen.");
    }
}
