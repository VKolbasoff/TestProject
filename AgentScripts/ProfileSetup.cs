using System;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEngine;

public static class ProfileSetup
{
    private const string ProfilePath = "Assets/Settings/Build Profiles/Android.asset";
    private const string ScenePath = "Assets/Scenes/TestScene.unity";

    public static string Run()
    {
        var sb = new StringBuilder();

        // Возвращаем версию после эксперимента.
        PlayerSettings.bundleVersion = "0.2";
        PlayerSettings.Android.bundleVersionCode = 2;

        var profile = BuildProfile.GetActiveBuildProfile();
        sb.AppendLine($"активный профиль: {(profile ? profile.name : "нет")}");
        if (!profile)
        {
            profile = AssetDatabase.LoadAssetAtPath<BuildProfile>(ProfilePath);
            sb.AppendLine($"профиль из ассета: {(profile ? profile.name : "не найден")}");
        }

        if (profile)
        {
            profile.overrideGlobalScenes = true;
            profile.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            EditorUtility.SetDirty(profile);
            sb.AppendLine("сцены профиля: " + string.Join(" | ",
                Array.ConvertAll(profile.GetScenesForBuild(), s => $"{s.path}{(s.enabled ? " (вкл)" : " (выкл)")}")));
        }

        EditorApplication.ExecuteMenuItem("File/Save Project");
        AssetDatabase.SaveAssets();
        sb.AppendLine($"версия в PlayerSettings: {PlayerSettings.bundleVersion} ({PlayerSettings.Android.bundleVersionCode})");
        return sb.ToString();
    }
}
