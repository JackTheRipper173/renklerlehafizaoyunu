using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

// Liderlik Tablosu Veri Yapıları
[Serializable]
public class ScoreEntry
{
    public string playerName;
    public int score;
    public string date;
}

[Serializable]
public class LeaderboardData
{
    public List<ScoreEntry> list = new List<ScoreEntry>();
}

public class Level16Manager : MonoBehaviour
{
    [Header("UI Elemanları")]
    public TMP_Text scoreText;
    public TMP_Text levelText;
    public GameObject resultPanel; // Oyun bitti paneli
    public TMP_Text resultText;
    public Button retryButton;
    public Button mainMenuButton;
    public TMP_Text fakeText;
    public GameObject InfoPanel;

    [Header("Leaderboard UI Giriş Elemanları")]
    public GameObject nameInputPanel;       // İsim alma paneli
    public TMP_InputField nameInputField;    // İsim yazılan yer
    public Button saveScoreButton;           // Kaydet butonu
    public TMP_Text leaderboardDisplayText;  // Skorların listeleneceği TMP Text

    [Header("Gizlenecek Panel Ebeveynleri (UI Kaosunu Önlemek İçin)")]
    public GameObject colorButtonsPanel; // Renk butonlarının bağlı olduğu üst ebeveyn panel
    public GameObject shapeButtonsPanel; // Şekil butonlarının bağlı olduğu üst ebeveyn panel
    public GameObject beepButtonsPanel;  // Bip butonlarının bağlı olduğu üst ebeveyn panel
    public GameObject displayPanel;      // Ortadaki büyük renk/şekil gösterge paneli

    [Header("Display (Level 10 ile Aynı)")]
    public Image displayImage;
    public Image displayShape;
    public Image fakeDisplayImage; // Yanıltıcı renk

    [Header("Buttons")]
    public Button[] colorButtons;
    public Button[] shapeButtons;
    public Button[] beepButtons;

    [Header("Assets")]
    public Sprite[] shapes;
    public Color[] colors;
    public AudioClip[] beeps;
    public AudioSource audioSource;

    [Header("Settings (SABİTLENDİ - Level 10 Temposu)")]
    public float flashDuration = 0.45f;
    public float delayBetweenFlashes = 0.3f;

    private List<Step> sequence = new List<Step>();
    private int playerIndex = 0;
    private int endlessStage = 1; // Sonsuz moddaki aşama sayacı (Level yerine)
    private int score = 0;

    private int selectedColor = -1;
    private int selectedShape = -1;
    private int selectedBeep = -1;

    private bool isShowingSequence = false;
    private const string LEADERBOARD_KEY = "EndlessGlobalLeaderboard";

    class Step
    {
        public int color;
        public int shape;
        public int beep;
    }

    void Start()
    {
        resultPanel.SetActive(false);
        nameInputPanel.SetActive(false);

        retryButton.onClick.AddListener(() => SceneManager.LoadScene("Level16"));
        mainMenuButton.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));
        saveScoreButton.onClick.AddListener(OnSaveButtonClicked);

        UpdateUI();
    }

    public void OnStartButtonPressed() => StartGame();

    void StartGame()
    {
        InfoPanel.SetActive(false);

        
        if (colorButtonsPanel != null) colorButtonsPanel.SetActive(true);
        if (shapeButtonsPanel != null) shapeButtonsPanel.SetActive(true);
        if (beepButtonsPanel != null) beepButtonsPanel.SetActive(true);
        if (displayPanel != null) displayPanel.SetActive(true);

        if (scoreText != null) scoreText.gameObject.SetActive(true);
        if (levelText != null) levelText.gameObject.SetActive(true);
        if (fakeText != null) fakeText.gameObject.SetActive(true);
        if (fakeDisplayImage != null) fakeDisplayImage.gameObject.SetActive(true);

        if (colors.Length == 0 || shapes.Length == 0 || beeps.Length < 2)
        {
            Debug.LogError("Gerekli assetler (Color, Shape, Beeps) eksik!");
            return;
        }
        score = 0;
        endlessStage = 1;

        // Orijinal Seviye 10 sabit hızları
        flashDuration = 0.45f;
        delayBetweenFlashes = 0.3f;

        sequence.Clear();
        AddNewStep();
        StartCoroutine(ShowSequence());
        UpdateUI();
    }

    void AddNewStep()
    {
        Step step = new Step();
        step.color = Random.Range(0, colors.Length);
        step.shape = Random.Range(0, shapes.Length);
        step.beep = Random.Range(0, 2);
        sequence.Add(step);
    }

    IEnumerator ShowSequence()
    {
        isShowingSequence = true;
        playerIndex = 0;
        SetButtonsInteractable(false);
        yield return new WaitForSeconds(1f);

        for (int i = 0; i < sequence.Count; i++)
        {
            yield return StartCoroutine(Flash(sequence[i]));
            yield return new WaitForSeconds(delayBetweenFlashes);
        }

        ResetDisplay();
        isShowingSequence = false;
        SetButtonsInteractable(true);
    }

    IEnumerator Flash(Step step)
    {
        // 1. Ses Çal
        if (audioSource != null && step.beep < beeps.Length)
        {
            audioSource.PlayOneShot(beeps[step.beep]);
        }

        // 2. Gerçek Renk ve Şekil
        if (displayImage != null)
        {
            Color c = colors[step.color];
            c.a = 1f;
            displayImage.color = c;
        }
        if (displayShape != null) displayShape.sprite = shapes[step.shape];

        // 3. Yanıltıcı Renk (Level 9 Mekaniği)
        if (fakeDisplayImage != null)
        {
            int fakeIndex = Random.Range(0, colors.Length);
            Color fc = colors[fakeIndex];
            fc.a = 1f;
            fakeDisplayImage.color = fc;
        }

        yield return new WaitForSeconds(flashDuration);
        ResetDisplay();
        yield return new WaitForSeconds(0.1f);
    }

    void ResetDisplay()
    {
        Color neutral = new Color(0.2f, 0.2f, 0.2f, 1f);
        if (displayImage != null) displayImage.color = neutral;
        if (fakeDisplayImage != null) fakeDisplayImage.color = neutral;
        if (displayShape != null) displayShape.sprite = null;
    }

    // Input Fonksiyonları
    public void OnColorPressed(int index) { if (!isShowingSequence) { selectedColor = index; CheckInput(); } }
    public void OnShapePressed(int index) { if (!isShowingSequence) { selectedShape = index; CheckInput(); } }
    public void OnBeepPressed(int index) { if (!isShowingSequence) { selectedBeep = index; CheckInput(); } }

    void CheckInput()
    {
        if (selectedColor == -1 || selectedShape == -1 || selectedBeep == -1)
            return;

        Step currentStep = sequence[playerIndex];

        if (currentStep.color == selectedColor && currentStep.shape == selectedShape && currentStep.beep == selectedBeep)
        {
            playerIndex++;
            selectedColor = -1;
            selectedShape = -1;
            selectedBeep = -1;

            if (playerIndex >= sequence.Count)
            {
                // SONSUZ MOD DÖNGÜSÜ: Seviye bitmiyor
                score += 10;
                endlessStage++;
                UpdateUI();

                AddNewStep();
                StartCoroutine(ShowSequence());
            }
        }
        else
        {
            GameOver();
        }
    }

    void SetButtonsInteractable(bool state)
    {
        foreach (Button b in colorButtons) b.interactable = state;
        foreach (Button b in shapeButtons) b.interactable = state;
        foreach (Button b in beepButtons) b.interactable = state;
    }

    void GameOver()
    {
        isShowingSequence = false;
        SetButtonsInteractable(false);

        // Oyuncu elendiği an doğrudan skor kaydı için isim alma panelini açıyoruz
        nameInputPanel.SetActive(true);
    }

    void OnSaveButtonClicked()
    {
        string pName = nameInputField.text;
        if (string.IsNullOrEmpty(pName)) pName = "Anonim";

        SaveToLocalLeaderboard(pName, score);
        nameInputPanel.SetActive(false);

        // --- TEMİZLİK ---
        if (colorButtonsPanel != null) colorButtonsPanel.SetActive(false);
        if (shapeButtonsPanel != null) shapeButtonsPanel.SetActive(false);
        if (beepButtonsPanel != null) beepButtonsPanel.SetActive(false);
        if (displayPanel != null) displayPanel.SetActive(false);
        if (InfoPanel != null) InfoPanel.SetActive(false);

        if (scoreText != null) scoreText.gameObject.SetActive(false);
        if (levelText != null) levelText.gameObject.SetActive(false);
        if (fakeDisplayImage != null) fakeDisplayImage.gameObject.SetActive(false);

        if (fakeText != null)
        {
            fakeText.text = "";
            fakeText.gameObject.SetActive(false);
        }

        resultPanel.SetActive(true);
        resultText.text = "<color=black><b>OYUN BİTTİ!\nUlaşılan Aşama: " + endlessStage + "\nToplam Puan: " + score + "</b></color>";

        LoadAndDisplayLeaderboard();
    }

    void SaveToLocalLeaderboard(string pName, int finalScore)
    {
        LeaderboardData data = new LeaderboardData();

        if (PlayerPrefs.HasKey(LEADERBOARD_KEY))
        {
            try
            {
                string encryptedJson = PlayerPrefs.GetString(LEADERBOARD_KEY);
                byte[] decryptedBytes = Convert.FromBase64String(encryptedJson);
                string oldJson = System.Text.Encoding.UTF8.GetString(decryptedBytes);

                data = JsonUtility.FromJson<LeaderboardData>(oldJson);
            }
            catch (Exception)
            {
                data = new LeaderboardData();
            }
        }

        ScoreEntry newEntry = new ScoreEntry
        {
            playerName = pName,
            score = finalScore,
            date = DateTime.Now.ToString("dd/MM/yyyy")
        };

        data.list.Add(newEntry);
        data.list.Sort((x, y) => y.score.CompareTo(x.score));

        if (data.list.Count > 10)
        {
            data.list.RemoveRange(10, data.list.Count - 10);
        }

        string newJson = JsonUtility.ToJson(data);
        byte[] bytesToEncrypt = System.Text.Encoding.UTF8.GetBytes(newJson);
        string encryptedBase64 = Convert.ToBase64String(bytesToEncrypt);

        PlayerPrefs.SetString(LEADERBOARD_KEY, encryptedBase64);
        PlayerPrefs.Save();
    }

    void LoadAndDisplayLeaderboard()
    {
        if (!PlayerPrefs.HasKey(LEADERBOARD_KEY)) return;

        LeaderboardData data;
        try
        {
            string encryptedJson = PlayerPrefs.GetString(LEADERBOARD_KEY);
            byte[] decryptedBytes = Convert.FromBase64String(encryptedJson);
            string json = System.Text.Encoding.UTF8.GetString(decryptedBytes);

            data = JsonUtility.FromJson<LeaderboardData>(json);
        }
        catch (Exception)
        {
            return;
        }

        string boardContent = "<color=black><b>TOP 10 EN YÜKSEK SKORLAR</b>\n\n";
        for (int i = 0; i < data.list.Count; i++)
        {
            boardContent += string.Format("<b>{0}. {1} — {2} Puan ({3})</b>\n",
                (i + 1),
                data.list[i].playerName,
                data.list[i].score,
                data.list[i].date);
        }
        boardContent += "</color>";

        leaderboardDisplayText.text = boardContent;
    }

    void UpdateUI()
    {
        scoreText.text = "Puan: " + score;
        levelText.text = "Aşama: " + endlessStage;
    }
}