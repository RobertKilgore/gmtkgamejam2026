using UnityEngine;
using UnityEngine.SceneManagement;

public class BackgroundMusicPlayer : MonoBehaviour
{
    [Header("Music Tracks")]
    [SerializeField] private AudioClip titleTheme;
    [SerializeField] private AudioClip cabinColdTheme;
    [SerializeField] private AudioClip cabinWarmTheme;
    [SerializeField] private AudioClip creditsTheme;
    [SerializeField] private AudioClip backgroundThemeA;
    [SerializeField] private AudioClip backgroundThemeB;

    [Header("Behavior")]
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool loop = true;
    [SerializeField] private bool stopOnDisable = true;
    [SerializeField] private float transitionDuration = 0.75f;

    private CameraManager cameraManager;
    private StoveController stoveController;
    private AudioClip currentTrack;
    private AudioClip currentBackgroundTrack;
    private string lastSceneName;
    private bool lastInCabin;
    private bool lastHasFuel;

    private void Start()
    {
        ResolveReferences();

        if (playOnStart)
        {
            EvaluateAndPlayTrack();
        }
    }

    private void Update()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        bool inCabin = IsInCabin();
        bool hasFuel = HasFuel();

        if (lastSceneName == sceneName && lastInCabin == inCabin && lastHasFuel == hasFuel)
        {
            return;
        }

        lastSceneName = sceneName;
        lastInCabin = inCabin;
        lastHasFuel = hasFuel;

        EvaluateAndPlayTrack();
    }

    private void OnDisable()
    {
        if (stopOnDisable)
            AudioManager.StopMusic();
    }

    private void EvaluateAndPlayTrack()
    {
        AudioClip desiredTrack = ResolveTrackForCurrentState();
        if (desiredTrack == null)
        {
            return;
        }

        if (currentTrack == desiredTrack)
        {
            return;
        }

        currentTrack = desiredTrack;
        AudioManager.PlayMusic(desiredTrack, loop, transitionDuration);
    }

    private AudioClip ResolveTrackForCurrentState()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (IsTitleScene(sceneName))
        {
            return titleTheme;
        }

        if (IsCreditsScene(sceneName))
        {
            return creditsTheme;
        }

        if (IsInCabin())
        {
            return HasFuel() ? cabinWarmTheme : cabinColdTheme;
        }

        if (currentTrack != null && (currentTrack == backgroundThemeA || currentTrack == backgroundThemeB))
        {
            return currentTrack;
        }

        return PickBackgroundTrack();
    }

    private bool IsTitleScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            return false;
        }

        string normalized = sceneName.ToLowerInvariant();
        return normalized.Contains("title") || normalized.Contains("menu") || normalized.Contains("start");
    }

    private bool IsCreditsScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            return false;
        }

        string normalized = sceneName.ToLowerInvariant();
        return normalized.Contains("credit");
    }

    private bool IsInCabin()
    {
        ResolveReferences();
        return cameraManager != null && cameraManager.IsCabinCameraActive();
    }

    private bool HasFuel()
    {
        ResolveReferences();
        return stoveController != null && stoveController.HasFuel();
    }

    private AudioClip PickBackgroundTrack()
    {
        if (currentBackgroundTrack != null)
        {
            return currentBackgroundTrack;
        }

        AudioClip[] candidates = { backgroundThemeA, backgroundThemeB };
        System.Collections.Generic.List<AudioClip> availableTracks = new System.Collections.Generic.List<AudioClip>();

        foreach (AudioClip candidate in candidates)
        {
            if (candidate != null)
            {
                availableTracks.Add(candidate);
            }
        }

        if (availableTracks.Count == 0)
        {
            return null;
        }

        currentBackgroundTrack = availableTracks[Random.Range(0, availableTracks.Count)];
        return currentBackgroundTrack;
    }

    private void ResolveReferences()
    {
        if (cameraManager == null)
        {
            cameraManager = FindFirstObjectByType<CameraManager>();
        }

        if (stoveController == null)
        {
            stoveController = FindFirstObjectByType<StoveController>();
        }
    }
}
