using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Obsługa przejść między scenami (indeksy jak w File > Build Profiles):
// 0 - Menu, 1 - Gra, 2 - Instrukcja, 3 - KoniecGry
public class SManager : MonoBehaviour
{
    public void IdzDoSceny()
    {
        SceneManager.LoadScene(1);
    }

    public void Wyjdz()
    {
        Application.Quit();
#if UNITY_EDITOR
        // Application.Quit nie działa w edytorze - zatrzymujemy tryb Play
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void IdzInstrukca()
    {
        SceneManager.LoadScene(2);
    }

    public void IdzMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void IdzKoniecGry()
    {
        SceneManager.LoadScene(3);
    }
}
