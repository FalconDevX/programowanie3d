using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Tymczasowa logika sceny gry (do czasu dodania platform i gracza):
// ESC przechodzi do ekranu końca gry, żeby dało się przetestować wszystkie sceny.
public class GraTymczasowa : MonoBehaviour
{
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            SceneManager.LoadScene(3);
    }
}
