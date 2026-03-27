using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Level1Manager : MonoBehaviour
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
    public float flashDuration = 0.8f;
    public float delayBetweenFlashes = 0.3f;

    private List<int> sequence = new List<int>();
    private int playerIndex = 0;
    private int level = 1;
    private int score = 0;
    private bool isShowingSequence = false;

    private int stepsToCompleteLevel = 5; // Level 1 için toplam adım sayısı

    void Start()
    {
        resultPanel.SetActive(false);

        retryButton.onClick.AddListener(() => SceneManager.LoadScene("Level1"));
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
        level = 1;
        score = 0;
        sequence.Clear();
        playerIndex = 0;

        // İlk adımı ekle
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

        yield return new WaitForSeconds(1f);

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
        yield return new WaitForSeconds(0.4f);
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
                // Eğer seviyeyi tamamlamadıysa yeni adım ekle
                if (sequence.Count < stepsToCompleteLevel)
                {
                    score += 2; // her doğru adım puanı
                    UpdateUI();
                    AddNewColor();
                    StartCoroutine(ShowSequence());
                }
                else
                {
                    // Level tamamlandı
                    score += 2;
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
        resultText.text = "Tebrikler! Seviye 1 tamamlandı.";
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