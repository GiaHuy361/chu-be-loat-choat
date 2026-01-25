using UnityEngine;

public class FootstepReceiver : MonoBehaviour
{
    [Header("Audio Source (gắn trên Player)")]
    public AudioSource audioSource;

    [Header("Clips")]
    public AudioClip[] footstepClips;
    public AudioClip jumpClip;
    public AudioClip landClip;

    [Header("Volume")]
    [Range(0f, 1f)] public float footstepVolume = 0.8f;
    [Range(0f, 1f)] public float jumpVolume = 0.9f;
    [Range(0f, 1f)] public float landVolume = 0.9f;

    void Awake()
    {
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        if (!audioSource) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // 3D sound
    }

    // Animation Event gọi (nếu clip walk/run của bạn có event này)
    public void OnFootstep()
    {
        if (footstepClips == null || footstepClips.Length == 0) return;
        if (!audioSource) return;

        var clip = footstepClips[Random.Range(0, footstepClips.Length)];
        audioSource.PlayOneShot(clip, footstepVolume);
    }

    public void PlayJump()
    {
        if (!jumpClip || !audioSource) return;
        audioSource.PlayOneShot(jumpClip, jumpVolume);
    }

    public void PlayLand()
    {
        if (!landClip || !audioSource) return;
        audioSource.PlayOneShot(landClip, landVolume);
    }
}
