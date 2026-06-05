using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public Button[] levelButtons;

    void Start()
    {
        // PlayerPrefs'ten veriyi çek
        int reachedLevel = PlayerPrefs.GetInt("ReachedLevel", 1);
        Debug.Log("Sistem Baþlatýldý. Kayýtlý Seviye: " + reachedLevel);

        // Butonlarý tek tek kontrol et
        for (int i = 0; i < levelButtons.Length; i++)
        {
            if (levelButtons[i] == null)
            {
                Debug.LogWarning("DÝKKAT: " + i + ". elementteki buton boþ (None)!");
                continue;
            }

            //  Eðer butonun sýrasý, ulaþýlan seviyeden küçükse veya eþitse AÇ
            if (i + 1 <= reachedLevel)
            {
                levelButtons[i].interactable = true;
                levelButtons[i].image.color = Color.white; // Görünür yap
                Debug.Log("Buton Açýldý: " + levelButtons[i].name);
            }
            else
            {
                levelButtons[i].interactable = false;
                // Kilitli olduðunu belli etmek için rengi karart
                levelButtons[i].image.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            }
        }
    }
    public void AnaMenuyeDon()
    {

        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
    public void OpenLevel(int levelIndex)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(levelIndex);
    }
    
}