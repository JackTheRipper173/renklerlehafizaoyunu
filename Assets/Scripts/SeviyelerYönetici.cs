using UnityEngine;
using UnityEngine.UI;

public class SeviyelerYonetici : MonoBehaviour
{
    // Statik değişkenler sayesinde bu verilere her yerden erişilebilir
    public static bool seviye1, seviye2, seviye3, seviye4, seviye5, seviye6, seviye7, seviye8, seviye9, seviye10;

    [Header("Seviye Butonları")]
    public Button btn1, btn2, btn3, btn4, btn5, btn6, btn7, btn8, btn9, btn10;

    private void Start()
    {
        seviye1 = true;
    }
    void Update()
    {
        // Eğer ilgili bool true olmuşsa butonu tıklanabilir yap
        if (seviye2==true) btn2.interactable = true;
        if (seviye3 == true) btn3.interactable = true;
        if (seviye4 == true) btn4.interactable = true;
        if (seviye5 == true) btn5.interactable = true;
        if (seviye6 == true) btn6.interactable = true;
        if (seviye7 == true) btn7.interactable = true;
        if (seviye8 == true) btn8.interactable = true;
        if (seviye9 == true) btn9.interactable = true;
        if (seviye10 == true) btn10.interactable = true;
    }
}