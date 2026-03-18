using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Mixer Settings")]
    public AudioMixer audioMixer;
    public string bgmParameter = "BGMVolume";
    public string sfxParameter = "SFXVolume";

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;
    public AudioSource voiceSource;

    [Header("BGM Tracks")]
    public AudioClip menuBGM;
    public AudioClip bgmLevel1_Cave;
    public AudioClip bgmLevel2_Stealth;
    public AudioClip bgmLevel3_Escort;

    [Header("SFX Tracks")]
    public AudioClip btnClickClip;
    public AudioClip pickupClip;

    [Header("Audio Settings")]
    public float fadeDuration = 1.0f;
    [Range(0f, 1f)] public float duckingVolume = 0.0f;
    public float duckingSpeed = 0.4f;

    private float savedBgmVolume;
    private float savedSfxVolume;
    private bool isBgmMuted;
    private bool isSfxMuted;
    private Coroutine fadeCoroutine;
    private Coroutine duckingCoroutine;

    // THÊM BIẾN NÀY ĐỂ KIỂM SOÁT
    public bool isDucking { get; private set; } = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        LoadSettings();
        if (SceneManager.GetActiveScene().name.Contains("Menu")) PlayBGM(menuBGM);
        else PlayBGM(bgmLevel1_Cave);
    }

    public void GoToGameplay(string sceneName) { StartCoroutine(TransitionSceneAndAudio(bgmLevel1_Cave, sceneName)); }
    public void GoToMenu(string sceneName) { StartCoroutine(TransitionSceneAndAudio(menuBGM, sceneName)); }

    IEnumerator TransitionSceneAndAudio(AudioClip nextBGM, string sceneName)
    {
        PlayButtonClickSound();
        if (bgmSource.isPlaying)
        {
            float t = 0;
            float startVol = bgmSource.volume;
            while (t < 0.5f) { bgmSource.volume = Mathf.Lerp(startVol, 0, t / 0.5f); t += Time.deltaTime; yield return null; }
        }
        bgmSource.Stop();
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone) yield return null;
        PlayBGM(nextBGM);
    }

    // ================= DUCKING CÓ ĐÁNH DẤU =================
    public void StartDucking()
    {
        isDucking = true; // Đánh dấu là đang nói
        if (duckingCoroutine != null) StopCoroutine(duckingCoroutine);
        duckingCoroutine = StartCoroutine(LerpBGMVolume(duckingVolume));
    }

    public void StopDucking()
    {
        isDucking = false; // Đánh dấu là nói xong
        if (duckingCoroutine != null) StopCoroutine(duckingCoroutine);
        duckingCoroutine = StartCoroutine(LerpBGMVolume(1f));
    }

    private IEnumerator LerpBGMVolume(float targetPercent)
    {
        float startVol = bgmSource.volume;
        float time = 0;
        while (time < duckingSpeed)
        {
            bgmSource.volume = Mathf.Lerp(startVol, targetPercent, time / duckingSpeed);
            time += Time.deltaTime; yield return null;
        }
        bgmSource.volume = targetPercent;
    }

    public void PlayLevel1() => PlayBGM(bgmLevel1_Cave);
    public void PlayLevel2() => PlayBGM(bgmLevel2_Stealth);
    public void PlayLevel3() => PlayBGM(bgmLevel3_Escort);

    public void PlayBGM(AudioClip newClip)
    {
        if (newClip == null) return;
        if (bgmSource.clip == newClip && bgmSource.isPlaying) return;
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeBGM(newClip));
    }

    private IEnumerator FadeBGM(AudioClip newClip)
    {
        if (bgmSource.isPlaying)
        {
            float t = 0;
            while (t < fadeDuration) { bgmSource.volume = Mathf.Lerp(1, 0, t / fadeDuration); t += Time.deltaTime; yield return null; }
        }
        bgmSource.clip = newClip;
        bgmSource.Play();
        bgmSource.loop = true;
        float tIn = 0;

        // TÍNH TOÁN LẠI ĐÍCH ĐẾN CỦA ÂM LƯỢNG
        float targetVol = isDucking ? duckingVolume : 1f;

        while (tIn < fadeDuration) { bgmSource.volume = Mathf.Lerp(0, targetVol, tIn / fadeDuration); tIn += Time.deltaTime; yield return null; }
        bgmSource.volume = targetVol;
    }

    public void PlayButtonClickSound() => PlaySFX(btnClickClip);
    public void PlayItemPickup() => PlaySFX(pickupClip);
    public void PlaySFX(AudioClip clip) { if (clip != null && !isSfxMuted) sfxSource.PlayOneShot(clip); }

    private void LoadSettings()
    {
        savedBgmVolume = PlayerPrefs.GetFloat("BGMVolume", 0.75f);
        savedSfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
        isBgmMuted = PlayerPrefs.GetInt("BGMMuted", 0) == 1;
        isSfxMuted = PlayerPrefs.GetInt("SFXMuted", 0) == 1;

        ApplyMixerVolume(bgmParameter, isBgmMuted ? 0.0001f : savedBgmVolume);
        ApplyMixerVolume(sfxParameter, isSfxMuted ? 0.0001f : savedSfxVolume);
    }

    private void ApplyMixerVolume(string parameterName, float volume)
    {
        if (audioMixer == null) return;
        float decibel = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat(parameterName, decibel);
    }

    public void ApplySettings() => PlayerPrefs.Save();
    public void SetBGMVolume(float v) { savedBgmVolume = v; if (!isBgmMuted) ApplyMixerVolume(bgmParameter, v); PlayerPrefs.SetFloat("BGMVolume", v); }
    public void SetSFXVolume(float v) { savedSfxVolume = v; if (!isSfxMuted) ApplyMixerVolume(sfxParameter, v); PlayerPrefs.SetFloat("SFXVolume", v); }
}