using UnityEngine;

public class UIAudioManager : MonoBehaviour
{
    public static UIAudioManager Instance;

    public AudioSource audioSource;

    [Header("UI Click Sounds")]
    public AudioClip menuClickSound;   // Ana menü butonlarý
    public AudioClip levelClickSound;  // Seviye içi butonlar

    private void Awake()
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

    public void PlayMenuClick()
    {
        PlayClip(menuClickSound);
    }

    public void PlayLevelClick()
    {
        PlayClip(levelClickSound);
    }

    private void PlayClip(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
