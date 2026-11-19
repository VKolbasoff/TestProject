using UnityEditor;
using UnityEngine;

public static class ReimportDevice
{
    public static string Run()
    {
        AssetDatabase.ImportAsset("Assets/Devices/DeviceOverlays/Astra 1080x2400_Overlay.png",
            ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset("Assets/Devices/Astra 1080x2400.device", ImportAssetOptions.ForceUpdate);
        AssetDatabase.Refresh();

        var asset = AssetDatabase.LoadMainAssetAtPath("Assets/Devices/Astra 1080x2400.device");
        var so = new SerializedObject(asset);
        var pres = so.FindProperty("deviceInfo.screens").GetArrayElementAtIndex(0).FindPropertyRelative("presentation");
        return $"overlay={pres.FindPropertyRelative("overlayPath").stringValue}, " +
               $"border={pres.FindPropertyRelative("borderSize").FindPropertyRelative("x").floatValue}, " +
               $"radius={pres.FindPropertyRelative("cornerRadius").floatValue}, " +
               $"tex={(AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Devices/DeviceOverlays/Astra 1080x2400_Overlay.png") != null)}";
    }
}
