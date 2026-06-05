using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Level7Manager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text scoreText;
    public TMP_Text levelText;
    public GameObject resultPanel;
    public TMP_Text resultText;
    public Button retryButton;
    public Button mainMenuButton;
    public Button nextLevelButton; // Sonraki seviye kontrolü için eklendi
    public GameObject InfoPanel;

    [Header("Display")]
    public Image displayImage;
    public Image displayShape;

    [Header("Buttons")]
    public Button[] colorButtons;
    public Button[] shapeButtons;

    [Header("Shapes")]
    public Sprite[] shapes;

    [Header("Colors")]
    public Color[] colors;

    public float flashDuration = 0.45f;
    public float delayBetweenFlashes = 0.2f;

    class Step
    {
        public int color;
        public int shape;
    }

    private List<Step> sequence = new List<Step>();

    private int playerIndex = 0;
    private int level = 7; // Bu sahnenin seviyesi
    private int score = 0;

    private int selectedColor = -1;
    private int selectedShape = -1;

    private bool isShowingSequence = false;

    void Start()
    {
        resultPanel.SetActive(false);

        retryButton.onClick.AddListener(() => SceneManager.LoadScene("Level7"));
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
        int nextLevelNumber = level + 1; // Seviye 7 için sonraki seviye 8'dir

        // Eğer sonraki seviye (Level 8) halihazırda önceden açılmışsa buton aktiftir
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
        SceneManager.LoadScene("Level" + (level + 1)); // Doğrudan Level8 sahnesini yükler
    }

    public void OnStartButtonPressed()
    {
        StartGame();
    }

    void StartGame()
    {
        InfoPanel.SetActive(false);

        if (colors.Length == 0 || shapes.Length == 0)
        {
            Debug.LogError("Colors veya Shapes dizisi boş! Inspector’dan doldurulmalı.");
            return;
        }

        score = 0;
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
        if (displayImage != null && step.color < colors.Length)
        {
            Color c = colors[step.color];
            c.a = 1f;
            displayImage.color = c;
            displayImage.gameObject.SetActive(true);
        }

        if (displayShape != null && step.shape < shapes.Length)
        {
            displayShape.sprite = shapes[step.shape];
            displayShape.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(flashDuration);

        ResetDisplay();

        yield return new WaitForSeconds(0.1f);
    }

    void ResetDisplay()
    {
        if (displayImage != null)
        {
            displayImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        }
        if (displayShape != null)
        {
            displayShape.sprite = null;
        }
    }

    public void OnColorPressed(int index)
    {
        if (isShowingSequence) return;

        selectedColor = index;
        CheckInput();
    }

    public void OnShapePressed(int index)
    {
        if (isShowingSequence) return;

        selectedShape = index;
        CheckInput();
    }

    void CheckInput()
    {
        if (selectedColor == -1 || selectedShape == -1)
            return;

        Step step = sequence[playerIndex];

        if (step.color == selectedColor && step.shape == selectedShape)
        {
            playerIndex++;
            selectedColor = -1;
            selectedShape = -1;

            if (playerIndex >= sequence.Count)
            {
                score += 8;
                UpdateUI();

                if (sequence.Count < 4)
                {
                    AddNewStep();
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
        foreach (Button btn in colorButtons)
            btn.interactable = state;

        foreach (Button btn in shapeButtons)
            btn.interactable = state;
    }

    void LevelCompleted()
    {
        int reachedLevel = PlayerPrefs.GetInt("ReachedLevel", 1);
        int currentLevelNum = 7;

        if (reachedLevel <= currentLevelNum)
        {
            PlayerPrefs.SetInt("ReachedLevel", currentLevelNum + 1); // 8. seviyeyi açar
            PlayerPrefs.Save();
            Debug.Log("Tebrikler! Seviye 8 kilidi açıldı.");
        }

        // İlk kez oynanıyor olsa bile seviye tamamlandığı an buton aktif hale gelir
        if (nextLevelButton != null)
        {
            nextLevelButton.interactable = true;
        }

        resultPanel.SetActive(true);
        resultText.text = "Tebrikler! Seviye " + currentLevelNum + " Tamamlandı.";
    }

    void GameOver()
    {
        // Seviye 8 önceden açıldıysa aktif kalır, açılmadıysa pasif kalır.
        resultPanel.SetActive(true);
        resultText.text = "Oyun Bitti!";
    }

    void UpdateUI()
    {
        scoreText.text = "Puan: " + score;
        levelText.text = "Level: " + level;
    }
}