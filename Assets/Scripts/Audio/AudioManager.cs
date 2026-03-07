using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Mixer")]
    public AudioMixer audioMixer;

    [Header("BGM")]
    public AudioSource bgmSource;
    public AudioClip dayBGM;
    public AudioClip nightBGM;
    public float fadeDuration = 1.5f;

    [Header("SFX")]
    public AudioSource sfxSource;
    public AudioClip pickupClip;

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
        PlayBGM(true);
        UpdateIcons();
    }

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
        PlayerPrefs.SetFloat("BGMVolume", bgmSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", sfxSlider.value);

        PlayerPrefs.SetInt("BGMMuted", isBgmMuted ? 1 : 0);
        PlayerPrefs.SetInt("SFXMuted", isSfxMuted ? 1 : 0);

        PlayerPrefs.Save();
    }

    public void PlayBGM(bool isDay)
    {
        AudioClip newClip = isDay ? dayBGM : nightBGM;

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
}