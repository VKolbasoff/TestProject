using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using AstraBricks;

/// <summary>
/// Создаёт Assets/Config/GameConfig.asset (если нет) и подключает его к камере,
/// бите, мячу и границам поля.
/// Запуск: script_run file=AgentScripts/BuildGameConfig.cs entry=BuildGameConfig.Run
/// </summary>
public static class BuildGameConfig
{
    const string AssetPath = "Assets/Config/GameConfig.asset";

    public static string Run()
    {
        var log = new List<string>();
        var config = LoadOrCreate(log);

        AssignCamera(config, log);
        AssignPaddle(config, log);
        AssignBall(config, log);
        AssignBounds(config, log);

        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();

        var scene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        log.Add("сцена сохранена");

        return string.Join("\n", log);
    }

    static GameConfig LoadOrCreate(List<string> log)
    {
        var config = AssetDatabase.LoadAssetAtPath<GameConfig>(AssetPath);
        if (config != null)
        {
            log.Add("конфиг найден: " + AssetPath);
            return config;
        }

        var dir = Path.GetDirectoryName(AssetPath);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        config = ScriptableObject.CreateInstance<GameConfig>();
        AssetDatabase.CreateAsset(config, AssetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        log.Add("конфиг создан: " + AssetPath);
        return config;
    }

    static void AssignCamera(GameConfig config, List<string> log)
    {
        var cam = Camera.main;
        if (cam == null)
        {
            log.Add("ОШИБКА: камера не найдена");
            return;
        }

        var fit = cam.GetComponent<PlayfieldCamera>();
        if (fit == null)
        {
            log.Add("ОШИБКА: на камере нет PlayfieldCamera");
            return;
        }

        fit.config = config;
        EditorUtility.SetDirty(fit);
        log.Add("камера подключена к конфигу");
    }

    static void AssignPaddle(GameConfig config, List<string> log)
    {
        var go = GameObject.Find("Paddle");
        if (go == null)
        {
            log.Add("ОШИБКА: объект Paddle не найден");
            return;
        }

        var paddle = go.GetComponent<PaddleController>();
        if (paddle == null)
        {
            log.Add("ОШИБКА: на Paddle нет PaddleController");
            return;
        }

        paddle.config = config;
        if (paddle.visual == null) paddle.visual = go.transform.Find("Sprite");
        paddle.ApplySize();
        EditorUtility.SetDirty(paddle);
        log.Add("бита подключена к конфигу");
    }

    static void AssignBall(GameConfig config, List<string> log)
    {
        var go = GameObject.Find("Ball");
        if (go == null)
        {
            log.Add("ОШИБКА: объект Ball не найден");
            return;
        }

        var ball = go.GetComponent<BallController>();
        if (ball == null)
        {
            log.Add("ОШИБКА: на Ball нет BallController");
            return;
        }

        ball.config = config;
        EditorUtility.SetDirty(ball);
        log.Add("мяч подключён к конфигу");
    }

    static void AssignBounds(GameConfig config, List<string> log)
    {
        var game = GameObject.Find("Game");
        if (game == null)
        {
            log.Add("ОШИБКА: объект Game не найден");
            return;
        }

        var bounds = game.GetComponent<FieldBounds>();
        if (bounds == null) bounds = game.AddComponent<FieldBounds>();

        bounds.config = config;
        bounds.left = FindTransform("Wall_Left");
        bounds.right = FindTransform("Wall_Right");
        bounds.top = FindTransform("Wall_Top");
        bounds.Apply();
        EditorUtility.SetDirty(bounds);
        log.Add("стены подключены к конфигу");
    }

    static Transform FindTransform(string name)
    {
        var go = GameObject.Find(name);
        return go != null ? go.transform : null;
    }
}
