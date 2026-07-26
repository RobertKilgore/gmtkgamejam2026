using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Paused : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject pauseBackground;
    public GameObject pauseMainPanel;
    public GameObject settingsPanel;
    public GameObject gameplayUI;
    public GameObject pauseRoot;
    public GameObject mainMenu;
    public Slider masterControl;
    public Slider musicControl;
    public Slider sfxControl;

    private bool isPaused;
    private float previousTimeScale = 1f;
    private playerMovement playerMovementController;
    private PlayerInteractor playerInteractor;
    private bool isDraggingVolumeSlider;
    private float pendingMasterVolume = 1f;
    private float pendingMusicVolume = 1f;
    private float pendingSfxVolume = 1f;
    private int frameCounter;

    private void Start()
    {
        CachePlayerControlComponents();
        ConnectVolumeSliders();
        RefreshSliderValues();
        Close();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Close();
            }
            else
            {
                OpenPauseMenu();
            }
        }

        if (!isPaused && !isDraggingVolumeSlider)
        {
            frameCounter++;
            if (frameCounter >= 10)
            {
                frameCounter = 0;
                RefreshSliderValues();
            }
        }
    }

    public void OpenPauseMenu()
    {
        previousTimeScale = Time.timeScale;
        isPaused = true;
        isDraggingVolumeSlider = false;
        Time.timeScale = 0f;
        RefreshSliderValues();
        SetPlayerControlsEnabled(false);
        if (gameplayUI != null)
        {
            gameplayUI.SetActive(false);
        }
        SetPauseMenuVisible(true);
        SetPauseBackgroundVisible(true);
        SetPausePanelVisible(true);
        SetSettingsPanelVisible(false);
    }

    public void ResumeButton()
    {
        Close();
    }

    public void ShowOptionsMenu()
    {
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(true);
        }

        if (pauseBackground != null)
        {
            pauseBackground.SetActive(true);
        }

        RefreshSliderValues();
        SetPausePanelVisible(false);
        SetSettingsPanelVisible(true);
    }

    public void Close()
    {
        isPaused = false;
        SetSettingsPanelVisible(false);
        SetPausePanelVisible(false);
        SetPauseBackgroundVisible(false);
        SetPauseMenuVisible(false);
        if (gameplayUI != null)
        {
            gameplayUI.SetActive(true);
        }
        SetPlayerControlsEnabled(true);
        Time.timeScale = previousTimeScale <= 0f ? 1f : previousTimeScale;
    }

    public void MenuButton()
    {
        Time.timeScale = 1f;
        if (mainMenu != null)
        {
            mainMenu.SetActive(false);
        }

        SceneManager.LoadScene("MainMenu");
    }

    public void UpdateMasterVolume(float value)
    {
        AudioManager audioManager = GetAudioManager();
        if (audioManager != null)
        {
            pendingMasterVolume = NormalizeVolume(value);
            if (isDraggingVolumeSlider)
            {
                audioManager.SetMasterVolume(pendingMasterVolume);
            }
            else
            {
                RefreshSliderValues();
            }
        }
    }

    public void UpdateMusicVolume(float value)
    {
        AudioManager audioManager = GetAudioManager();
        if (audioManager != null)
        {
            pendingMusicVolume = NormalizeVolume(value);
            if (isDraggingVolumeSlider)
            {
                audioManager.SetMusicVolume(pendingMusicVolume);
            }
            else
            {
                RefreshSliderValues();
            }
        }
    }

    public void UpdateSfxVolume(float value)
    {
        AudioManager audioManager = GetAudioManager();
        if (audioManager != null)
        {
            pendingSfxVolume = NormalizeVolume(value);
            if (isDraggingVolumeSlider)
            {
                audioManager.SetSfxVolume(pendingSfxVolume);
            }
            else
            {
                RefreshSliderValues();
            }
        }
    }

    private void ConnectVolumeSliders()
    {
        AttachVolumeSlider(masterControl, UpdateMasterVolume);
        AttachVolumeSlider(musicControl, UpdateMusicVolume);
        AttachVolumeSlider(sfxControl, UpdateSfxVolume);
    }

    private void AttachVolumeSlider(Slider slider, Action<float> valueChangedHandler)
    {
        if (slider == null)
        {
            return;
        }

        slider.onValueChanged.RemoveAllListeners();
        slider.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<float>(valueChangedHandler));

        EventTrigger trigger = slider.GetComponent<EventTrigger>() ?? slider.gameObject.AddComponent<EventTrigger>();
        trigger.triggers.Clear();

        EventTrigger.Entry pointerDown = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerDown
        };
        pointerDown.callback.AddListener(_ => isDraggingVolumeSlider = true);

        EventTrigger.Entry pointerUp = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerUp
        };
        pointerUp.callback.AddListener(_ =>
        {
            isDraggingVolumeSlider = false;
            ApplyPendingVolumeValues();
        });

        trigger.triggers.Add(pointerDown);
        trigger.triggers.Add(pointerUp);
    }

    private void ApplyPendingVolumeValues()
    {
        AudioManager audioManager = GetAudioManager();
        if (audioManager == null)
        {
            return;
        }

        audioManager.SetMasterVolume(pendingMasterVolume);
        audioManager.SetMusicVolume(pendingMusicVolume);
        audioManager.SetSfxVolume(pendingSfxVolume);

        if (masterControl != null)
        {
            masterControl.value = pendingMasterVolume;
        }

        if (musicControl != null)
        {
            musicControl.value = pendingMusicVolume;
        }

        if (sfxControl != null)
        {
            sfxControl.value = pendingSfxVolume;
        }
    }

    private void CachePlayerControlComponents()
    {
        if (playerMovementController == null)
        {
            playerMovementController = FindFirstObjectByType<playerMovement>();
        }

        if (playerInteractor == null)
        {
            playerInteractor = FindFirstObjectByType<PlayerInteractor>();
        }
    }

    private void SetPlayerControlsEnabled(bool enabled)
    {
        CachePlayerControlComponents();

        if (playerMovementController != null)
        {
            playerMovementController.enabled = enabled;
        }

        if (playerInteractor != null)
        {
            playerInteractor.enabled = enabled;
        }
    }

    private float NormalizeVolume(float value)
    {
        return Mathf.Clamp01(value);
    }

    private AudioManager GetAudioManager()
    {
        return AudioManager.Instance != null ? AudioManager.Instance : AudioManager.EnsureInstance();
    }

    private void SetPauseMenuVisible(bool visible)
    {
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(visible);
        }
    }

    private void SetPauseBackgroundVisible(bool visible)
    {
        if (pauseBackground != null)
        {
            pauseBackground.SetActive(visible);
        }
    }

    private void SetPausePanelVisible(bool visible)
    {
        if (pauseMainPanel != null)
        {
            pauseMainPanel.SetActive(visible);
        }
    }

    private void SetSettingsPanelVisible(bool visible)
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(visible);
        }
    }

    private void RefreshSliderValues()
    {
        AudioManager audioManager = GetAudioManager();
        if (audioManager == null)
        {
            return;
        }

        if (!isDraggingVolumeSlider && !isPaused)
        {
            if (masterControl != null)
            {
                masterControl.value = audioManager.GetMasterVolume();
                pendingMasterVolume = audioManager.GetMasterVolume();
            }

            if (musicControl != null)
            {
                musicControl.value = audioManager.GetMusicVolume();
                pendingMusicVolume = audioManager.GetMusicVolume();
            }

            if (sfxControl != null)
            {
                sfxControl.value = audioManager.GetSfxVolume();
                pendingSfxVolume = audioManager.GetSfxVolume();
            }
        }
    }
}
