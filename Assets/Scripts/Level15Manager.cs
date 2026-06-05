using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Level15Manager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text scoreText;
    public TMP_Text levelText;
    public GameObject resultPanel;
    public TMP_Text resultText;
    public Button retryButton;
    public Button mainMenuButton;
    public Button nextLevelButton; // Seviye 16'ya geçiş için eklendi
    public GameObject InfoPanel;

    [Header("Display (Sabit Kalacaklar)")]
    public Image displayImage;
    public Image displayShape;
    public Image fakeDisplayImage;
    public Image fakeDisplayShape;

    [Header("Buttons (Yüzecek Olanlar)")]
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

    [Header("Level 15: Chaos Settings")]
    public float fadeSpeed = 0.15f;
    public float floatSpeed = 1.5f;     // Yüzme hızı
    public float floatAmount = 15f;    // Dalgalanma yarıçapı
    public float rotationSpeed = 30f;   // Kendi etrafında dönme hızı

    private List<Step> sequence = new List<Step>();
    private int playerIndex = 0;
    private int level = 15;
    private int score = 0;

    private int selectedColor = -1;
    private int selectedShape = -1;
    private int selectedBeep = -1;

    private bool isShowingSequence = false;
    private bool gameActive = false;
    private float currentAlpha = 1f;

    private Dictionary<Transform, Vector3> initialPositions = new Dictionary<Transform, Vector3>();

    class Step { public int color; public int shape; public int beep; }

    void Start()
    {
        resultPanel.SetActive(false);
        retryButton.onClick.AddListener(() => SceneManager.LoadScene("Level15"));
        mainMenuButton.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));

        // --- SONRAKİ SEVİYE BUTON KONTROLÜ (SEVİYE 16 İÇİN) ---
        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.RemoveAllListeners();
            nextLevelButton.onClick.AddListener(OnNextLevelPressed);
            CheckNextLevelLock(); // Sahne ilk açıldığında kilit durumuna göre aktif/pasif yapar
        }

        // Butonların Layout Group tarafından ilk dizildiği düzgün pozisyonları kaydet
        SaveInitialPositions(colorButtons);
        SaveInitialPositions(shapeButtons);
        SaveInitialPositions(beepButtons);

        UpdateUI();
    }

    void CheckNextLevelLock()
    {
        int reachedLevel = PlayerPrefs.GetInt("ReachedLevel", 1);
        int nextLevelNumber = level + 1; // Seviye 15 için sonraki seviye 16'dır

        if (reachedLevel >= nextLevelNumber)
        {
            nextLevelButton.interactable = true;
        }
        else
        {
            nextLevelButton.interactable = false; // İlk oynamada kilitli başlar
        }
    }

    void OnNextLevelPressed()
    {
        SceneManager.LoadScene("Level16"); // Doğrudan sonsuz sahneye geçiş yapar
    }

    void SaveInitialPositions(Button[] buttons)
    {
        foreach (Button b in buttons)
        {
            if (b != null)
            {
                initialPositions[b.transform] = b.transform.localPosition;
            }
        }
    }

    void Update()
    {
        if (gameActive && !isShowingSequence)
        {
            // 1. Şeffaflık Mekanizması
            currentAlpha -= fadeSpeed * Time.deltaTime;
            currentAlpha = Mathf.Clamp(currentAlpha, 0.05f, 1f);
            SetButtonsAlpha(currentAlpha);

            // 2. Yüzme ve Dönme Mekanizması
            FloatAndRotateButtons(colorButtons);
            FloatAndRotateButtons(shapeButtons);
            FloatAndRotateButtons(beepButtons);
        }
    }

    void FloatAndRotateButtons(Button[] buttons)
    {
        foreach (Button b in buttons)
        {
            if (b == null) continue;

            if (initialPositions.ContainsKey(b.transform))
            {
                Vector3 origin = initialPositions[b.transform];

                // Orijinal dizildikleri konum merkez alınarak yumuşak dalgalanma yapılır
                float offsetX = Mathf.Sin(Time.time * floatSpeed + origin.x) * floatAmount;
                float offsetY = Mathf.Cos(Time.time * floatSpeed + origin.y) * floatAmount;

                b.transform.localPosition = origin + new Vector3(offsetX, offsetY, 0);
            }

            // Kendi etrafında dönme
            b.transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
        }
    }

    public void OnStartButtonPressed() => StartGame();

    void StartGame()
    {
        InfoPanel.SetActive(false);
        if (colors.Length == 0 || shapes.Length == 0 || beeps.Length < 2) return;

        // --- Layout kontrolünü kaldır ama konumları bozma ---
        FreeButtonsFromLayouts();

        score = 0;
        sequence.Clear();
        gameActive = true;
        AddNewStep();
        StartCoroutine(ShowSequence());
        UpdateUI();
    }

    // Butonların ekrandaki dizilimini bozmadan
    void FreeButtonsFromLayouts()
    {
        Button[][] allGroups = { colorButtons, shapeButtons, beepButtons };
        foreach (var group in allGroups)
        {
            if (group != null && group.Length > 0 && group[0] != null)
            {
                // Ebeveyndeki Layout Group bileşenini bul
                LayoutGroup layout = group[0].transform.parent.GetComponent<LayoutGroup>();
                if (layout != null)
                {
                    if (layout is HorizontalOrVerticalLayoutGroup)
                    {
                        var horVerLayout = (HorizontalOrVerticalLayoutGroup)layout;
                        horVerLayout.childControlWidth = false;
                        horVerLayout.childControlHeight = false;
                        horVerLayout.childForceExpandWidth = false;
                        horVerLayout.childForceExpandHeight = false;
                    }
                    else if (layout is GridLayoutGroup)
                    {
                        
                        UpdateInitialPositions();
                        layout.enabled = false;
                    }
                }
            }
        }
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
        ResetButtonRotations();
        ResetToInitialPositions();

        yield return new WaitForSeconds(1f);

        for (int i = 0; i < sequence.Count; i++)
        {
            yield return StartCoroutine(Flash(sequence[i]));
            yield return new WaitForSeconds(delayBetweenFlashes);
        }

        ResetDisplay();

        // Karıştırma
        ShuffleButtons(colorButtons);
        ShuffleButtons(shapeButtons);
        ShuffleButtons(beepButtons);

        // Karıştırmadan sonra butonların yeni kazandığı koordinatları "merkez" olarak kaydet
        yield return new WaitForEndOfFrame(); // Unity'nin arayüzü yeniden dizmesi için 1 kare bekle
        UpdateInitialPositions();

        isShowingSequence = false;
        SetButtonsInteractable(true);
    }

    void ResetButtonRotations()
    {
        Button[][] allGroups = { colorButtons, shapeButtons, beepButtons };
        foreach (var group in allGroups)
        {
            foreach (var b in group)
            {
                if (b != null) b.transform.localRotation = Quaternion.identity;
            }
        }
    }

    void ResetToInitialPositions()
    {
        Button[][] allGroups = { colorButtons, shapeButtons, beepButtons };
        foreach (var group in allGroups)
        {
            foreach (var b in group)
            {
                if (b != null && initialPositions.ContainsKey(b.transform))
                {
                    b.transform.localPosition = initialPositions[b.transform];
                }
            }
        }
    }

    void UpdateInitialPositions()
    {
        initialPositions.Clear();
        SaveInitialPositions(colorButtons);
        SaveInitialPositions(shapeButtons);
        SaveInitialPositions(beepButtons);
    }

    void ShuffleButtons(Button[] buttons)
    {
        if (buttons == null) return;
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null)
            {
                buttons[i].transform.SetSiblingIndex(Random.Range(0, buttons.Length));
            }
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
        if (audioSource != null && step.beep < beeps.Length) audioSource.PlayOneShot(beeps[step.beep]);

        if (displayImage != null)
        {
            Color c = colors[step.color];
            c.a = 1f;
            displayImage.color = c;
        }

        if (displayShape != null) displayShape.sprite = shapes[step.shape];

        if (fakeDisplayImage != null)
        {
            Color fc = colors[Random.Range(0, colors.Length)];
            fc.a = 1f;
            fakeDisplayImage.color = fc;
        }

        if (fakeDisplayShape != null) fakeDisplayShape.sprite = shapes[Random.Range(0, shapes.Length)];

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

            currentAlpha = Mathf.Min(currentAlpha + 0.2f, 1f);

            if (playerIndex >= sequence.Count)
            {
                score += 15;
                UpdateUI();
                if (sequence.Count < 5)
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
        Button[][] allGroups = { colorButtons, shapeButtons, beepButtons };
        foreach (var group in allGroups)
        {
            foreach (var b in group)
            {
                if (b != null) b.interactable = state;
            }
        }
    }

    void LevelCompleted()
    {
        gameActive = false;
        ResetButtonRotations();
        ResetToInitialPositions();

        int reachedLevel = PlayerPrefs.GetInt("ReachedLevel", 1);
        int currentLevelNum = 15;

        if (reachedLevel <= currentLevelNum)
        {
            PlayerPrefs.SetInt("ReachedLevel", currentLevelNum + 1); // ReachedLevel'i 16 yapar
            PlayerPrefs.Save();
        }

        // Seviye ilk kez bitiriliyor olsa bile buton panelle birlikte anında aktifleşir
        if (nextLevelButton != null)
        {
            nextLevelButton.interactable = true;
        }

        resultPanel.SetActive(true);
        resultText.text = "ŞAMPİYON! TÜM OYUNU TAMAMLADIN.";
    }

    void GameOver()
    {
        gameActive = false;
        ResetButtonRotations();
        ResetToInitialPositions();
        resultPanel.SetActive(true);
        resultText.text = "FİNALDE ELENDİN!";
    }

    void UpdateUI() { scoreText.text = "Puan: " + score; levelText.text = "Level: " + level; }
}