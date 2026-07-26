using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central scene flow manager for start, gameplay, and end scenes.
/// Use this to keep scene transitions configurable and easy to change later.
/// </summary>
public class SceneFlowManager : MonoBehaviour
{
    public static SceneFlowManager Instance { get; private set; }

    [Header("Scene Configuration")]
    [Tooltip("Scene name used for the start menu.")]
    public string startSceneName = "Start Menu";

    [Tooltip("Scene name used for gameplay.")]
    public string gameplaySceneName = "MainScene";

    [Tooltip("Scene name used for the end game screen.")]
    public string endSceneName = "Credits";

    public static SceneFlowManager GetOrCreateInstance()
    {
        if (Instance != null)
            return Instance;

        SceneFlowManager[] managers = FindObjectsByType<SceneFlowManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (managers != null && managers.Length > 0)
        {
            for (int i = 0; i < managers.Length; i++)
            {
                if (managers[i] != null && managers[i].gameObject.scene.name == "DontDestroyOnLoad")
                {
                    Instance = managers[i];
                    return Instance;
                }
            }

            Instance = managers[0];
            return Instance;
        }

        GameObject managerObject = new GameObject("SceneFlowManager");
        Instance = managerObject.AddComponent<SceneFlowManager>();
        DontDestroyOnLoad(managerObject);
        return Instance;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            return;
        }

        if (Instance == this)
            return;

        if (Instance.gameObject.scene.name == "DontDestroyOnLoad")
        {
            Destroy(gameObject);
            return;
        }

        if (gameObject.scene.name == "DontDestroyOnLoad")
        {
            Destroy(Instance.gameObject);
            Instance = this;
            return;
        }

        Destroy(gameObject);
    }

    public void LoadStartScene()
    {
        LoadScene(startSceneName);
    }

    public void LoadGameplayScene()
    {
        LoadScene(gameplaySceneName);
    }

    public void LoadEndScene()
    {
        LoadScene(endSceneName);
    }

    public void RestartGameplayScene()
    {
        LoadScene(gameplaySceneName);
    }

    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("SceneFlowManager: scene name is empty.");
            return;
        }

        Debug.Log($"SceneFlowManager: loading scene '{sceneName}'...");
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }



    public void QuitGame()
    {
#if UNITY_EDITOR
        Debug.Log("SceneFlowManager: quitting play mode.");
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Debug.Log("SceneFlowManager: quitting application.");
        Application.Quit();
#endif
    }
}
