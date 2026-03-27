using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager1 : MonoBehaviour
{
    public void OyunaBasla()
    {
        SceneManager.LoadScene("Level1");
    }
    
    public void OyundanCik()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void Settings()
    {
        SceneManager.LoadScene("Settings Menu");
    }
    public void Seviyeler()
    {
        SceneManager.LoadScene("LevelSelect");
    }
}
