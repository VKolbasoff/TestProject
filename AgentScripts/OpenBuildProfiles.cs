using UnityEditor;
using UnityEngine;

public static class OpenBuildProfiles
{
    public static string Run()
    {
        EditorApplication.ExecuteMenuItem("File/Build Profiles");
        foreach (var w in Resources.FindObjectsOfTypeAll<EditorWindow>())
        {
            if (w.GetType().Name.Contains("BuildProfile"))
            {
                w.Focus();
                return $"открыто окно: {w.titleContent.text} ({w.GetType().Name})";
            }
        }

        return "окно Build Profiles не найдено";
    }
}
