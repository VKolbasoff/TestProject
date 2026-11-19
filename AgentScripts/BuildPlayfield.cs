using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using AstraBricks;

/// <summary>
/// Собирает биту, мяч и стены поля в активной сцене.
/// Все размеры берутся из GameConfig (Assets/Config/GameConfig.asset).
/// Запуск: unity command script run --file AgentScripts/BuildPlayfield.cs --entry BuildPlayfield.Run
/// </summary>
public static class BuildPlayfield
{
    const string PaddleSprite = "Assets/Art/Paddle/bat_01_standart.png";
    const string BallSprite = "Assets/Art/Ball/ball_01_standart.png";
    const string ConfigPath = "Assets/Config/GameConfig.asset";

    /// <summary>Запасной вариант, если в конфиге толщина стен не задана.</summary>
    const float DefaultWallThickness = 2f;

    public static string Run()
    {
        var log = new List<string>();

        var config = AssetDatabase.LoadAssetAtPath<GameConfig>(ConfigPath);
        if (config == null)
        {
            log.Add("ОШИБКА: нет конфига " + ConfigPath);
            return string.Join("\n", log);
        }

        float paddlePpu = 150f / config.paddleWidth;
        float ballPpu = 20f / (config.ballRadius * 2f);
        ConfigureSprite(PaddleSprite, paddlePpu, log);
        ConfigureSprite(BallSprite, ballPpu, log);

        var root = GameObject.Find("Game");
        if (root == null) root = new GameObject("Game");

        var paddle = BuildPaddle(root.transform, config, log);
        BuildBall(root.transform, config, paddle, log);
        BuildWalls(root.transform, config, log);

        var scene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        log.Add("сцена сохранена");

        return string.Join("\n", log);
    }

    static void ConfigureSprite(string path, float ppu, List<string> log)
    {
        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null)
        {
            log.Add("ОШИБКА: нет импортёра " + path);
            return;
        }

        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.spritePixelsPerUnit = ppu;
        ti.mipmapEnabled = false;
        ti.wrapMode = TextureWrapMode.Clamp;
        ti.filterMode = FilterMode.Bilinear;
        ti.alphaIsTransparency = true;

        var settings = new TextureImporterSettings();
        ti.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        settings.spriteAlignment = (int)SpriteAlignment.Center;
        settings.spriteGenerateFallbackPhysicsShape = false;
        ti.SetTextureSettings(settings);

        ti.SaveAndReimport();
        log.Add(string.Format("{0}: PPU {1:F2}", path, ppu));
    }

    static GameObject BuildPaddle(Transform parent, GameConfig config, List<string> log)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(PaddleSprite);
        float centerY = -config.fieldHeight * 0.5f + config.paddleHeightAboveBottom;

        var go = GameObject.Find("Paddle");
        if (go == null) go = new GameObject("Paddle");
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(0f, centerY, 0f);
        go.transform.localScale = Vector3.one;

        var box = go.GetComponent<BoxCollider2D>();
        if (box == null) box = go.AddComponent<BoxCollider2D>();
        box.size = new Vector2(config.paddleWidth, config.paddleHeight);
        box.offset = Vector2.zero;

        var visual = go.transform.Find("Sprite");
        var visualGo = visual != null ? visual.gameObject : new GameObject("Sprite");
        visualGo.transform.SetParent(go.transform, false);
        visualGo.transform.localPosition = Vector3.zero;

        var sr = visualGo.GetComponent<SpriteRenderer>();
        if (sr == null) sr = visualGo.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 10;
        sr.color = Color.white;

        var controller = go.GetComponent<PaddleController>();
        if (controller == null) controller = go.AddComponent<PaddleController>();
        controller.config = config;
        controller.targetCamera = Camera.main;
        controller.visual = visualGo.transform;
        controller.ApplySize();

        log.Add(string.Format("бита: {0}x{1} единиц, центр Y {2:F1}, спрайт {3:F1}x{4:F1}",
            config.paddleWidth, config.paddleHeight, centerY,
            sprite != null ? sprite.bounds.size.x : 0f,
            sprite != null ? sprite.bounds.size.y : 0f));

        return go;
    }

    static GameObject BuildBall(Transform parent, GameConfig config, GameObject paddle, List<string> log)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(BallSprite);

        var go = GameObject.Find("Ball");
        if (go == null) go = new GameObject("Ball");
        go.transform.SetParent(parent, false);

        var sr = go.GetComponent<SpriteRenderer>();
        if (sr == null) sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 11;
        sr.color = Color.white;

        var controller = go.GetComponent<BallController>();
        if (controller == null) controller = go.AddComponent<BallController>();
        controller.config = config;
        controller.paddle = paddle.GetComponent<PaddleController>();

        float paddleTop = -config.fieldHeight * 0.5f + config.paddleHeightAboveBottom
                          + config.paddleHeight * 0.5f;
        go.transform.position = new Vector3(0f, paddleTop + config.ballRadius, 0f);

        log.Add(string.Format("мяч: радиус {0}, старт Y {1:F1}, спрайт {2:F1}x{3:F1}",
            config.ballRadius, paddleTop + config.ballRadius,
            sprite != null ? sprite.bounds.size.x : 0f,
            sprite != null ? sprite.bounds.size.y : 0f));

        return go;
    }

    static void BuildWalls(Transform parent, GameConfig config, List<string> log)
    {
        float halfW = config.fieldWidth * 0.5f;
        float topY = config.fieldHeight * 0.5f;
        float t = config.wallThickness > 0f ? config.wallThickness : DefaultWallThickness;

        MakeWall(parent, "Wall_Left", new Vector3(-halfW - t * 0.5f, 0f, 0f),
            new Vector2(t, config.fieldHeight + t * 2f));
        MakeWall(parent, "Wall_Right", new Vector3(halfW + t * 0.5f, 0f, 0f),
            new Vector2(t, config.fieldHeight + t * 2f));
        MakeWall(parent, "Wall_Top", new Vector3(0f, topY + t * 0.5f, 0f),
            new Vector2(config.fieldWidth + t * 2f, t));

        log.Add("стены: слева, справа, сверху");
    }

    static void MakeWall(Transform parent, string name, Vector3 position, Vector2 size)
    {
        var go = GameObject.Find(name);
        if (go == null) go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = position;

        var box = go.GetComponent<BoxCollider2D>();
        if (box == null) box = go.AddComponent<BoxCollider2D>();
        box.size = size;
        box.offset = Vector2.zero;
    }
}
