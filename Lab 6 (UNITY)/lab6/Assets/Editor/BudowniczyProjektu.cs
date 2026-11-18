using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

// Generator scen "Kosmiczny Biegacz" (Lab 6 - menu gry z obsługą scen).
// Menu edytora: Lab6 > Zbuduj sceny  /  Lab6 > Zbuduj grę (Linux)
// Batch: Unity -batchmode -executeMethod BudowniczyProjektu.WszystkoBatch
public static class BudowniczyProjektu
{
    const string KatScen = "Assets/Scenes";
    const string KatMat = "Assets/Materialy";
    static readonly string[] Sceny =
    {
        KatScen + "/Menu.unity",        // 0
        KatScen + "/Gra.unity",         // 1
        KatScen + "/Instrukcja.unity",  // 2
        KatScen + "/KoniecGry.unity",   // 3
    };

    static Font czcionkaBold, czcionkaReg;
    static Sprite ramkaPrzycisku, tloMenu, gradient;
    static Material matUI;

    static readonly Color Neon = new Color(0.25f, 0.7f, 1f);   // niebieski neon

    // ------------------------------------------------------------------ wejście

    [MenuItem("Lab6/Zbuduj sceny")]
    public static void ZbudujSceny()
    {
        PrzygotujZasoby();
        ScenaMenu();
        ScenaGra();
        ScenaInstrukcja();
        ScenaKoniec();

        EditorBuildSettings.scenes = System.Array.ConvertAll(Sceny, s => new EditorBuildSettingsScene(s, true));
        PlayerSettings.productName = "Space Runner";
        PlayerSettings.companyName = "WIMIiP";
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.defaultScreenWidth = 1600;
        PlayerSettings.defaultScreenHeight = 900;
        PlayerSettings.resizableWindow = true;

        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene(Sceny[0]);
        Debug.Log("[Lab6] Sceny zbudowane.");
    }

    [MenuItem("Lab6/Zbuduj grę (Linux)")]
    public static bool ZbudujGre()
    {
        var opcje = new BuildPlayerOptions
        {
            scenes = Sceny,
            locationPathName = "Build/Linux/KosmicznyBiegacz.x86_64",
            target = BuildTarget.StandaloneLinux64,
            options = BuildOptions.None,
        };
        var raport = BuildPipeline.BuildPlayer(opcje);
        Debug.Log($"[Lab6] Build: {raport.summary.result}, błędy: {raport.summary.totalErrors}, rozmiar: {raport.summary.totalSize / 1048576f:F1} MB");
        return raport.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded;
    }

    public static void WszystkoBatch()
    {
        ZbudujSceny();
        var kat = System.Environment.GetEnvironmentVariable("LAB6_PODGLADY");
        if (!string.IsNullOrEmpty(kat)) Podglady(kat);
        bool ok = System.Environment.GetEnvironmentVariable("LAB6_BEZ_BUILDU") == "1" || ZbudujGre();
        EditorSceneManager.OpenScene(Sceny[0]);
        EditorApplication.Exit(ok ? 0 : 1);
    }

    // Renderuje kamerę każdej sceny do PNG (podgląd bez uruchamiania gry)
    public static void Podglady(string katalog)
    {
        Directory.CreateDirectory(katalog);
        foreach (var s in Sceny)
        {
            EditorSceneManager.OpenScene(s);
            var cam = Object.FindAnyObjectByType<Camera>();
            var rt = new RenderTexture(1600, 900, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            cam.targetTexture = rt;
            Canvas.ForceUpdateCanvases();
            cam.Render();
            Canvas.ForceUpdateCanvases();
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(1600, 900, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0);
            tex.Apply();
            File.WriteAllBytes(Path.Combine(katalog, Path.GetFileNameWithoutExtension(s) + ".png"), tex.EncodeToPNG());
            RenderTexture.active = null;
            cam.targetTexture = null;
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(tex);
        }
    }

    // ------------------------------------------------------------------ zasoby

    static void PrzygotujZasoby()
    {
        Directory.CreateDirectory(KatScen);
        Directory.CreateDirectory(KatMat);

        // sprzątanie szablonu URP
        AssetDatabase.DeleteAsset(KatScen + "/SampleScene.unity");
        AssetDatabase.DeleteAsset("Assets/Readme.asset");
        AssetDatabase.DeleteAsset("Assets/TutorialInfo");

        ImportujSprite("Assets/Grafika/RamkaPrzycisku.png", new Vector4(44, 44, 44, 44));
        ImportujSprite("Assets/Grafika/Gradient.png", Vector4.zero);
        ImportujSprite("Assets/Grafika/TloMenu.jpg", Vector4.zero, 4096);

        ramkaPrzycisku = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Grafika/RamkaPrzycisku.png");
        tloMenu = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Grafika/TloMenu.jpg");
        gradient = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Grafika/Gradient.png");
        czcionkaBold = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/NotoSans-Bold.ttf");
        czcionkaReg = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/NotoSans-Regular.ttf");

        matUI = Mat("UI_Jasny", "Lab6/UIJasny", m => m.SetFloat("_Jasnosc", 1.8f));
    }

    static void ImportujSprite(string sciezka, Vector4 ramka, int maxRozmiar = 2048)
    {
        var ti = (TextureImporter)AssetImporter.GetAtPath(sciezka);
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.spriteBorder = ramka;
        ti.alphaIsTransparency = true;
        ti.mipmapEnabled = false;
        ti.wrapMode = TextureWrapMode.Clamp;
        ti.maxTextureSize = maxRozmiar;
        ti.textureCompression = TextureImporterCompression.CompressedHQ;
        ti.SaveAndReimport();
    }

    static Material Mat(string nazwa, string shader, System.Action<Material> ustaw)
    {
        string sciezka = $"{KatMat}/{nazwa}.mat";
        var sh = Shader.Find(shader);
        if (sh == null) throw new System.Exception("Brak shadera " + shader);
        var m = AssetDatabase.LoadAssetAtPath<Material>(sciezka);
        if (m == null)
        {
            m = new Material(sh);
            AssetDatabase.CreateAsset(m, sciezka);
        }
        m.shader = sh;
        ustaw?.Invoke(m);
        EditorUtility.SetDirty(m);
        return m;
    }

    // Domyślne niebo Unity (Skybox/Procedural) z możliwością zabarwienia
    static Material Niebo(string nazwa, Color niebo, Color ziemia, float ekspozycja)
    {
        return Mat(nazwa, "Skybox/Procedural", m =>
        {
            m.SetColor("_SkyTint", niebo);
            m.SetColor("_GroundColor", ziemia);
            m.SetFloat("_Exposure", ekspozycja);
            m.SetFloat("_AtmosphereThickness", 1f);
            m.SetFloat("_SunSize", 0.04f);
        });
    }

    // Prosty post-processing: Bloom (poświata przycisków) + tonemapping ACES + lekka winieta
    static VolumeProfile Profil()
    {
        string sciezka = $"{KatMat}/PostProcessing.asset";
        var p = AssetDatabase.LoadAssetAtPath<VolumeProfile>(sciezka);
        if (p != null) return p;
        p = ScriptableObject.CreateInstance<VolumeProfile>();
        AssetDatabase.CreateAsset(p, sciezka);

        var b = Dodaj<Bloom>(p);
        b.threshold.Override(0.9f);
        b.intensity.Override(1.0f);
        b.scatter.Override(0.7f);
        b.highQualityFiltering.Override(true);
        Dodaj<Tonemapping>(p).mode.Override(TonemappingMode.ACES);
        var ca = Dodaj<ColorAdjustments>(p);
        ca.postExposure.Override(0.2f);
        ca.contrast.Override(8f);
        Dodaj<Vignette>(p).intensity.Override(0.25f);

        EditorUtility.SetDirty(p);
        return p;
    }

    static T Dodaj<T>(VolumeProfile p) where T : VolumeComponent
    {
        var c = p.Add<T>(true);
        c.name = typeof(T).Name;
        AssetDatabase.AddObjectToAsset(c, p);
        return c;
    }

    // ------------------------------------------------------------------ elementy sceny

    static UnityEngine.SceneManagement.Scene NowaScena()
    {
        return EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
    }

    static Camera Kamera(CameraClearFlags tlo)
    {
        var go = new GameObject("Main Camera");
        go.tag = "MainCamera";
        go.transform.position = new Vector3(0, 1, -10);
        var cam = go.AddComponent<Camera>();
        cam.clearFlags = tlo;
        cam.backgroundColor = Color.black;
        cam.allowHDR = true;
        go.AddComponent<AudioListener>();
        var dane = cam.GetUniversalAdditionalCameraData();
        dane.renderPostProcessing = true;
        dane.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
        return cam;
    }

    static void Swiatlo()
    {
        var go = new GameObject("Directional Light");
        go.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        var l = go.AddComponent<Light>();
        l.type = LightType.Directional;
        l.color = new Color(1f, 0.96f, 0.84f);
        l.intensity = 1.5f;
        l.shadows = LightShadows.Soft;
        RenderSettings.sun = l;
    }

    static void Otoczenie(Material niebo)
    {
        RenderSettings.skybox = niebo;
        RenderSettings.ambientMode = AmbientMode.Skybox;
    }

    static void Wolumen()
    {
        var go = new GameObject("Global Volume");
        var v = go.AddComponent<Volume>();
        v.isGlobal = true;
        v.sharedProfile = Profil();
    }

    static SManager Menedzer()
    {
        return new GameObject("SceneManager").AddComponent<SManager>();
    }

    static void SystemZdarzen()
    {
        var go = new GameObject("EventSystem", typeof(EventSystem));
        go.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
    }

    // ------------------------------------------------------------------ UI

    static Canvas Plotno(Camera cam, string nazwa = "Canvas", float odleglosc = 1f)
    {
        var go = new GameObject(nazwa, typeof(RectTransform));
        go.layer = 5;
        var c = go.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceCamera;  // UI przechodzi przez post-processing (Bloom)
        c.worldCamera = cam;
        c.planeDistance = odleglosc;
        var sk = go.AddComponent<CanvasScaler>();
        sk.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sk.referenceResolution = new Vector2(1920, 1080);
        sk.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        return c;
    }

    static RectTransform Element(string nazwa, Transform rodzic, Vector2 poz, Vector2 rozmiar)
    {
        var go = new GameObject(nazwa, typeof(RectTransform));
        go.layer = 5;
        var rt = (RectTransform)go.transform;
        rt.SetParent(rodzic, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = poz;
        rt.sizeDelta = rozmiar;
        return rt;
    }

    static RectTransform Rozciagnij(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
        return rt;
    }

    static Text Napis(Transform rodzic, string nazwa, string tekst, int rozmiar, Color kolor, Vector2 poz, Vector2 wym,
                      TextAnchor wyr = TextAnchor.MiddleLeft, bool gruby = false)
    {
        var rt = Element(nazwa, rodzic, poz, wym);
        var t = rt.gameObject.AddComponent<Text>();
        t.text = tekst;
        t.font = gruby ? czcionkaBold : czcionkaReg;
        t.fontSize = rozmiar;
        t.color = kolor;
        t.alignment = wyr;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        var s = rt.gameObject.AddComponent<Shadow>();
        s.effectColor = new Color(0f, 0f, 0f, 0.6f);
        s.effectDistance = new Vector2(2f, -2f);
        return t;
    }

    static Button Przycisk(Transform rodzic, string tekst, Vector2 poz, Color kolor, UnityAction akcja)
    {
        var rt = Element("Button " + tekst, rodzic, poz, new Vector2(360, 88));
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = ramkaPrzycisku;
        img.type = Image.Type.Sliced;
        img.pixelsPerUnitMultiplier = 1.25f;
        img.color = kolor;
        img.material = matUI;

        var b = rt.gameObject.AddComponent<Button>();
        b.targetGraphic = img;
        b.transition = Selectable.Transition.None;   // efekt najechania robi EfektPrzycisku
        UnityEventTools.AddPersistentListener(b.onClick, akcja);

        var napis = Napis(rt, "Text", tekst, 30, Color.Lerp(kolor, Color.white, 0.45f), Vector2.zero, Vector2.zero,
                          TextAnchor.MiddleCenter, true);
        napis.material = matUI;
        Rozciagnij(napis.rectTransform);

        rt.gameObject.AddComponent<EfektPrzycisku>();
        return b;
    }

    // ------------------------------------------------------------------ sceny

    static void ScenaMenu()
    {
        var s = NowaScena();
        var cam = Kamera(CameraClearFlags.SolidColor);
        Swiatlo();
        Otoczenie(Niebo("Niebo_Domyslne", new Color(0.5f, 0.5f, 0.5f), new Color(0.37f, 0.35f, 0.34f), 1.3f));
        Wolumen();

        // Tło menu - grafika na osobnym płótnie za przyciskami
        var tloPlotno = Plotno(cam, "Canvas Tło", 50f);
        tloPlotno.sortingOrder = -1;
        Object.DestroyImmediate(tloPlotno.GetComponent<GraphicRaycaster>());
        var tlo = Rozciagnij(Element("Tło", tloPlotno.transform, Vector2.zero, Vector2.zero));
        var tloImg = tlo.gameObject.AddComponent<Image>();
        tloImg.sprite = tloMenu;
        tloImg.raycastTarget = false;
        var fit = tlo.gameObject.AddComponent<AspectRatioFitter>();
        fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        fit.aspectRatio = tloMenu.rect.width / tloMenu.rect.height;
        // lekkie przyciemnienie po bokach pod napisami i przyciskami
        foreach (bool lewa in new[] { true })
        {
            var rt = Element(lewa ? "Cień lewy" : "Cień prawy", tloPlotno.transform, Vector2.zero, Vector2.zero);
            rt.anchorMin = new Vector2(lewa ? 0f : 0.7f, 0f);
            rt.anchorMax = new Vector2(lewa ? 0.38f : 1f, 1f);
            rt.sizeDelta = Vector2.zero;
            if (!lewa) rt.localScale = new Vector3(-1f, 1f, 1f);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = gradient;
            img.color = new Color(0f, 0f, 0.03f, 0.7f);
            img.raycastTarget = false;
        }

        var c = Plotno(cam);
        var m = Menedzer();

        Napis(c.transform, "Tytuł", "Space\nRunner", 88, Color.white, new Vector2(-640, 300), new Vector2(560, 260),
              TextAnchor.UpperLeft, true).lineSpacing = 0.85f;
        Napis(c.transform, "Podtytuł", "WIMIiP", 38, Color.white, new Vector2(-640, 120), new Vector2(560, 60), gruby: true);
        Napis(c.transform, "Wersja", "wersja 1.0", 28, Color.white, new Vector2(-640, 72), new Vector2(560, 44));

        // Lewa kolumna pod napisem - przyciski w równych odstępach
        const float XP = -740f, Y0 = -40f, ODSTEP = 120f;
        Przycisk(c.transform, "START GRY", new Vector2(XP, Y0), Neon, m.IdzDoSceny);
        Przycisk(c.transform, "INSTRUKCJA", new Vector2(XP, Y0 - ODSTEP), Neon, m.IdzInstrukca);
        Przycisk(c.transform, "WYJŚCIE", new Vector2(XP, Y0 - 2 * ODSTEP), Neon, m.Wyjdz);

        SystemZdarzen();
        EditorSceneManager.SaveScene(s, Sceny[0]);
    }

    // ------------------------------------------------------------------ prefaby platform (slajd "Efekt końcowy 3")

    static Material MatLit(string nazwa, Color kolor, float gladkosc = 0.4f)
    {
        return Mat(nazwa, "Universal Render Pipeline/Lit", m =>
        {
            m.SetColor("_BaseColor", kolor);
            m.SetFloat("_Smoothness", gladkosc);
            m.SetFloat("_Metallic", 0f);
        });
    }

    static GameObject Bryla(PrimitiveType typ, string nazwa, Transform rodzic, Vector3 poz, Vector3 skala, Material mat, float obrotY = 0f)
    {
        var go = GameObject.CreatePrimitive(typ);
        go.name = nazwa;
        go.transform.SetParent(rodzic, false);
        go.transform.localPosition = poz;
        go.transform.localRotation = Quaternion.Euler(0f, obrotY, 0f);
        go.transform.localScale = skala;
        go.GetComponent<MeshRenderer>().sharedMaterial = mat;
        return go;
    }

    // Przeszkoda = pusty obiekt "PrzeszkodaN" z bryłą w środku (jak w filmie instruktażowym)
    static GameObject Przeszkoda(Transform kafelek, int nr, PrimitiveType typ, Vector3 poz, Vector3 skala, Material mat, float obrotY = 0f)
    {
        var p = new GameObject("Przeszkoda" + nr);
        p.transform.SetParent(kafelek, false);
        Bryla(typ, typ == PrimitiveType.Sphere ? "Sphere" : "Cube", p.transform, poz, skala, mat, obrotY);
        return p;
    }

    static GameObject Kafelek(string nazwa, float szerokosc, float dlugosc, System.Func<Transform, GameObject[]> przeszkody)
    {
        var root = new GameObject(nazwa);
        Bryla(PrimitiveType.Cube, "Cube", root.transform, new Vector3(0f, -0.25f, 0f), new Vector3(szerokosc, 0.5f, dlugosc),
              MatLit("MatPlatforma", new Color(0.78f, 0.83f, 0.93f), 0.55f));
        var lista = przeszkody(root.transform);
        var start = new GameObject("StartPoint").transform;
        start.SetParent(root.transform, false);
        start.localPosition = new Vector3(0f, 0f, -dlugosc / 2f);
        var koniec = new GameObject("EndPoint").transform;
        koniec.SetParent(root.transform, false);
        koniec.localPosition = new Vector3(0f, 0f, dlugosc / 2f);

        var k = root.AddComponent<KafelekPlatformy>();
        k.punktStartowy = start;
        k.punktKoncowy = koniec;
        k.przeszkody = lista;

        var prefab = PrefabUtility.SaveAsPrefabAsset(root, $"Assets/Prefabs/{nazwa}.prefab");
        Object.DestroyImmediate(root);
        return prefab;
    }

    const float SzerokoscPlatformy = 12f;

    static GameObject[] Prefaby()
    {
        Directory.CreateDirectory("Assets/Prefabs");
        var mat1 = MatLit("Mat1", new Color(0.55f, 0.12f, 0.14f));   // czerwony
        var mat2 = MatLit("Mat2", new Color(0.16f, 0.55f, 0.22f));   // zielony
        var mat3 = MatLit("Mat3", new Color(0.13f, 0.17f, 0.6f));    // niebieski
        var mat4 = MatLit("Mat4", new Color(0.75f, 0.6f, 0.15f));    // żółty

        // Platforma 1: wąska i długa - klocek, wysoki słup, płaska belka
        var k1 = Kafelek("NaszKafelek1", SzerokoscPlatformy, 30f, t => new[]
        {
            Przeszkoda(t, 1, PrimitiveType.Cube, new Vector3(-1.8f, 1.25f, -8f), new Vector3(2f, 2.5f, 2f), mat1),
            Przeszkoda(t, 2, PrimitiveType.Cube, new Vector3(0.8f, 4f, 3f), new Vector3(0.7f, 8f, 0.7f), mat3),
            Przeszkoda(t, 3, PrimitiveType.Cube, new Vector3(2.2f, 0.6f, 10f), new Vector3(2.8f, 1.2f, 0.5f), mat2, -15f),
        });

        // Platforma 2: wysoka czerwona ściana, niebieski blok, zielona belka i żółta kula
        var k2 = Kafelek("NaszKafelek2", SzerokoscPlatformy, 30f, t => new[]
        {
            Przeszkoda(t, 1, PrimitiveType.Cube, new Vector3(-3.5f, 3f, -2f), new Vector3(0.8f, 6f, 3.2f), mat1, -20f),
            Przeszkoda(t, 2, PrimitiveType.Cube, new Vector3(1.5f, 1.75f, 4f), new Vector3(1.6f, 3.5f, 1.6f), mat3),
            Przeszkoda(t, 3, PrimitiveType.Cube, new Vector3(3.5f, 1f, 9f), new Vector3(3.6f, 2f, 0.6f), mat2, -20f),
            Przeszkoda(t, 4, PrimitiveType.Sphere, new Vector3(-1.5f, 2f, -9f), new Vector3(4f, 4f, 4f), mat4),
        });
        return new[] { k1, k2 };
    }

    static void ScenaGra()
    {
        var s = NowaScena();
        var cam = Kamera(CameraClearFlags.SolidColor);
        cam.backgroundColor = new Color(0.13f, 0.2f, 0.3f);
        cam.transform.SetPositionAndRotation(new Vector3(-26f, 17f, -22f), Quaternion.identity);
        cam.transform.LookAt(new Vector3(0f, 0f, 18f));
        cam.fieldOfView = 50f;
        cam.farClipPlane = 500f;
        cam.gameObject.AddComponent<KameraSwobodna>();
        Swiatlo();
        Otoczenie(Niebo("Niebo_Domyslne", new Color(0.5f, 0.5f, 0.5f), new Color(0.37f, 0.35f, 0.34f), 1.3f));
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.55f, 0.62f, 0.75f);
        RenderSettings.ambientEquatorColor = new Color(0.35f, 0.4f, 0.5f);
        RenderSettings.ambientGroundColor = new Color(0.15f, 0.18f, 0.24f);
        Wolumen();

        // podłoże z siatką jak w widoku Scene
        var siatka = GameObject.CreatePrimitive(PrimitiveType.Plane);
        siatka.name = "Siatka";
        Object.DestroyImmediate(siatka.GetComponent<Collider>());
        siatka.transform.position = new Vector3(0f, -3f, 0f);
        siatka.transform.localScale = new Vector3(60f, 1f, 60f);
        siatka.GetComponent<MeshRenderer>().sharedMaterial = Mat("Siatka", "Lab6/SiatkaProsta", null);

        // dwa kafelki połączone punktem końcowym -> startowym
        var prefaby = Prefaby();
        Vector3 poz = Vector3.zero;
        foreach (var p in prefaby)
        {
            var k = (GameObject)PrefabUtility.InstantiatePrefab(p);
            var kp = k.GetComponent<KafelekPlatformy>();
            k.transform.position = poz - kp.punktStartowy.localPosition;
            poz = kp.punktKoncowy.position;
        }

        Menedzer().gameObject.AddComponent<GraTymczasowa>();   // ESC -> KoniecGry (do testów)
        EditorSceneManager.SaveScene(s, Sceny[1]);
    }

    static void ScenaInstrukcja()
    {
        var s = NowaScena();
        var cam = Kamera(CameraClearFlags.Skybox);
        Swiatlo();
        Otoczenie(Niebo("Niebo_Instrukcja", new Color(0.2f, 0.55f, 0.85f), new Color(0.2f, 0.4f, 0.55f), 1.1f));
        Wolumen();

        var c = Plotno(cam);
        var m = Menedzer();
        Przycisk(c.transform, "POWRÓT DO MENU", new Vector2(720, -430), Neon, m.IdzMenu);
        SystemZdarzen();
        EditorSceneManager.SaveScene(s, Sceny[2]);
    }

    static void ScenaKoniec()
    {
        var s = NowaScena();
        var cam = Kamera(CameraClearFlags.Skybox);
        Swiatlo();
        Otoczenie(Niebo("Niebo_Koniec", new Color(0.45f, 0.45f, 0.5f), new Color(0.45f, 0.45f, 0.45f), 1.2f));
        Wolumen();

        var c = Plotno(cam);
        var m = Menedzer();
        Przycisk(c.transform, "RESTART GRY", new Vector2(-660, -400), Neon, m.IdzDoSceny);
        Przycisk(c.transform, "WYJŚCIE", new Vector2(660, -400), Neon, m.IdzMenu);   // z ekranu końca gry wracamy do menu
        SystemZdarzen();
        EditorSceneManager.SaveScene(s, Sceny[3]);
    }
}
