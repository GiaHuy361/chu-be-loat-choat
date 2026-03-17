using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Mixer Settings")]
    public AudioMixer audioMixer;
    public string bgmParameter = "BGMVolume";
    public string sfxParameter = "SFXVolume";

    [Header("Audio Sources")]
    [Tooltip("Source phát nhạc nền (BGM)")]
    public AudioSource bgmSource;
    [Tooltip("Source phát hiệu ứng âm thanh (SFX)")]
    public AudioSource sfxSource;

    [Header("BGM Tracks")]
    public AudioClip menuBGM;
    [Tooltip("Màn 1: Lấy thư trong hang")]
    public AudioClip bgmLevel1_Cave;
    [Tooltip("Màn 2: Trinh sát đồn địch")]
    public AudioClip bgmLevel2_Stealth;
    [Tooltip("Màn 3: Hộ tống cán bộ")]
    public AudioClip bgmLevel3_Escort;

    [Header("SFX Tracks")]
    public AudioClip btnClickClip;
    public AudioClip pickupClip;

    [Header("Audio Settings & Timings")]
    public float fadeDuration = 1.0f;
    [Range(0f, 1f)] public float duckingVolume = 0.25f; // Âm lượng BGM khi có hội thoại (25%)
    public float duckingSpeed = 0.5f;

    [Header("UI Controls (Optional)")]
    public Slider bgmSlider;
    public Slider sfxSlider;
    public Image bgmToggleImage;
    public Image sfxToggleImage;
    public Sprite bgmOnSprite, bgmOffSprite;
    public Sprite sfxOnSprite, sfxOffSprite;

    // Trạng thái lưu trữ
    private float savedBgmVolume;
    private float savedSfxVolume;
    private bool isBgmMuted;
    private bool isSfxMuted;

    // Quản lý tiến trình (tránh xung đột)
    private Coroutine fadeCoroutine;
    private Coroutine duckingCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        LoadSettings();
        SetupUI();

        if (SceneManager.GetActiveScene().name == "Menu" && menuBGM != null)
        {
            PlayBGM(menuBGM);
        }
    }

    // =========================================================
    // 1. CÁC HÀM XỬ LÝ CHUYỂN SCENE & FADE NHẠC (ĐÃ KHÔI PHỤC)
    // =========================================================

    public void GoToGameplay(string sceneName)
    {
        // Mặc định khi bấm Play sẽ mở nhạc của màn 1
        StartCoroutine(TransitionSceneAndAudio(bgmLevel1_Cave, sceneName));
    }

    public void GoToMenu(string sceneName)
    {
        StartCoroutine(TransitionSceneAndAudio(menuBGM, sceneName));
    }

    IEnumerator TransitionSceneAndAudio(AudioClip nextBGM, string sceneName)
    {
        PlayButtonClickSound();

        float waitTime = btnClickClip != null ? btnClickClip.length : 0.5f;
        float time = 0;
        float startVolume = bgmSource.volume;

        // Fade out nhạc cũ
        while (time < waitTime)
        {
            bgmSource.volume = Mathf.Lerp(startVolume, 0, time / waitTime);
            time += Time.deltaTime;
            yield return null;
        }

        bgmSource.volume = 0;
        bgmSource.Stop();

        // Chuyển Scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (asyncLoad != null && !asyncLoad.isDone)
        {
            yield return null;
        }

        // Phát nhạc mới
        if (nextBGM != null)
        {
            bgmSource.clip = nextBGM;
            bgmSource.Play();

            // Fade in nhạc mới
            time = 0;
            while (time < fadeDuration)
            {
                bgmSource.volume = Mathf.Lerp(0, 1, time / fadeDuration);
                time += Time.deltaTime;
                yield return null;
            }
            bgmSource.volume = 1;
        }
    }

    // =========================================================
    // 2. QUẢN LÝ NHẠC NỀN (BGM) & DUCKING THEO GAMEPLAY
    // =========================================================

    public void PlayLevel1() => PlayBGM(bgmLevel1_Cave);
    public void PlayLevel2() => PlayBGM(bgmLevel2_Stealth);
    public void PlayLevel3() => PlayBGM(bgmLevel3_Escort);

    public void PlayBGM(AudioClip newClip)
    {
        if (newClip == null || bgmSource.clip == newClip) return;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeBGM(newClip));
    }

    private IEnumerator FadeBGM(AudioClip newClip)
    {
        float startVol = bgmSource.volume;
        float time = 0;

        if (bgmSource.isPlaying)
        {
            while (time < fadeDuration)
            {
                bgmSource.volume = Mathf.Lerp(startVol, 0f, time / fadeDuration);
                time += Time.deltaTime;
                yield return null;
            }
        }

        bgmSource.Stop();
        bgmSource.clip = newClip;
        bgmSource.Play();

        time = 0;
        while (time < fadeDuration)
        {
            bgmSource.volume = Mathf.Lerp(0f, 1f, time / fadeDuration);
            time += Time.deltaTime;
            yield return null;
        }

        bgmSource.volume = 1f;
    }

    public void StartDialogMode()
    {
        if (duckingCoroutine != null) StopCoroutine(duckingCoroutine);
        duckingCoroutine = StartCoroutine(AdjustBGMVolume(duckingVolume, duckingSpeed));
    }

    public void EndDialogMode()
    {
        if (duckingCoroutine != null) StopCoroutine(duckingCoroutine);
        duckingCoroutine = StartCoroutine(AdjustBGMVolume(1f, duckingSpeed));
    }

    private IEnumerator AdjustBGMVolume(float targetVolume, float duration)
    {
        float startVol = bgmSource.volume;
        float time = 0;

        while (time < duration)
        {
            bgmSource.volume = Mathf.Lerp(startVol, targetVolume, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        bgmSource.volume = targetVolume;
    }

    // =========================================================
    // 3. QUẢN LÝ HIỆU ỨNG (SFX) (ĐÃ KHÔI PHỤC TÊN CŨ)
    // =========================================================

    public void PlayButtonClickSound() => PlaySFX(btnClickClip);
    public void PlayItemPickup() => PlaySFX(pickupClip);

    public void PlaySFX(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (clip == null || isSfxMuted) return;
        sfxSource.PlayOneShot(clip, volumeMultiplier);
    }

    // =========================================================
    // 4. SETTINGS & UI (SLIDER, TOGGLE) (ĐÃ KHÔI PHỤC TÊN CŨ)
    // =========================================================

    private void SetupUI()
    {
        if (bgmSlider != null)
        {
            bgmSlider.minValue = 0.0001f;
            bgmSlider.maxValue = 1f;
            bgmSlider.value = savedBgmVolume;
            bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.minValue = 0.0001f;
            sfxSlider.maxValue = 1f;
            sfxSlider.value = savedSfxVolume;
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }

        UpdateIcons();
    }

    private void LoadSettings()
    {
        savedBgmVolume = PlayerPrefs.GetFloat("BGMVolume", 0.75f);
        savedSfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
        isBgmMuted = PlayerPrefs.GetInt("BGMMuted", 0) == 1;
        isSfxMuted = PlayerPrefs.GetInt("SFXMuted", 0) == 1;

        ApplyMixerVolume(bgmParameter, isBgmMuted ? 0.0001f : savedBgmVolume);
        ApplyMixerVolume(sfxParameter, isSfxMuted ? 0.0001f : savedSfxVolume);
    }

    public void SetBGMVolume(float value)
    {
        savedBgmVolume = value;
        if (!isBgmMuted) ApplyMixerVolume(bgmParameter, value);
    }

    public void SetSFXVolume(float value)
    {
        savedSfxVolume = value;
        if (!isSfxMuted) ApplyMixerVolume(sfxParameter, value);
    }

    public void ToggleBGM()
    {
        isBgmMuted = !isBgmMuted;
        ApplyMixerVolume(bgmParameter, isBgmMuted ? 0.0001f : savedBgmVolume);
        UpdateIcons();
        PlayButtonClickSound();
    }

    public void ToggleSFX()
    {
        isSfxMuted = !isSfxMuted;
        ApplyMixerVolume(sfxParameter, isSfxMuted ? 0.0001f : savedSfxVolume);
        UpdateIcons();
        PlayButtonClickSound();
    }

    private void ApplyMixerVolume(string parameterName, float volume)
    {
        float decibel = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat(parameterName, decibel);
    }

    private void UpdateIcons()
    {
        if (bgmToggleImage != null && bgmOnSprite != null && bgmOffSprite != null)
            bgmToggleImage.sprite = isBgmMuted ? bgmOffSprite : bgmOnSprite;

        if (sfxToggleImage != null && sfxOnSprite != null && sfxOffSprite != null)
            sfxToggleImage.sprite = isSfxMuted ? sfxOffSprite : sfxOnSprite;
    }

    public void ApplySettings()
    {
        PlayerPrefs.SetFloat("BGMVolume", savedBgmVolume);
        PlayerPrefs.SetFloat("SFXVolume", savedSfxVolume);
        PlayerPrefs.SetInt("BGMMuted", isBgmMuted ? 1 : 0);
        PlayerPrefs.SetInt("SFXMuted", isSfxMuted ? 1 : 0);
        PlayerPrefs.Save();
    }
}