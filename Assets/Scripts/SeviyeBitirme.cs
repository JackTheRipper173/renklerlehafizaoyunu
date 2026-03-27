using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SeviyeBitirme : MonoBehaviour
{
    public void seviye1bitir()
    {
        SeviyelerYonetici.seviye2 = true; // Bir sonraki seviyenin kilidini açar
        SceneManager.LoadScene(1);       // Seviye seçim ekranýna döner
    }

    public void seviye2bitir()
    {
        SeviyelerYonetici.seviye3 = true;
        SceneManager.LoadScene(1);
    }
    public void seviye3bitir()
    {
        SeviyelerYonetici.seviye4 = true;
        SceneManager.LoadScene(1);
    }
    public void seviye4bitir()
    {
        SeviyelerYonetici.seviye5 = true;
        SceneManager.LoadScene(1);
    }
    public void seviye5bitir()
    {
        SeviyelerYonetici.seviye6 = true;
        SceneManager.LoadScene(1);
    }
    public void seviye6bitir()
    {
        SeviyelerYonetici.seviye7 = true;
        SceneManager.LoadScene(1);
    }
    public void seviye7bitir()
    {
        SeviyelerYonetici.seviye8 = true;
        SceneManager.LoadScene(1);
    }
    public void seviye8bitir()
    {
        SeviyelerYonetici.seviye9 = true;
        SceneManager.LoadScene(1);
    }
    public void seviye9bitir()
    {
        SeviyelerYonetici.seviye10 = true;
        SceneManager.LoadScene(1);
    }
   
}