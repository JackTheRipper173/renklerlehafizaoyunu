using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Level14Manager : MonoBehaviour
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
    public Image displayImage;      // Gerçek renk kutusu
    public Image displayShape;      // Gerçek şekil kutusu
    public Image fakeDisplayImage;  // Yanıltıcı renk kutusu
    public Image fakeDisplayShape;  // Yanıltıcı şekil kutusu

    [Header("Buttons")]
    public Button[] colorButtons;
    public Button[] shapeButtons;
    public Button[] beepButtons;

    [Header("Assets")]
    public Sprite[] shapes;
    public Color[] colors;
    public AudioClip[] beeps;
    public AudioSource audioSource;

    [Header("Settings")]
    public float flashDuration = 0.45f;
    public float delayBetweenFlashes = 0.3f;

    [Header("Level 14: Fade Effect")]
    [Tooltip("Butonların saniyede ne kadar şeffaflaşacağını belirler")]
    public float fadeSpeed = 0.15f;

    private List<Step> sequence = new List<Step>();
    private int playerIndex = 0;
    private int level = 14; // Bu sahnenin seviyesi
    private int score = 0;

    private int selectedColor = -1;
    private int selectedShape = -1;
    private int selectedBeep = -1;

    private bool isShowingSequence = false;
    private bool gameActive = false;
    private float currentAlpha = 1f;

    class Step
    {
        public int color;
        public int shape;
        public int beep;
    }

    void Start()
    {
        resultPanel.SetActive(false);
        retryButton.onClick.AddListener(() => SceneManager.LoadScene("Level14"));
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
        int nextLevelNumber = level + 1; // Seviye 14 için sonraki seviye 15'tir (Büyük Final)

        // Eğer büyük final (Level 15) halihazırda önceden açılmışsa buton aktiftir
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
        SceneManager.LoadScene("Level" + (level + 1)); // Doğrudan Level15 sahnesini yükler
    }

    void Update()
    {
        // Seviye 13'ten gelen şeffaflaşma mekanizması
        if (gameActive && !isShowingSequence)
        {
            currentAlpha -= fadeSpeed * Time.deltaTime;
            currentAlpha = Mathf.Clamp(currentAlpha, 0.05f, 1f);
            SetButtonsAlpha(currentAlpha);
        }
    }

    public void OnStartButtonPressed() => StartGame();

    void StartGame()
    {
        InfoPanel.SetActive(false);
        if (colors.Length == 0 || shapes.Length == 0 || beeps.Length < 2) return;

        score = 0;
        sequence.Clear();
        gameActive = true;
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

        currentAlpha = 1f;
        SetButtonsAlpha(1f);

        yield return new WaitForSeconds(1f);

        for (int i = 0; i < sequence.Count; i++)
        {
            yield return StartCoroutine(Flash(sequence[i]));
            yield return new WaitForSeconds(delayBetweenFlashes);
        }

        ResetDisplay();

        // Seviye 12'den gelen Shuffle mekanizması
        ShuffleButtons(colorButtons);
        ShuffleButtons(shapeButtons);

        isShowingSequence = false;
        SetButtonsInteractable(true);
    }

    void ShuffleButtons(Button[] buttons)
    {
        if (buttons == null) return;
        for (int i = 0; i < buttons.Length; i++)
        {
            int randomIndex = Random.Range(0, buttons.Length);
            buttons[i].transform.SetSiblingIndex(randomIndex);
        }
    }

    void SetButtonsAlpha(float alpha)
    {
        ApplyAlphaToGroup(colorButtons, alpha);
        ApplyAlphaToGroup(shapeButtons, alpha);
        ApplyAlphaToGroup(beepButtons, alpha);
    }

    void ApplyAlphaToGroup(Button[] buttons, float alpha)
    {
        foreach (Button btn in buttons)
        {
            if (btn != null)
            {
                Color c = btn.image.color;
                c.a = alpha;
                btn.image.color = c;
            }
        }
    }

    IEnumerator Flash(Step step)
    {
        // 1. Ses Çal
        if (audioSource != null && step.beep < beeps.Length)
            audioSource.PlayOneShot(beeps[step.beep]);

        // 2. Gerçek Uyaranlar
        if (displayImage != null)
        {
            Color c = colors[step.color];
            c.a = 1f;
            displayImage.color = c;
        }
        if (displayShape != null) displayShape.sprite = shapes[step.shape];

        // 3. TAM YANILTICI
        if (fakeDisplayImage != null)
        {
            int fakeColorIndex = Random.Range(0, colors.Length);
            Color fc = colors[fakeColorIndex];
            fc.a = 1f;
            fakeDisplayImage.color = fc;
        }
        if (fakeDisplayShape != null)
        {
            int fakeShapeIndex = Random.Range(0, shapes.Length);
            fakeDisplayShape.sprite = shapes[fakeShapeIndex];
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
        if (fakeDisplayShape != null) fakeDisplayShape.sprite = null;
    }

    public void OnColorPressed(int index) { if (!isShowingSequence) { selectedColor = index; CheckInput(); } }
    public void OnShapePressed(int index) { if (!isShowingSequence) { selectedShape = index; CheckInput(); } }
    public void OnBeepPressed(int index) { if (!isShowingSequence) { selectedBeep = index; CheckInput(); } }

    void CheckInput()
    {
        if (selectedColor == -1 || selectedShape == -1 || selectedBeep == -1) return;

        Step currentStep = sequence[playerIndex];
        if (currentStep.color == selectedColor && currentStep.shape == selectedShape && currentStep.beep == selectedBeep)
        {
            playerIndex++;
            selectedColor = -1;
            selectedShape = -1;
            selectedBeep = -1;

            currentAlpha = Mathf.Min(currentAlpha + 0.25f, 1f);

            if (playerIndex >= sequence.Count)
            {
                score += 14;
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
        else GameOver();
    }

    void SetButtonsInteractable(bool state)
    {
        foreach (Button b in colorButtons) b.interactable = state;
        foreach (Button b in shapeButtons) b.interactable = state;
        foreach (Button b in beepButtons) b.interactable = state;
    }

    void LevelCompleted()
    {
        gameActive = false;

        int reachedLevel = PlayerPrefs.GetInt("ReachedLevel", 1);
        int currentLevelNum = 14;

        if (reachedLevel <= currentLevelNum)
        {
            PlayerPrefs.SetInt("ReachedLevel", currentLevelNum + 1); // Seviye 15 kilidini açar
            PlayerPrefs.Save();
            Debug.Log("Sistem: BÜYÜK FİNAL! Seviye 15 kilidi başarıyla açıldı.");
        }

        // Seviye ilk kez bitiriliyor olsa bile buton panelle birlikte anında aktifleşir
        if (nextLevelButton != null)
        {
            nextLevelButton.interactable = true;
        }

        resultPanel.SetActive(true);
        resultText.text = "MUHTEŞEM! SEVİYE " + currentLevelNum + " BİTTİ.";
    }

    void GameOver()
    {
        // Seviye 15 önceden açıldıysa aktif kalır, açılmadıysa pasif kalır.
        gameActive = false;
        resultPanel.SetActive(true);
        resultText.text = "DİKKAT DAĞILDI! OYUN BİTTİ.";
    }

    void UpdateUI() { scoreText.text = "Puan: " + score; levelText.text = "Level: " + level; }
}