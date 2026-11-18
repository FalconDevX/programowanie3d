using UnityEngine;
using UnityEngine.InputSystem;

// Swobodna kamera:
// WASD / strzałki - ruch, Q / E - w dół / w górę, prawy przycisk myszy + ruch myszy - rozglądanie,
// Shift - szybciej, kółko myszy - zmiana prędkości
public class KameraSwobodna : MonoBehaviour
{
    public float predkosc = 12f;
    public float mnoznikShift = 3f;
    public float czuloscMyszy = 0.15f;

    float yaw, pitch;

    void Start()
    {
        Vector3 e = transform.eulerAngles;
        yaw = e.y;
        pitch = e.x > 180f ? e.x - 360f : e.x;
    }

    void Update()
    {
        var kb = Keyboard.current;
        var mysz = Mouse.current;

        // rozglądanie przy wciśniętym prawym przycisku myszy
        if (mysz != null)
        {
            if (mysz.rightButton.isPressed)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Vector2 d = mysz.delta.ReadValue();
                yaw += d.x * czuloscMyszy;
                pitch = Mathf.Clamp(pitch - d.y * czuloscMyszy, -89f, 89f);
                transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
            }
            else if (Cursor.lockState != CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.None;
            }

            float kolko = mysz.scroll.ReadValue().y;
            if (kolko != 0f)
                predkosc = Mathf.Clamp(predkosc * (kolko > 0f ? 1.15f : 1f / 1.15f), 1f, 200f);
        }

        if (kb == null) return;

        Vector3 kierunek = Vector3.zero;
        if (kb.wKey.isPressed || kb.upArrowKey.isPressed) kierunek += Vector3.forward;
        if (kb.sKey.isPressed || kb.downArrowKey.isPressed) kierunek += Vector3.back;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) kierunek += Vector3.left;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) kierunek += Vector3.right;

        float pion = 0f;
        if (kb.eKey.isPressed) pion += 1f;
        if (kb.qKey.isPressed) pion -= 1f;

        float v = predkosc * (kb.leftShiftKey.isPressed ? mnoznikShift : 1f) * Time.deltaTime;
        transform.position += (transform.rotation * kierunek.normalized + Vector3.up * pion) * v;
    }

    void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
    }
}
