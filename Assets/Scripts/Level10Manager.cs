using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Level10Manager : MonoBehaviour
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
    public Image fakeDisplayImage;  // Yanıltıcı renk (Level 9 mekaniği)

    [Header("Buttons")]
    public Button[] colorButtons;
    public Button[] shapeButtons;
    public Button[] beepButtons;    // Bip sesi seçim butonları (1 Bip ve 2 Bip)

    [Header("Assets")]
    public Sprite[] shapes;
    public Color[] colors;
    public AudioClip[] beeps;       // Index 0 = 1 Bip, Index 1 = 2 Bip sesi
    public AudioSource audioSource;

    [Header("Settings")]
    public float flashDuration = 0.45f;
    public float delayBetweenFlashes = 0.3f;

    class Step
    {
        public int color;
        public int shape;
        public int beep; // 0: 1 Bip, 1: 2 Bip
    }

    private List<Step> sequence = new List<Step>();
    private int playerIndex = 0;
    private int level = 10; // Bu sahnenin seviyesi
    private int score = 0;

    private int selectedColor = -1;
    private int selectedShape = -1;
    private int selectedBeep = -1; // Seçilen ses

    private bool isShowingSequence = false;

    void Start()
    {
        resultPanel.SetActive(false);

        retryButton.onClick.AddListener(() => SceneManager.LoadScene("Level10"));
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
        int nextLevelNumber = level + 1; // Seviye 10 için sonraki seviye 11'dir

        // Eğer sonraki seviye (Level 11) halihazırda önceden açılmışsa buton aktiftir
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
        SceneManager.LoadScene("Level" + (level + 1)); // Doğrudan Level11 sahnesini yükler
    }

    public void OnStartButtonPressed() => StartGame();

    void StartGame()
    {
        InfoPanel.SetActive(false);
        if (colors.Length == 0 || shapes.Length == 0 || beeps.Length < 2)
        {
            Debug.LogError("Gerekli assetler (Color, Shape, Beeps) eksik!");
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
        // 1. Ses Çal (Renk ve Şekil ile aynı anda)
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
                score += 10;
                UpdateUI();
                if (sequence.Count < 3)
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
        foreach (Button b in colorButtons) b.interactable = state;
        foreach (Button b in shapeButtons) b.interactable = state;
        foreach (Button b in beepButtons) b.interactable = state;
    }

    void LevelCompleted()
    {
        int reachedLevel = PlayerPrefs.GetInt("ReachedLevel", 1);
        int currentLevelNum = 10;

        if (reachedLevel <= currentLevelNum)
        {
            PlayerPrefs.SetInt("ReachedLevel", currentLevelNum + 1); // Seviye 11 kilidini açar
            PlayerPrefs.Save();
            Debug.Log("Sistem: Seviye 11 kilidi başarıyla açıldı ve kaydedildi.");
        }

        // Seviye ilk kez bitiriliyor olsa bile buton panelle birlikte anında aktifleşir
        if (nextLevelButton != null)
        {
            nextLevelButton.interactable = true;
        }

        resultPanel.SetActive(true);
        resultText.text = "Mükemmel! Seviye " + currentLevelNum + " Tamamlandı.";
    }

    void GameOver()
    {
        // Seviye 11 önceden açıldıysa aktif kalır, açılmadıysa pasif kalır.
        resultPanel.SetActive(true);
        resultText.text = "Hatalı Seçim! Oyun Bitti.";
    }

    void UpdateUI() { scoreText.text = "Puan: " + score; levelText.text = "Level: " + level; }
}