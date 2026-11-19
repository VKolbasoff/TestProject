using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class ProfileOverrides
{
    private const string ProfilePath = "Assets/Settings/Build Profiles/Android.asset";
    private const string SettingsPath = "ProjectSettings/ProjectSettings.asset";

    public static string Run()
    {
        var sb = new StringBuilder();
        var profile = AssetDatabase.LoadAssetAtPath<ScriptableObject>(ProfilePath);
        sb.AppendLine($"строк в профиле до: {CountLines(profile)}");

        PlayerSettings.bundleVersion = "8.8";
        EditorApplication.ExecuteMenuItem("File/Save Project");

        sb.AppendLine($"PlayerSettings.bundleVersion = {PlayerSettings.bundleVersion}");
        sb.AppendLine($"строк в профиле после: {CountLines(profile)}");
        sb.AppendLine($"содержимое профиля: {DumpLines(profile)}");

        foreach (var line in File.ReadAllLines(SettingsPath))
        {
            if (line.StartsWith("  bundleVersion:"))
            {
                sb.AppendLine($"в ProjectSettings.asset: {line.Trim()}");
            }
        }

        return sb.ToString();
    }

    private static int CountLines(ScriptableObject profile)
    {
        return new SerializedObject(profile).FindProperty("m_PlayerSettingsYaml.m_Settings").arraySize;
    }

    private static string DumpLines(ScriptableObject profile)
    {
        var so = new SerializedObject(profile);
        var settings = so.FindProperty("m_PlayerSettingsYaml.m_Settings");
        var sb = new StringBuilder();
        for (int i = 0; i < settings.arraySize; i++)
        {
            sb.Append('[').Append(settings.GetArrayElementAtIndex(i).FindPropertyRelative("line").stringValue).Append("] ");
        }

        return sb.ToString();
    }
}
