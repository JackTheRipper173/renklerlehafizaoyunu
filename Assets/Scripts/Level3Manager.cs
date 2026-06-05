using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Level3Manager : MonoBehaviour
{
    [Header("UI Panelleri")]
    public TMP_Text scoreText;
    public TMP_Text levelText;
    public GameObject resultPanel;
    public TMP_Text resultText;
    public Button retryButton;
    public Button mainMenuButton;
    public Button nextLevelButton;
    public GameObject InfoPanel;

    [Header("Görsel Gösterge")]
    public Image displayImage;

    [Header("Giriş Butonları")]
    public Button[] inputButtons;

    [Header("Renk Ayarları")]
    public Color[] colors;
    public float flashDuration = 0.7f;
    public float delayBetweenFlashes = 0.3f;

    private List<int> sequence = new List<int>();
    private int playerIndex = 0;
    private int level = 3;
    private int score = 0;
    private bool isShowingSequence = false;
    private Color originalDisplayColor;

    void Start()
    {
        resultPanel.SetActive(false);
        if (displayImage != null)
            originalDisplayColor = displayImage.color;

        retryButton.onClick.AddListener(() => SceneManager.LoadScene("Level3"));
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
        int nextLevelNumber = level + 1; 

        // Eğer sonraki seviye halihazırda önceden açılmışsa buton aktiftir
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
        SceneManager.LoadScene("Level" + (level + 1)); // Doğrudan Level4 sahnesini yükler
    }

    public void OnStartButtonPressed() { StartGame(); }

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
        Color gosterilecekRenk = colors[index];
        gosterilecekRenk.a = 1f;

        displayImage.color = gosterilecekRenk;

        // Rengin ekranda kalma süresi
        yield return new WaitForSeconds(flashDuration);

        displayImage.color = new Color(0.2f, 0.2f, 0.2f, 1f); // Koyu Gri (Sönme efekti)
        yield return new WaitForSeconds(0.1f);
    }

    public void OnColorPressed(int index)
    {
        if (isShowingSequence) return;

        if (sequence[playerIndex] == index)
        {
            playerIndex++;
            if (playerIndex >= sequence.Count)
            {
                if (sequence.Count < 6)
                {
                    score += 4;
                    UpdateUI();
                    AddNewColor();
                    StartCoroutine(ShowSequence());
                }
                else
                {
                    score += 4;
                    UpdateUI();
                    LevelCompleted();
                }
            }
        }
        else { GameOver(); }
    }

    void SetButtonsInteractable(bool state)
    {
        foreach (Button btn in inputButtons) btn.interactable = state;
    }

    void LevelCompleted()
    {
        int reachedLevel = PlayerPrefs.GetInt("ReachedLevel", 1);
        int currentLevelNum = 3;

        if (reachedLevel <= currentLevelNum)
        {
            PlayerPrefs.SetInt("ReachedLevel", currentLevelNum + 1); // ReachedLevel'ı 4 yapar
            PlayerPrefs.Save();
            Debug.Log("Kilit Açıldı! Yeni Ulaşılan Seviye: " + (currentLevelNum + 1));
        }

        // İlk kez oynanıyor olsa bile seviye tamamlandığı an buton aktif hale gelir
        if (nextLevelButton != null)
        {
            nextLevelButton.interactable = true;
        }

        resultPanel.SetActive(true);
        resultText.text = "Mükemmel! Seviye " + currentLevelNum + " Tamamlandı.";
    }

    void GameOver()
    {
        // Seviye önceden açıldıysa aktif kalır, açılmadıysa pasif kalır.
        resultPanel.SetActive(true);
        resultText.text = "Oyun Bitti!";
    }

    void UpdateUI() { scoreText.text = "Puan: " + score; levelText.text = "Level: " + level; }
}