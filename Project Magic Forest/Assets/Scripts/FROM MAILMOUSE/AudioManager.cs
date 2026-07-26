using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[DisallowMultipleComponent]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource secondaryMusicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Sound Library")]
    [SerializeField] private AudioClips audioClips;

    [Header("Playback Rules")]
    [Tooltip("If true, sounds will be prevented from overlapping when the same SFX channel is already busy.")]
    [SerializeField] private bool preventSfxOverlap = true;

    [Tooltip("How long a sound remains considered active before another same-category sound can play again.")]
    [SerializeField] private float overlapCooldown = 0.1f;

    [Header("Volumes")]
    [Range(0f, 1f)]
    [SerializeField] private float masterVolume = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;

    private readonly Dictionary<string, float> lastPlayedTimes = new Dictionary<string, float>();
    private readonly Dictionary<string, LoopingAudioSource> loopingAudioSources = new Dictionary<string, LoopingAudioSource>();
    private bool musicPausedByTimeScale;
    private AudioSource activeMusicSource;
    private AudioSource inactiveMusicSource;
    private Coroutine musicFadeRoutine;
    private AudioClip currentMusicClip;
    private float lastAppliedMasterVolume = -1f;
    private float lastAppliedMusicVolume = -1f;
    private float lastAppliedSfxVolume = -1f;
    private int volumeCheckFrameInterval = 10;
    private int volumeCheckFrameCounter;
    private const string VolumeSettingsFileName = "audio-settings.json";

    private class LoopingAudioSource
    {
        public AudioSource Source;
        public float VolumeScale;
    }

    [System.Serializable]
    private struct AudioVolumeSettings
    {
        public float masterVolume;
        public float musicVolume;
        public float sfxVolume;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[AudioManager] Another AudioManager already exists; destroying duplicate instance.", this);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        AudioSource[] existingSources = GetComponents<AudioSource>();

        if (musicSource == null)
            musicSource = existingSources.Length > 0 ? existingSources[0] : gameObject.AddComponent<AudioSource>();

        if (secondaryMusicSource == null)
        {
            if (existingSources.Length > 1)
                secondaryMusicSource = existingSources[1];
            else
                secondaryMusicSource = gameObject.AddComponent<AudioSource>();
        }

        if (sfxSource == null)
        {
            if (existingSources.Length > 2)
                sfxSource = existingSources[2];
            else
                sfxSource = gameObject.AddComponent<AudioSource>();
        }

        if (musicSource == sfxSource || musicSource == secondaryMusicSource)
            secondaryMusicSource = gameObject.AddComponent<AudioSource>();

        if (sfxSource == secondaryMusicSource)
            sfxSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.spatialBlend = 0f;
        musicSource.volume = 0f;

        secondaryMusicSource.loop = true;
        secondaryMusicSource.playOnAwake = false;
        secondaryMusicSource.spatialBlend = 0f;
        secondaryMusicSource.volume = 0f;

        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.spatialBlend = 0f;

        activeMusicSource = musicSource;
        inactiveMusicSource = secondaryMusicSource;

        LoadVolumes();
        ApplyVolumes();
    }

    public static AudioManager EnsureInstance()
    {
        if (Instance != null)
            return Instance;

        AudioManager existing = FindFirstObjectByType<AudioManager>(FindObjectsInactive.Include);
        if (existing != null)
        {
            Instance = existing;
            return existing;
        }

        GameObject audioManagerObject = new GameObject("AudioManager");
        return audioManagerObject.AddComponent<AudioManager>();
    }

    public static void PlaySFX(AudioClip clip, float volumeScale = 1f, float pitch = 1f, string channel = "default")
    {
        if (clip == null)
        {
            Debug.LogWarning("[AudioManager] PlaySFX called with a null clip.");
            return;
        }

        AudioManager manager = EnsureInstance();
        Debug.Log($"[AudioManager] Playing SFX '{clip.name}' via {manager.name}");
        manager.PlaySFXInternal(clip, volumeScale, pitch, channel);
    }

    public static void PlayInventoryOpenSound()
    {
        AudioManager manager = EnsureInstance();
        manager.PlayInventoryOpenSoundInternal();
    }

    public static void PlayInventoryCloseSound()
    {
        AudioManager manager = EnsureInstance();
        manager.PlayInventoryCloseSoundInternal();
    }

    public static void PlayPauseOpenSound()
    {
        AudioManager manager = EnsureInstance();
        manager.PlayPauseOpenSoundInternal();
    }

    public static void PlayPauseCloseSound()
    {
        AudioManager manager = EnsureInstance();
        manager.PlayPauseCloseSoundInternal();
    }

    public static void PlayUIButtonClickSound()
    {
        AudioManager manager = EnsureInstance();
        manager.PlayUIButtonClickSoundInternal();
    }

    public static void PlayPackageSound()
    {
        AudioManager manager = EnsureInstance();
        manager.PlayPackageSoundInternal();
    }

    public static void PlaySFXAtPoint(AudioClip clip, Vector3 position, float volumeScale = 1f)
    {
        if (clip == null)
            return;

        AudioSource.PlayClipAtPoint(clip, position, Mathf.Clamp01(Instance != null ? Instance.GetEffectiveSfxVolume(volumeScale) : volumeScale));
    }

    public static void PlayLoopingSfx(AudioClip clip, float volumeScale = 1f, float pitch = 1f, string channel = "default")
    {
        if (clip == null)
        {
            Debug.LogWarning("[AudioManager] PlayLoopingSfx called with a null clip.");
            return;
        }

        AudioManager manager = EnsureInstance();
        manager.PlayLoopingSfxInternal(clip, volumeScale, pitch, channel);
    }

    public static void StopLoopingSfx(string channel)
    {
        if (Instance == null || string.IsNullOrEmpty(channel))
            return;

        Instance.StopLoopingSfxInternal(channel);
    }

    public static void PlayMusic(AudioClip clip, bool loop = true, float fadeDuration = 0.5f)
    {
        if (clip == null)
        {
            Debug.LogWarning("[AudioManager] PlayMusic called with a null clip.");
            return;
        }

        AudioManager manager = EnsureInstance();
        Debug.Log($"[AudioManager] Playing music '{clip.name}' via {manager.name}");
        manager.PlayMusicInternal(clip, loop, fadeDuration);
    }

    public static void StopMusic()
    {
        if (Instance == null)
            return;

        Instance.StopMusicInternal();
    }

    private void Update()
    {
        volumeCheckFrameCounter++;
        if (volumeCheckFrameCounter >= volumeCheckFrameInterval)
        {
            volumeCheckFrameCounter = 0;
            if (ShouldReapplyVolumeState())
            {
                ApplyVolumes();
            }
        }

        if (activeMusicSource == null && inactiveMusicSource == null)
            return;

        if (Time.timeScale <= 0f)
        {
            if (activeMusicSource != null && activeMusicSource.isPlaying)
            {
                activeMusicSource.Pause();
            }

            if (inactiveMusicSource != null && inactiveMusicSource.isPlaying)
            {
                inactiveMusicSource.Pause();
            }

            musicPausedByTimeScale = true;
            return;
        }

        if (musicPausedByTimeScale)
        {
            if (activeMusicSource != null && activeMusicSource.clip != null && !activeMusicSource.isPlaying)
            {
                activeMusicSource.UnPause();
            }

            if (inactiveMusicSource != null && inactiveMusicSource.clip != null && !inactiveMusicSource.isPlaying)
            {
                inactiveMusicSource.UnPause();
            }

            musicPausedByTimeScale = false;
        }
    }

    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value);
        ApplyVolumes();
        SaveVolumes();
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        ApplyVolumes();
        SaveVolumes();
    }

    public void SetSfxVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        ApplyVolumes();
        SaveVolumes();
    }

    public float GetMasterVolume()
    {
        return masterVolume;
    }

    public float GetMusicVolume()
    {
        return musicVolume;
    }

    public float GetSfxVolume()
    {
        return sfxVolume;
    }

    public float GetEffectiveMusicVolume()
    {
        return Mathf.Clamp01(masterVolume * musicVolume);
    }

    public float GetEffectiveSfxVolume(float volumeScale = 1f)
    {
        return Mathf.Clamp01(masterVolume * sfxVolume * volumeScale);
    }

    private void ApplyMusicSourceVolume(AudioSource source, float volumeScale = 1f)
    {
        if (source == null)
            return;

        source.volume = Mathf.Clamp01(GetEffectiveMusicVolume() * volumeScale);
    }

    private void ApplySfxSourceVolume(float volumeScale = 1f)
    {
        if (sfxSource == null)
            return;

        sfxSource.volume = Mathf.Clamp01(GetEffectiveSfxVolume(volumeScale));
    }

    private void ApplyLoopingSourceVolume(AudioSource source, float volumeScale = 1f)
    {
        if (source == null)
            return;

        source.volume = Mathf.Clamp01(GetEffectiveSfxVolume(volumeScale));
    }

    private void PlayInventoryOpenSoundInternal()
    {
        if (audioClips != null && audioClips.getItemSound != null)
        {
            Debug.Log("[AudioManager] Playing get item sound.");
            PlaySFX(audioClips.getItemSound, channel: "inventory_ui");
            return;
        }

        Debug.LogWarning("[AudioManager] Get item sound clip is not assigned.");
    }

    private void PlayInventoryCloseSoundInternal()
    {
        if (audioClips != null && audioClips.getItemSound != null)
        {
            Debug.Log("[AudioManager] Playing get item sound.");
            PlaySFX(audioClips.getItemSound, channel: "inventory_ui");
            return;
        }

        Debug.LogWarning("[AudioManager] Get item sound clip is not assigned.");
    }

    private void PlayPauseOpenSoundInternal()
    {
        if (audioClips != null && audioClips.pauseMenuSound != null)
        {
            Debug.Log("[AudioManager] Playing pause menu sound.");
            PlaySFX(audioClips.pauseMenuSound, channel: "menu_ui");
            return;
        }

        Debug.LogWarning("[AudioManager] Pause menu sound clip is not assigned.");
    }

    private void PlayPauseCloseSoundInternal()
    {
        if (audioClips != null && audioClips.pauseMenuSound != null)
        {
            Debug.Log("[AudioManager] Playing pause menu sound.");
            PlaySFX(audioClips.pauseMenuSound, channel: "menu_ui");
            return;
        }

        Debug.LogWarning("[AudioManager] Pause menu sound clip is not assigned.");
    }

    private void PlayUIButtonClickSoundInternal()
    {
        if (audioClips != null && audioClips.uiClick != null)
        {
            Debug.Log("[AudioManager] Playing button click sound.");
            PlaySFX(audioClips.uiClick, channel: "menu_ui");
            return;
        }

        Debug.LogWarning("[AudioManager] UI click sound clip is not assigned.");
    }

    private void PlayPackageSoundInternal()
    {
        if (audioClips != null && audioClips.getItemSound != null)
        {
            Debug.Log("[AudioManager] Playing get item sound.");
            PlaySFX(audioClips.getItemSound, channel: "inventory_ui");
            return;
        }

        Debug.LogWarning("[AudioManager] Get item sound clip is not assigned.");
    }

    private void PlaySFXInternal(AudioClip clip, float volumeScale, float pitch, string channel)
    {
        if (sfxSource == null || clip == null)
        {
            Debug.LogWarning($"[AudioManager] Cannot play SFX '{clip?.name ?? "null"}' because the SFX AudioSource is missing.");
            return;
        }

        if (preventSfxOverlap && ShouldBlockPlayback(channel, clip.name))
        {
            Debug.Log($"[AudioManager] Blocked overlapping SFX '{clip.name}' on channel '{channel}'.");
            return;
        }

        float effectiveSfxVolume = GetEffectiveSfxVolume(volumeScale);
        sfxSource.pitch = pitch;
        ApplySfxSourceVolume(volumeScale);
        sfxSource.PlayOneShot(clip, effectiveSfxVolume);
        lastPlayedTimes[channel] = Time.unscaledTime;
        Debug.Log($"[AudioManager] SFX playback started: '{clip.name}', channel='{channel}', volume={effectiveSfxVolume:0.00}, pitch={pitch:0.00}");
    }

    private void PlayLoopingSfxInternal(AudioClip clip, float volumeScale, float pitch, string channel)
    {
        if (string.IsNullOrEmpty(channel))
        {
            channel = "default";
        }

        if (loopingAudioSources.TryGetValue(channel, out LoopingAudioSource existing))
        {
            if (existing.Source != null && existing.Source.clip == clip && existing.Source.isPlaying)
            {
                return;
            }

            if (existing.Source != null)
            {
                existing.Source.Stop();
                Destroy(existing.Source);
            }
        }

        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        newSource.playOnAwake = false;
        newSource.loop = true;
        newSource.spatialBlend = 0f;
        newSource.clip = clip;
        newSource.pitch = pitch;
        ApplyLoopingSourceVolume(newSource, volumeScale);
        newSource.Play();

        loopingAudioSources[channel] = new LoopingAudioSource
        {
            Source = newSource,
            VolumeScale = volumeScale
        };

        Debug.Log($"[AudioManager] Looping SFX started: '{clip.name}', channel='{channel}', volumeScale={volumeScale:0.00}, pitch={pitch:0.00}");
    }

    private void StopLoopingSfxInternal(string channel)
    {
        if (string.IsNullOrEmpty(channel))
        {
            return;
        }

        if (!loopingAudioSources.TryGetValue(channel, out LoopingAudioSource existing) || existing.Source == null)
        {
            return;
        }

        existing.Source.Stop();
        Destroy(existing.Source);
        loopingAudioSources.Remove(channel);

        Debug.Log($"[AudioManager] Looping SFX stopped: channel='{channel}'");
    }

    private void PlayMusicInternal(AudioClip clip, bool loop, float fadeDuration)
    {
        if (clip == null)
        {
            Debug.LogWarning($"[AudioManager] Cannot play music '{clip?.name ?? "null"}' because the music AudioSource is missing.");
            return;
        }

        if (currentMusicClip == clip && activeMusicSource != null && activeMusicSource.isPlaying)
        {
            return;
        }

        if (musicFadeRoutine != null)
        {
            StopCoroutine(musicFadeRoutine);
            musicFadeRoutine = null;
        }

        AudioSource targetSource = activeMusicSource == musicSource ? secondaryMusicSource : musicSource;
        if (targetSource == null)
        {
            targetSource = gameObject.AddComponent<AudioSource>();
            targetSource.loop = true;
            targetSource.playOnAwake = false;
            targetSource.spatialBlend = 0f;
            targetSource.volume = 0f;
        }

        targetSource.Stop();
        targetSource.clip = clip;
        targetSource.loop = loop;
        targetSource.volume = 0f;
        targetSource.Play();

        if (Time.timeScale <= 0f)
        {
            targetSource.Pause();
            musicPausedByTimeScale = true;
        }
        else
        {
            musicPausedByTimeScale = false;
        }

        currentMusicClip = clip;
        musicFadeRoutine = StartCoroutine(FadeBetweenMusicSources(activeMusicSource, targetSource, fadeDuration));

        Debug.Log($"[AudioManager] Music playback started: '{clip.name}', loop={loop}, fade={fadeDuration:F2}");
    }

    private IEnumerator FadeBetweenMusicSources(AudioSource oldSource, AudioSource newSource, float fadeDuration)
    {
        if (oldSource == null || newSource == null)
        {
            yield break;
        }

        float duration = Mathf.Max(fadeDuration, 0.01f);
        float elapsed = 0f;
        float startOldVolume = oldSource.volume;
        float startNewVolume = newSource.volume;

        while (elapsed < duration)
        {
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = Mathf.SmoothStep(0f, 1f, t);
            float targetVolume = GetEffectiveMusicVolume();

            if (oldSource != null)
            {
                oldSource.volume = Mathf.Lerp(startOldVolume, 0f, easedT);
            }

            if (newSource != null)
            {
                newSource.volume = Mathf.Lerp(startNewVolume, targetVolume, easedT);
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (oldSource != null)
        {
            oldSource.Stop();
            oldSource.volume = 0f;
        }

        if (newSource != null)
        {
            ApplyMusicSourceVolume(newSource);
        }

        activeMusicSource = newSource;
        inactiveMusicSource = oldSource;
        musicFadeRoutine = null;
    }

    private void StopMusicInternal()
    {
        if (musicFadeRoutine != null)
        {
            StopCoroutine(musicFadeRoutine);
            musicFadeRoutine = null;
        }

        if (musicSource != null)
        {
            musicSource.Stop();
            musicSource.volume = 0f;
        }

        if (secondaryMusicSource != null)
        {
            secondaryMusicSource.Stop();
            secondaryMusicSource.volume = 0f;
        }

        currentMusicClip = null;
        activeMusicSource = musicSource;
        inactiveMusicSource = secondaryMusicSource;
    }

    private bool ShouldBlockPlayback(string channel, string clipName)
    {
        if (string.IsNullOrEmpty(channel))
            return false;

        if (!lastPlayedTimes.TryGetValue(channel, out float lastTime))
            return false;

        return Time.unscaledTime - lastTime < overlapCooldown;
    }

    private void ApplyVolumes()
    {
        bool valuesChanged = Mathf.Abs(lastAppliedMasterVolume - masterVolume) > 0.0001f
            || Mathf.Abs(lastAppliedMusicVolume - musicVolume) > 0.0001f
            || Mathf.Abs(lastAppliedSfxVolume - sfxVolume) > 0.0001f;

        float effectiveMusicVolume = GetEffectiveMusicVolume();

        if (musicFadeRoutine == null)
        {
            if (musicSource != null)
            {
                musicSource.volume = ReferenceEquals(activeMusicSource, musicSource)
                    ? effectiveMusicVolume
                    : 0f;
            }

            if (secondaryMusicSource != null)
            {
                secondaryMusicSource.volume = ReferenceEquals(activeMusicSource, secondaryMusicSource)
                    ? effectiveMusicVolume
                    : 0f;
            }
        }

        ApplySfxSourceVolume();
        foreach (var loopingSource in loopingAudioSources.Values)
        {
            ApplyLoopingSourceVolume(loopingSource.Source, loopingSource.VolumeScale);
        }

        lastAppliedMasterVolume = masterVolume;
        lastAppliedMusicVolume = musicVolume;
        lastAppliedSfxVolume = sfxVolume;

        if (valuesChanged)
        {
            SaveVolumes();
        }
    }

    private bool ShouldReapplyVolumeState()
    {
        if (Mathf.Abs(lastAppliedMasterVolume - masterVolume) > 0.0001f)
            return true;

        if (Mathf.Abs(lastAppliedMusicVolume - musicVolume) > 0.0001f)
            return true;

        if (Mathf.Abs(lastAppliedSfxVolume - sfxVolume) > 0.0001f)
            return true;

        return false;
    }

    private void LoadVolumes()
    {
        string path = GetVolumeSettingsPath();
        if (!File.Exists(path))
        {
            SaveVolumes();
            return;
        } else
        {
            Debug.Log($"[AudioManager] Loading volume settings from {path}");
        }

        try
        {
            string json = File.ReadAllText(path);
            AudioVolumeSettings settings = JsonUtility.FromJson<AudioVolumeSettings>(json);
            masterVolume = Mathf.Clamp01(settings.masterVolume);
            musicVolume = Mathf.Clamp01(settings.musicVolume);
            sfxVolume = Mathf.Clamp01(settings.sfxVolume);
            Debug.Log($"[AudioManager] Loaded volume settings from {path}");
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[AudioManager] Failed to load volume settings: {ex.Message}");
            SaveVolumes();
        }
    }

    private void SaveVolumes()
    {
        string path = GetVolumeSettingsPath();
        string directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        AudioVolumeSettings settings = new AudioVolumeSettings
        {
            masterVolume = masterVolume,
            musicVolume = musicVolume,
            sfxVolume = sfxVolume
        };

        File.WriteAllText(path, JsonUtility.ToJson(settings, true));
        Debug.Log($"[AudioManager] Saved volume settings to {path}");
    }

    private string GetVolumeSettingsPath()
    {
        return Path.Combine(Application.persistentDataPath, VolumeSettingsFileName);
    }
}
