using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class Paused : MonoBehaviour
{
   public GameObject pauseMenu;
   public Slider musicControl;
   public Slider sfxControl;
   public AudioMixer audioMixer;

    // Update is called once per frame
   void Start()
    {
        musicControl.value = 50f;
        sfxControl.value = 50f;
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

    public void UpdateMusicVolume(float value)
    {
        audioMixer.SetFloat("MusicVolume", value);
    }

    public void UpdateSfxVolume(float value)
    {
        audioMixer.SetFloat("SfxVolume", value);
    }


    public void ResumeButton()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
    }
    public void MenuButton()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
    }   
}
