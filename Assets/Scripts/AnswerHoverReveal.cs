using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class AnswerHoverReveal : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Level5Manager manager;
    public int optionIndex;

    private bool locked = false;
    private Coroutine hideCoroutine;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (locked || manager == null) return;

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        manager.RevealOption(optionIndex);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (locked || manager == null) return;

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }
        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(2f);

        if (!locked && manager != null)
        {
            manager.HideOption(optionIndex);
        }

        hideCoroutine = null;
    }

    public void Lock()
    {
        locked = true;

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
    }

    public void Unlock()
    {
        locked = false;
    }
}
