using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Efekt najechania na przycisk: powiększenie + mocniejsza poświata (Bloom) + delikatne pulsowanie
public class EfektPrzycisku : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    public float skalaNajechania = 1.08f;
    public float jasnoscBazowa = 1.8f;
    public float jasnoscNajechania = 3.4f;
    public float predkosc = 12f;

    Graphic[] grafiki;
    Material[] materialy;
    bool najechany;
    float jasnosc;
    float faza;

    void Awake()
    {
        grafiki = GetComponentsInChildren<Graphic>();
        materialy = new Material[grafiki.Length];
        for (int i = 0; i < grafiki.Length; i++)
        {
            // własna kopia materiału, żeby każdy przycisk świecił niezależnie
            if (grafiki[i].material != null && grafiki[i].material.HasProperty("_Jasnosc"))
            {
                materialy[i] = new Material(grafiki[i].material);
                grafiki[i].material = materialy[i];
            }
        }
        jasnosc = jasnoscBazowa;
        faza = Random.value * 10f;
    }

    void Update()
    {
        float k = 1f - Mathf.Exp(-predkosc * Time.unscaledDeltaTime);
        float cel = najechany ? jasnoscNajechania : jasnoscBazowa + 0.15f * Mathf.Sin(Time.unscaledTime * 2f + faza);
        jasnosc = Mathf.Lerp(jasnosc, cel, k);
        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * (najechany ? skalaNajechania : 1f), k);
        foreach (var m in materialy)
            if (m != null) m.SetFloat("_Jasnosc", jasnosc);
    }

    public void OnPointerEnter(PointerEventData e) => najechany = true;
    public void OnPointerExit(PointerEventData e) => najechany = false;
    public void OnSelect(BaseEventData e) => najechany = true;
    public void OnDeselect(BaseEventData e) => najechany = false;

    void OnDestroy()
    {
        foreach (var m in materialy)
            if (m != null) Destroy(m);
    }
}
