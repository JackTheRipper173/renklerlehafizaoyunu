using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Level4Manager : MonoBehaviour
{
    [Header("UI Panelleri")]
    public TMP_Text scoreText;
    public TMP_Text levelText;
    public GameObject resultPanel;
    public TMP_Text resultText;
    public Button retryButton;
    public Button mainMenuButton;
    public GameObject InfoPanel;

    [Header("Görsel Gösterge")]
    public Image displayImage;

    [Header("Giriş Butonları")]
    public Button[] inputButtons; // Artık 4 buton olacak

    [Header("Renk Ayarları")]
    public Color[] colors; // Artık 4 renk olacak
    public float flashDuration = 0.6f; // Biraz kısaldı
    public float delayBetweenFlashes = 0.25f;

    private List<int> sequence = new List<int>();
    private int playerIndex = 0;
    private int level = 4;
    private int score = 0;
    private bool isShowingSequence = false;
    private Color originalDisplayColor;

    void Start()
    {
        resultPanel.SetActive(false);

        if (displayImage != null)
            originalDisplayColor = displayImage.color;

        retryButton.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().name));
        mainMenuButton.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));

        UpdateUI();
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
                score += 5;
                UpdateUI();

                if (sequence.Count < 8) // Sekans artık 8'e kadar
                {
                    AddNewColor();
                    StartCoroutine(ShowSequence());
                }
                else
                {
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
        resultPanel.SetActive(true);
        resultText.text = "Tebrikler!";
    }

    void GameOver()
    {
        resultPanel.SetActive(true);
        resultText.text = "Oyun Bitti!";
    }

    void UpdateUI()
    {
        scoreText.text = "Puan: " + score;
        levelText.text = "Level: " + level;
    }
}