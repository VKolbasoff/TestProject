using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

public static class AndroidSettings
{
    public static string Run()
    {
        var android = NamedBuildTarget.Android;

        PlayerSettings.companyName = "Astra";
        PlayerSettings.productName = "Astra Bricks";
        PlayerSettings.bundleVersion = "0.2";

        PlayerSettings.SetApplicationIdentifier(android, "com.astra.bricks.test");
        PlayerSettings.Android.bundleVersionCode = 2;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.useCustomKeystore = false;

        PlayerSettings.SetScriptingBackend(android, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetManagedStrippingLevel(android, ManagedStrippingLevel.Low);

        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = false;
        PlayerSettings.allowedAutorotateToLandscapeRight = false;

        AssetDatabase.SaveAssets();
        // PlayerSettings пишутся в ProjectSettings.asset по «File → Save Project».
        EditorApplication.ExecuteMenuItem("File/Save Project");

        var sb = new StringBuilder();
        sb.AppendLine($"id: {PlayerSettings.GetApplicationIdentifier(android)}");
        sb.AppendLine($"product: {PlayerSettings.companyName} / {PlayerSettings.productName}, version {PlayerSettings.bundleVersion} ({PlayerSettings.Android.bundleVersionCode})");
        sb.AppendLine($"backend: {PlayerSettings.GetScriptingBackend(android)}, arch: {PlayerSettings.Android.targetArchitectures}, stripping: {PlayerSettings.GetManagedStrippingLevel(android)}");
        sb.AppendLine($"minSdk: {PlayerSettings.Android.minSdkVersion}, targetSdk: {PlayerSettings.Android.targetSdkVersion}, custom keystore: {PlayerSettings.Android.useCustomKeystore}");
        sb.AppendLine($"orientation: {PlayerSettings.defaultInterfaceOrientation}");
        return sb.ToString();
    }
}
