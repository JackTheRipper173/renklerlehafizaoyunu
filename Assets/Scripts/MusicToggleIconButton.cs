using UnityEngine;
using UnityEngine.UI;
using UIImage = UnityEngine.UI.Image;
using static System.Net.Mime.MediaTypeNames;

public class MusicToggleIconButton : MonoBehaviour
{
    public Button toggleButton;
    public UnityEngine.UI.Image iconImage;

    public Sprite iconOn;   // Müzik açýkken
    public Sprite iconOff;  // Müzik kapalýyken

    private void Start()
    {
        if (toggleButton == null)
            toggleButton = GetComponent<Button>();

        if (iconImage == null)
            iconImage = GetComponent<UnityEngine.UI.Image>();

        toggleButton.onClick.AddListener(OnToggleClicked);
        RefreshIcon();
    }

    private void OnToggleClicked()
    {
        if (MusicManager.Instance != null)
            MusicManager.Instance.ToggleMusic();

        RefreshIcon();
    }

    private void RefreshIcon()
    {
        bool on = MusicManager.Instance == null
                  || MusicManager.Instance.MusicEnabled;

        iconImage.sprite = on ? iconOn : iconOff;
    }
}
