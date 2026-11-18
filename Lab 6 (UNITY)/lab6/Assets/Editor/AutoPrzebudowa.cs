using System.IO;
using UnityEditor;

// Jeśli w katalogu projektu istnieje plik "lab6_przebuduj.flag", po przeładowaniu skryptów edytor
// sam przebuduje sceny, skompiluje grę i ją uruchomi (flaga jest usuwana od razu).
[InitializeOnLoad]
static class AutoPrzebudowa
{
    const string Flaga = "lab6_przebuduj.flag";

    static AutoPrzebudowa()
    {
        EditorApplication.delayCall += Sprawdz;
    }

    static void Sprawdz()
    {
        if (!File.Exists(Flaga) || UnityEngine.Application.isBatchMode) return;
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += Sprawdz;
            return;
        }
        File.Delete(Flaga);
        BudowniczyProjektu.ZbudujSceny();
        if (BudowniczyProjektu.ZbudujGre())
        {
            var exe = Path.GetFullPath("Build/Linux/KosmicznyBiegacz.x86_64");
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(exe)
            {
                WorkingDirectory = Path.GetDirectoryName(exe),
                UseShellExecute = false,
            });
        }
    }
}
