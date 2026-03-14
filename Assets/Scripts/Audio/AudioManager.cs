using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Mixer")]
    public AudioMixer audioMixer;

    [Header("BGM")]
    public AudioSource bgmSource;
    [Tooltip("Kéo file inendm.mp3 vào đây")]
    public AudioClip menuBGM;
    [Tooltip("Kéo file gaplay.mp3 vào đây")]
    public AudioClip gameplayBGM;
    public float fadeDuration = 1.5f;

    [Header("SFX")]
    public AudioSource sfxSource;
    public AudioClip pickupClip;
    [Tooltip("Kéo file btn_click.wav vào đây")]
    public AudioClip btnClickClip;

    [Header("UI")]
    public Slider bgmSlider;
    public Slider sfxSlider;

    public Image bgmToggleImage;
    public Image sfxToggleImage;

    [Header("Sprites")]
    public Sprite bgmOnSprite;
    public Sprite bgmOffSprite;

    public Sprite sfxOnSprite;
    public Sprite sfxOffSprite;

    bool isBgmMuted;
    bool isSfxMuted;

    float bgmVolume;
    float sfxVolume;

    void Awake()
    {
        // Đảm bảo chỉ có 1 AudioManager tồn tại khi chuyển Scene
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        SetupSliders();
        LoadSettings();

        // Kiểm tra Scene hiện tại để phát đúng nhạc nền ban đầu
        // Lưu ý: Đổi "Menu" thành đúng tên Scene menu của bạn
        if (SceneManager.GetActiveScene().name == "Menu")
            PlayBGM(menuBGM);
        else
            PlayBGM(gameplayBGM);

        UpdateIcons();
    }

    // =========================================================
    // 1. CÁC HÀM XỬ LÝ NÚT BẤM CÓ CHUYỂN SCENE & FADE NHẠC
    // =========================================================

    // Gán vào nút "Play" ở Menu
    public void GoToGameplay(string sceneName)
    {
        StartCoroutine(TransitionSceneAndAudio(gameplayBGM, sceneName));
    }

    // Gán vào nút "Back to Menu" hoặc "Quit to Menu"
    public void GoToMenu(string sceneName)
    {
        StartCoroutine(TransitionSceneAndAudio(menuBGM, sceneName));
    }

    IEnumerator TransitionSceneAndAudio(AudioClip nextBGM, string sceneName)
    {
        // Phát tiếng Click
        PlayButtonClickSound();

        // Tắt dần nhạc nền cũ
        float waitTime = btnClickClip != null ? btnClickClip.length : 0.5f;
        float time = 0;
        float startVolume = bgmSource.volume;

        while (time < waitTime)
        {
            bgmSource.volume = Mathf.Lerp(startVolume, 0, time / waitTime);
            time += Time.deltaTime;
            yield return null;
        }

        bgmSource.volume = 0;
        bgmSource.Stop();

        // Chuyển Scene và đợi Load xong
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (asyncLoad != null && !asyncLoad.isDone)
        {
            yield return null;
        }

        // Đổi nhạc và phát nhạc mới
        bgmSource.clip = nextBGM;
        bgmSource.Play();

        // Tăng dần âm lượng nhạc mới lên
        time = 0;
        while (time < fadeDuration)
        {
            bgmSource.volume = Mathf.Lerp(0, 1, time / fadeDuration);
            time += Time.deltaTime;
            yield return null;
        }

        bgmSource.volume = 1;
    }

    // =========================================================
    // 2. CÁC HÀM XỬ LÝ ÂM THANH BÌNH THƯỜNG (SFX & BGM)
    // =========================================================

    // Gán hàm này vào các nút bình thường (Pause, Resume, Settings...)
    public void PlayButtonClickSound()
    {
        if (btnClickClip != null)
        {
            PlaySFX(btnClickClip);
        }
    }

    public void PlayBGM(AudioClip newClip)
    {
        if (bgmSource.clip == newClip && bgmSource.isPlaying)
            return;

        StopAllCoroutines();
        StartCoroutine(FadeTrack(newClip));
    }

    IEnumerator FadeTrack(AudioClip newClip)
    {
        float time = 0;

        if (bgmSource.isPlaying)
        {
            float startVolume = bgmSource.volume;

            while (time < fadeDuration)
            {
                bgmSource.volume = Mathf.Lerp(startVolume, 0, time / fadeDuration);
                time += Time.deltaTime;
                yield return null;
            }
        }

        bgmSource.volume = 0;
        bgmSource.clip = newClip;
        bgmSource.Play();

        time = 0;

        while (time < fadeDuration)
        {
            bgmSource.volume = Mathf.Lerp(0, 1, time / fadeDuration);
            time += Time.deltaTime;
            yield return null;
        }

        bgmSource.volume = 1;
    }

    public void PlayItemPickup()
    {
        PlaySFX(pickupClip);
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, volume);
    }

    // =========================================================
    // 3. CÁC HÀM QUẢN LÝ UI VÀ SETTING (Giữ nguyên từ code cũ)
    // =========================================================

    void SetupSliders()
    {
        if (bgmSlider)
        {
            bgmSlider.minValue = 0.0001f;
            bgmSlider.maxValue = 1f;
            bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        }

        if (sfxSlider)
        {
            sfxSlider.minValue = 0.0001f;
            sfxSlider.maxValue = 1f;
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }
    }

    void LoadSettings()
    {
        bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

        isBgmMuted = PlayerPrefs.GetInt("BGMMuted", 0) == 1;
        isSfxMuted = PlayerPrefs.GetInt("SFXMuted", 0) == 1;

        if (bgmSlider) bgmSlider.value = bgmVolume;
        if (sfxSlider) sfxSlider.value = sfxVolume;

        if (isBgmMuted)
            audioMixer.SetFloat("BGMVolume", -80f);
        else
            SetBGMVolume(bgmVolume);

        if (isSfxMuted)
            audioMixer.SetFloat("SFXVolume", -80f);
        else
            SetSFXVolume(sfxVolume);
    }

    public void SetBGMVolume(float value)
    {
        if (isBgmMuted) return;

        bgmVolume = value;
        audioMixer.SetFloat("BGMVolume", Mathf.Log10(value) * 20);
    }

    public void SetSFXVolume(float value)
    {
        if (isSfxMuted) return;

        sfxVolume = value;
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20);
    }

    public void ToggleBGM()
    {
        isBgmMuted = !isBgmMuted;

        if (isBgmMuted)
            audioMixer.SetFloat("BGMVolume", -80f);
        else
            SetBGMVolume(bgmSlider.value);

        UpdateIcons();
    }

    public void ToggleSFX()
    {
        isSfxMuted = !isSfxMuted;

        if (isSfxMuted)
            audioMixer.SetFloat("SFXVolume", -80f);
        else
            SetSFXVolume(sfxSlider.value);

        UpdateIcons();
    }

    void UpdateIcons()
    {
        if (bgmToggleImage)
            bgmToggleImage.sprite = isBgmMuted ? bgmOffSprite : bgmOnSprite;

        if (sfxToggleImage)
            sfxToggleImage.sprite = isSfxMuted ? sfxOffSprite : sfxOnSprite;
    }

    public void ApplySettings()
    {
        // Sử dụng trực tiếp biến lưu trữ thay vì gọi UI Slider để tránh lỗi khi chuyển Scene
        PlayerPrefs.SetFloat("BGMVolume", bgmVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);

        PlayerPrefs.SetInt("BGMMuted", isBgmMuted ? 1 : 0);
        PlayerPrefs.SetInt("SFXMuted", isSfxMuted ? 1 : 0);

        PlayerPrefs.Save();
    }
}