using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using AstraBricks;

/// <summary>
/// Собирает в активной сцене камеру под поле 100x170 и два слоя бесшовного фона.
/// Запуск: script_run file=AgentScripts/BuildCameraSetup.cs entry=BuildCameraSetup.Run
/// </summary>
public static class BuildCameraSetup
{
    const string Back01 = "Assets/Art/Backgroud/back_01.png";
    const string Back02 = "Assets/Art/Backgroud/back_02.png";

    const float FieldWidth = 100f;
    const float FieldHeight = 170f;
    const float TilePixels = 1024f;

    public static string Run()
    {
        var log = new List<string>();

        ConfigureTexture(Back01, log);
        ConfigureTexture(Back02, log);

        var cam = SetupCamera(log);

        var root = GameObject.Find("Background");
        if (root == null) root = new GameObject("Background");
        root.transform.position = Vector3.zero;

        SetupLayer(root.transform, "BG_Far_Nebula", Back01, -20, new Vector2(0.35f, 0.10f), cam, log);
        SetupLayer(root.transform, "BG_Near_Planets", Back02, -10, new Vector2(1.10f, 0.30f), cam, log);

        var scene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        log.Add("сцена сохранена: " + scene.path);

        return string.Join("\n", log);
    }

    static void ConfigureTexture(string path, List<string> log)
    {
        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null)
        {
            log.Add("ОШИБКА: нет импортёра для " + path);
            return;
        }

        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        // 1024 px = 100 мировых единиц -> один тайл ровно на ширину поля
        ti.spritePixelsPerUnit = TilePixels / FieldWidth;
        ti.mipmapEnabled = false;
        ti.wrapMode = TextureWrapMode.Repeat;
        ti.filterMode = FilterMode.Bilinear;
        ti.alphaIsTransparency = true;
        ti.maxTextureSize = 2048;
        ti.textureCompression = TextureImporterCompression.Uncompressed;

        var settings = new TextureImporterSettings();
        ti.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        settings.spriteGenerateFallbackPhysicsShape = false;
        ti.SetTextureSettings(settings);

        ti.SaveAndReimport();
        log.Add(string.Format("{0}: Sprite/Single, PPU {1:F2}, Repeat, FullRect", path, ti.spritePixelsPerUnit));
    }

    static Camera SetupCamera(List<string> log)
    {
        var cam = Camera.main;
        if (cam == null)
        {
            var found = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
            if (found.Length > 0) cam = found[0];
        }

        if (cam == null)
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            cam = go.AddComponent<Camera>();
            go.AddComponent<AudioListener>();
            log.Add("камера: создана новая");
        }
        else
        {
            log.Add("камера: использована существующая " + cam.name);
        }

        var t = cam.transform;
        t.position = new Vector3(0f, 0f, -10f);
        t.rotation = Quaternion.identity;
        t.localScale = Vector3.one;

        cam.orthographic = true;
        cam.orthographicSize = FieldHeight * 0.5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.nearClipPlane = 0.3f;
        cam.farClipPlane = 100f;
        cam.allowHDR = false;
        cam.allowMSAA = false;

        var fit = cam.GetComponent<PlayfieldCamera>();
        if (fit == null) fit = cam.gameObject.AddComponent<PlayfieldCamera>();
        fit.fieldWidth = FieldWidth;
        fit.fieldHeight = FieldHeight;
        fit.Apply();

        log.Add(string.Format("поле {0}x{1}, orthoSize {2:F2}, aspect {3:F4}", FieldWidth, FieldHeight, cam.orthographicSize, cam.aspect));
        return cam;
    }

    static void SetupLayer(Transform parent, string name, string spritePath, int order, Vector2 speed, Camera cam, List<string> log)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (sprite == null)
        {
            log.Add("ОШИБКА: спрайт не загрузился " + spritePath);
            return;
        }

        var existing = parent.Find(name);
        var go = existing != null ? existing.gameObject : new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = Vector3.zero;

        var sr = go.GetComponent<SpriteRenderer>();
        if (sr == null) sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.tileMode = SpriteTileMode.Continuous;
        sr.sortingLayerName = "Default";
        sr.sortingOrder = order;
        sr.color = Color.white;
        sr.flipX = false;
        sr.flipY = false;

        var layer = go.GetComponent<BackgroundLayer>();
        if (layer == null) layer = go.AddComponent<BackgroundLayer>();
        layer.targetCamera = cam;
        layer.scrollSpeed = speed;
        layer.overscanTiles = 1f;
        layer.Apply();

        var size = sr.size;
        log.Add(string.Format("{0}: order {1}, тайл {2:F0}x{3:F0} ед., размер слоя {4:F0}x{5:F0} ед., дрейф {6}",
            name, order, sprite.bounds.size.x, sprite.bounds.size.y, size.x, size.y, speed));
    }
}
