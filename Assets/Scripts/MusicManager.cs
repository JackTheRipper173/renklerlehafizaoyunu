using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Müzik Kaynaðý")]
    public AudioSource musicSource;   // Arkaplan müziðini çalan AudioSource

    private const string MusicPrefKey = "MusicEnabled";
    public bool MusicEnabled { get; private set; }

    private void Awake()
    {
        // Singleton kur
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (musicSource == null)
            musicSource = GetComponent<AudioSource>();

        // Kayýtlý ayarý oku (varsayýlan: açýk)
        MusicEnabled = PlayerPrefs.GetInt(MusicPrefKey, 1) == 1;
        ApplyMusicState();
    }

    public void ToggleMusic()
    {
        SetMusicEnabled(!MusicEnabled);
    }

    public void SetMusicEnabled(bool enabled)
    {
        MusicEnabled = enabled;
        PlayerPrefs.SetInt(MusicPrefKey, MusicEnabled ? 1 : 0);
        PlayerPrefs.Save();
        ApplyMusicState();
    }

    private void ApplyMusicState()
    {
        if (musicSource == null) return;

        if (MusicEnabled)
        {
            musicSource.mute = false;

            if (!musicSource.isPlaying)
                musicSource.Play();
        }
        else
        {
            musicSource.mute = true;
        }
    }
}
