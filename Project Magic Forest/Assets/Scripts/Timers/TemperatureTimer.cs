using UnityEngine;
using UnityEngine.SceneManagement;

public class TemperatureTimer : Timer
{
    [Header("Temperature Timer")]
    [SerializeField] private GameObject playerToKill;
    public GameObject SnowIn;

    public override void OnTimerEnd()
    {
        SnowIn.SetActive(true);
        Invoke("LoadTitleScreen", 2f);

        

        Debug.LogWarning("[TemperatureTimer] SceneFlowManager not found. Could not load title screen.");
    }
    public void LoadTitleScreen()
    {
        SceneFlowManager sceneFlowManager = SceneFlowManager.Instance ?? FindFirstObjectByType<SceneFlowManager>();
        if (sceneFlowManager != null)
        {
            
           


            sceneFlowManager.LoadStartScene();
            return;
        }
    }
}
