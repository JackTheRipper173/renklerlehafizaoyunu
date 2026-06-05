using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Level5Manager : MonoBehaviour
{
    [Header("UI Panelleri")]
    public TMP_Text scoreText;
    public TMP_Text levelText;
    public GameObject resultPanel;
    public TMP_Text resultText;
    public Button retryButton;
    public Button mainMenuButton;
    public Button nextLevelButton; // Sonraki seviye kontrolü için eklendi
    public GameObject InfoPanel;

    [Header("Görsel Gösterge")]
    public Image displayImage;

    [Header("Giriş Butonları")]
    public Button[] inputButtons;

    [Header("Renk Ayarları")]
    public Color[] colors;
    public float flashDuration = 0.4f;   // Daha hızlı
    public float delayBetweenFlashes = 0.15f;  // Daha kısa bekleme

    private List<int> sequence = new List<int>();
    private int playerIndex = 0;
    private int level = 5; // Bu sahnenin seviyesi
    private int score = 0;
    private bool isShowingSequence = false;
    private Color originalDisplayColor;

    void Start()
    {
        resultPanel.SetActive(false);

        if (displayImage != null)
            originalDisplayColor = displayImage.color;

        retryButton.onClick.AddListener(() => SceneManager.LoadScene("Level5"));
        mainMenuButton.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));

        // --- SONRAKİ SEVİYE BUTON KONTROLÜ ---
        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.RemoveAllListeners(); // Eski kalıntıları temizle
            nextLevelButton.onClick.AddListener(OnNextLevelPressed);
            CheckNextLevelLock(); // Sahne ilk açıldığında genel kilit durumuna göre aktif/pasif yapar
        }

        UpdateUI();
    }

    // Butonun aktifliğini SADECE genel kilit durumuna bağlayan fonksiyon
    void CheckNextLevelLock()
    {
        int reachedLevel = PlayerPrefs.GetInt("ReachedLevel", 1);
        int nextLevelNumber = level + 1; // Seviye 5 için sonraki seviye 6'dır

        // Eğer sonraki seviye (Level 6) halihazırda önceden açılmışsa buton aktiftir
        if (reachedLevel >= nextLevelNumber)
        {
            nextLevelButton.interactable = true;
        }
        else
        {
            nextLevelButton.interactable = false; // İlk kez oynanıyorsa kilitli başlar
        }
    }

    // Sonraki seviye butonuna basıldığında tetiklenecek fonksiyon
    void OnNextLevelPressed()
    {
        SceneManager.LoadScene("Level" + (level + 1)); // Doğrudan Level6 sahnesini yükler
    }

    public void OnStartButtonPressed()
    {
        StartGame();
    }

    void StartGame()
    {
        InfoPanel.SetActive(false);
        score = 0;
        sequence.Clear();
        AddNewColor();
        StartCoroutine(ShowSequence());
        UpdateUI();
    }

    void AddNewColor()
    {
        int randomColor = Random.Range(0, inputButtons.Length);
        sequence.Add(randomColor);
    }

    IEnumerator ShowSequence()
    {
        isShowingSequence = true;
        playerIndex = 0;

        SetButtonsInteractable(false);

        yield return new WaitForSeconds(1f);

        for (int i = 0; i < sequence.Count; i++)
        {
            yield return StartCoroutine(FlashDisplay(sequence[i]));
            yield return new WaitForSeconds(delayBetweenFlashes);
        }

        isShowingSequence = false;
        SetButtonsInteractable(true);
    }

    IEnumerator FlashDisplay(int index)
    {
        Color showColor = colors[index];
        showColor.a = 1f;

        displayImage.color = showColor;

        yield return new WaitForSeconds(flashDuration);

        displayImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        yield return new WaitForSeconds(0.08f);
    }

    public void OnColorPressed(int index)
    {
        if (isShowingSequence) return;

        if (sequence[playerIndex] == index)
        {
            playerIndex++;

            if (playerIndex >= sequence.Count)
            {
                if (sequence.Count < 10)   // Maksimum sekans
                {
                    score += 6;
                    UpdateUI();
                    AddNewColor();
                    StartCoroutine(ShowSequence());
                }
                else
                {
                    score += 6;
                    UpdateUI();
                    LevelCompleted();
                }
            }
        }
        else
        {
            GameOver();
        }
    }

    void SetButtonsInteractable(bool state)
    {
        foreach (Button btn in inputButtons)
            btn.interactable = state;
    }

    void LevelCompleted()
    {
        int reachedLevel = PlayerPrefs.GetInt("ReachedLevel", 1);
        int currentLevelNum = 5;

        if (reachedLevel <= currentLevelNum)
        {
            PlayerPrefs.SetInt("ReachedLevel", currentLevelNum + 1); // ReachedLevel'ı 6 yapar
            PlayerPrefs.Save();
            Debug.Log("Kilit Açıldı! Yeni Ulaşılan Seviye: " + (currentLevelNum + 1));
        }

        // Seviye ilk kez tamamlanıyor olsa bile buton panelle birlikte anında aktifleşir
        if (nextLevelButton != null)
        {
            nextLevelButton.interactable = true;
        }

        resultPanel.SetActive(true);
        resultText.text = "Mükemmel! Seviye " + currentLevelNum + " Tamamlandı.";
    }

    void GameOver()
    {
        // Butonun durumuna dokunulmuyor. Seviye 6 önceden açıldıysa aktif kalır, açılmadıysa pasif kalır.
        resultPanel.SetActive(true);
        resultText.text = "Oyun Bitti!";
    }

    public void HideOption(int index)
    {
        if (index >= 0 && index < inputButtons.Length)
        {
            inputButtons[index].gameObject.SetActive(false);
        }
    }

    public void RevealOption(int index)
    {
        if (index >= 0 && index < inputButtons.Length)
        {
            inputButtons[index].gameObject.SetActive(true);
        }
    }

    void UpdateUI()
    {
        scoreText.text = "Puan: " + score;
        levelText.text = "Level: " + level;
    }
}