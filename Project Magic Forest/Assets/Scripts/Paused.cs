using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class Paused : MonoBehaviour
{
   public GameObject pauseMenu;
   public GameObject mainMenu;
   public Slider musicControl;
   public Slider sfxControl;
   public AudioMixer audioMixer;

    // Update is called once per frame
   void Start()
    {
        AudioManager audioManager = AudioManager.Instance != null ? AudioManager.Instance : AudioManager.EnsureInstance();
        if (audioManager != null)
        {
            musicControl.value = audioManager.GetMusicVolume() * 100f;
            sfxControl.value = audioManager.GetSfxVolume() * 100f;
        }
        else
        {
            musicControl.value = 50f;
            sfxControl.value = 50f;
        }
    }
   
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            pauseMenu.SetActive(!pauseMenu.activeSelf);
            // Toggle pause state
            Time.timeScale = 0;
        }
    }

    private float NormalizeVolume(float value)
    {
        if (value > 1f)
        {
            return Mathf.Clamp01(value / 100f);
        }

        return Mathf.Clamp01(value);
    }

    private float ConvertLinearToDecibels(float linear)
    {
        if (linear <= 0f)
        {
            return -80f;
        }

        return Mathf.Log10(linear) * 20f;
    }

    public void UpdateMusicVolume(float value)
    {
        float normalized = NormalizeVolume(value);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(normalized);
        }

        if (audioMixer != null)
        {
            audioMixer.SetFloat("MusicVolume", ConvertLinearToDecibels(normalized));
        }
    }

    public void UpdateSfxVolume(float value)
    {
        float normalized = NormalizeVolume(value);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSfxVolume(normalized);
        }

        if (audioMixer != null)
        {
            audioMixer.SetFloat("SfxVolume", ConvertLinearToDecibels(normalized));
        }
    }


    public void ResumeButton()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
    }
    public void MenuButton()
    {
        mainMenu.SetActive(false);
        SceneManager.LoadScene("MainMenu");
        Time.timeScale = 1;
    }   
}
