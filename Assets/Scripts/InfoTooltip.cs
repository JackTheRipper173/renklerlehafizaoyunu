using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class InfoTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Bilgi Paneli")]
    public GameObject infoPanel;          // Panel objesi
    public TMPro.TextMeshProUGUI titleText; // Baþlýk Text
    public TMPro.TextMeshProUGUI descriptionText; // Açýklama Text

    [Header("Mesajlar")]
    [TextArea] public string titleMessage;
    [TextArea] public string descriptionMessage;

    [Header("Fade Ayarlarý")]
    public float fadeDuration = 0.3f;

    private CanvasGroup canvasGroup;

    void Start()
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(true);
            canvasGroup = infoPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = infoPanel.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            infoPanel.SetActive(false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (infoPanel != null)
        {
            if (titleText != null) titleText.text = titleMessage;
            if (descriptionText != null) descriptionText.text = descriptionMessage;

            infoPanel.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(FadeCanvasGroup(canvasGroup, canvasGroup.alpha, 1f, fadeDuration));
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (infoPanel != null)
        {
            StopAllCoroutines();
            StartCoroutine(FadeOutAndDisable(canvasGroup, fadeDuration));
        }
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }
        cg.alpha = end;
    }

    private IEnumerator FadeOutAndDisable(CanvasGroup cg, float duration)
    {
        yield return FadeCanvasGroup(cg, cg.alpha, 0f, duration);
        infoPanel.SetActive(false);
    }
}
