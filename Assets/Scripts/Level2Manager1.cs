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
    public GameObject InfoPanel;

    [Header("Flash Settings")]
    public float flashDuration = 0.5f;          // Kısaltıldı (Level 1: 0.8f)
    public float delayBetweenFlashes = 0.2f;    // Kısaltıldı (Level 1: 0.3f)

    private List<int> sequence = new List<int>();
    private int playerIndex = 0;
    private int level = 2;
    private int score = 0;
    private bool isShowingSequence = false;

    private int stepsToCompleteLevel = 7; // Artırıldı (Level 1: 5)

    void Start()
    {
        resultPanel.SetActive(false);

        retryButton.onClick.AddListener(() => SceneManager.LoadScene("Level2"));
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
                    score += 3; // Level 2'de puan biraz daha yüksek
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