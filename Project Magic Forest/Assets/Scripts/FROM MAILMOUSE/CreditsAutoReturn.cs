using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class CreditsAutoReturn : MonoBehaviour
{
    [Header("Return Delay")]
    [Tooltip("Minutes to wait before returning to the main menu.")]
    [SerializeField] private int delayMinutes = 2;

    [Tooltip("Seconds to wait before returning to the main menu.")]
    [SerializeField] private int delaySeconds = 10;

    [Tooltip("Milliseconds to wait before returning to the main menu.")]
    [SerializeField] private int delayMilliseconds = 15;

    [Header("Scene Transition")]
    [Tooltip("Use SceneFlowManager if available. Otherwise fall back to SceneManager.")]
    [SerializeField] private bool useSceneFlowManager = true;

    [Tooltip("Optional explicit main menu scene name to load. Leave empty to use SceneFlowManager's start scene.")]
    [SerializeField] private string mainMenuSceneName = "Start Menu";

    private void Start()
    {
        float totalSeconds = (delayMinutes * 60f) + delaySeconds + (delayMilliseconds / 1000f);
        StartCoroutine(ReturnToMainMenuAfterDelay(totalSeconds));
    }

    private IEnumerator ReturnToMainMenuAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        if (useSceneFlowManager)
        {
            SceneFlowManager sceneFlowManager = SceneFlowManager.Instance ?? FindObjectOfType<SceneFlowManager>();
            if (sceneFlowManager != null)
            {
                if (!string.IsNullOrWhiteSpace(mainMenuSceneName))
                {
                    sceneFlowManager.LoadScene(mainMenuSceneName);
                    yield break;
                }

                sceneFlowManager.LoadStartScene();
                yield break;
            }

            Debug.LogWarning("[CreditsAutoReturn] SceneFlowManager not found. Falling back to SceneManager.LoadScene.");
        }

        if (!string.IsNullOrWhiteSpace(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);
            yield break;
        }

        Debug.LogWarning("[CreditsAutoReturn] No main menu scene name assigned. Cannot load main menu.");
    }
}
