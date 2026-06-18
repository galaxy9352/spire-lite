using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [SerializeField] private AudioClip hitSound;

    private void Awake()
    {
        Instance = this;
    }

    public void PlayHitSound(AudioClip hitSound)
    {
        sfxSource.PlayOneShot(hitSound);
    }
}