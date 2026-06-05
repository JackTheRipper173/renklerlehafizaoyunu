using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Level2Manager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text scoreText;
    public TMP_Text levelText;
    public Button[] colorButtons;
    public GameObject resultPanel;
    public TMP_Text resultText;
    public Button retryButton;
    public Button mainMenuButton;
    public Button nextLevelButton; // Sonraki seviye kontrolü için eklendi
    public GameObject InfoPanel;

    [Header("Flash Settings")]
    public float flashDuration = 0.5f;
    public float delayBetweenFlashes = 0.2f;

    private List<int> sequence = new List<int>();
    private int playerIndex = 0;
    private int level = 2; // Bu sahnenin seviyesi
    private int score = 0;
    private bool isShowingSequence = false;

    private int stepsToCompleteLevel = 7;

    void Start()
    {
        resultPanel.SetActive(false);

        retryButton.onClick.AddListener(() => SceneManager.LoadScene("Level2"));
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

        // Tek Şart: Eğer sonraki seviye halihazırda önceden açılmışsa buton aktiftir
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
        SceneManager.LoadScene("Level" + (level + 1)); // Doğrudan Level3 sahnesini yükler
    }

    public void OnStartButtonPressed()
    {
        StartGame();
    }

    void StartGame()
    {
        InfoPanel.SetActive(false);
        level = 2;
        score = 0;
        sequence.Clear();
        playerIndex = 0;

        AddNewColor();
        StartCoroutine(ShowSequence());
        UpdateUI();
    }

    void AddNewColor()
    {
        int randomColor = Random.Range(0, colorButtons.Length);
        sequence.Add(randomColor);
    }

    IEnumerator ShowSequence()
    {
        foreach (Button btn in colorButtons)
            btn.interactable = false;

        isShowingSequence = true;
        playerIndex = 0;

        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < sequence.Count; i++)
        {
            yield return StartCoroutine(FlashButton(sequence[i]));
            yield return new WaitForSeconds(delayBetweenFlashes);
        }

        isShowingSequence = false;

        foreach (Button btn in colorButtons)
            btn.interactable = true;
    }

    IEnumerator FlashButton(int index)
    {
        Button btn = colorButtons[index];
        Image img = btn.GetComponent<Image>();

        btn.enabled = false;

        Color originalColor = img.color;
        img.color = Color.white;
        yield return new WaitForSeconds(flashDuration);
        img.color = originalColor;

        btn.enabled = true;
    }

    public void OnColorPressed(int index)
    {
        if (isShowingSequence) return;

        if (sequence[playerIndex] == index)
        {
            playerIndex++;

            if (playerIndex >= sequence.Count)
            {
                if (sequence.Count < stepsToCompleteLevel)
                {
                    score += 3;
                    UpdateUI();
                    AddNewColor();
                    StartCoroutine(ShowSequence());
                }
                else
                {
                    score += 3;
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

    void LevelCompleted()
    {
        int currentLevel = 2;
        int reachedLevel = PlayerPrefs.GetInt("ReachedLevel", 1);

        if (reachedLevel <= currentLevel)
        {
            PlayerPrefs.SetInt("ReachedLevel", currentLevel + 1); // ReachedLevel'ı 3 yapar
            PlayerPrefs.Save();
        }

        // İlk kez oynanıyor olsa bile seviye tamamlandığı an buton aktif hale gelir
        if (nextLevelButton != null)
        {
            nextLevelButton.interactable = true;
        }

        resultPanel.SetActive(true);
        resultText.text = "Tebrikler! Seviye 2 tamamlandı.";
    }

    void GameOver()
    {
        
        resultPanel.SetActive(true);
        resultText.text = "Yanlış seçim! Oyun Bitti.";
    }

    void UpdateUI()
    {
        scoreText.text = "Puan: " + score;
        levelText.text = "Level: " + level;
    }
}