using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using AstraBricks;

/// <summary>
/// Сбрасывает смещение фоновых слоёв в ноль, чтобы дрейф начинался с начала координат.
/// </summary>
public static class ResetBackgroundOffset
{
    public static string Run()
    {
        var log = new List<string>();

        foreach (var layer in Object.FindObjectsByType<BackgroundLayer>(FindObjectsSortMode.None))
        {
            var p = layer.transform.localPosition;
            layer.transform.localPosition = new Vector3(0f, 0f, p.z);
            layer.Apply();
            log.Add(layer.name + " -> (0, 0)");
        }

        var scene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        log.Add("сцена сохранена");
        return string.Join("\n", log);
    }
}
