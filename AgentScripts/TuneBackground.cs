using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using AstraBricks;

/// <summary>
/// Настраивает масштаб тайла фоновых слоёв и пересобирает размеры.
/// Запуск: script_run file=AgentScripts/TuneBackground.cs entry=TuneBackground.Run
/// </summary>
public static class TuneBackground
{
    public static string Run()
    {
        var log = new List<string>();
        var layers = Object.FindObjectsByType<BackgroundLayer>();
        foreach (var layer in layers)
        {
            layer.tileScale = 2f;
            layer.Apply();
            var sr = layer.GetComponent<SpriteRenderer>();
            log.Add(string.Format("{0}: tileScale {1}, тайл {2:F0}x{3:F0} ед., слой {4:F0}x{5:F0} лок.",
                layer.name, layer.tileScale,
                sr.sprite.bounds.size.x * layer.tileScale, sr.sprite.bounds.size.y * layer.tileScale,
                sr.size.x, sr.size.y));
        }

        var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        log.Add("слоёв обработано: " + layers.Length);
        return string.Join("\n", log);
    }
}
