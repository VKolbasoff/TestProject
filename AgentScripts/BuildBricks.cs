using System.Text;
using AstraBricks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Ставит в TestScene объект Bricks с BrickField и ссылками на конфиг и спрайты.</summary>
public static class BuildBricks
{
    private const string ScenePath = "Assets/Scenes/TestScene.unity";
    private const string ConfigPath = "Assets/Config/GameConfig.asset";
    private const string Brick1Path = "Assets/Art/Bricks/blue_brick_01.png";
    private const string Brick2Path = "Assets/Art/Bricks/blue_brick_02.png";

    public static string Run()
    {
        var sb = new StringBuilder();

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        sb.AppendLine($"сцена: {scene.path}");

        var config = AssetDatabase.LoadAssetAtPath<GameConfig>(ConfigPath);
        var brick1 = AssetDatabase.LoadAssetAtPath<Sprite>(Brick1Path);
        var brick2 = AssetDatabase.LoadAssetAtPath<Sprite>(Brick2Path);
        sb.AppendLine($"config: {(config ? "ок" : "НЕТ")}, brick1: {(brick1 ? brick1.rect.width + "x" + brick1.rect.height : "НЕТ")}, brick2: {(brick2 ? brick2.rect.width + "x" + brick2.rect.height : "НЕТ")}");

        var game = Find(scene, "Game");
        if (game == null)
        {
            sb.AppendLine("НЕ найден объект Game");
            return sb.ToString();
        }

        var bricks = game.transform.Find("Bricks");
        if (bricks == null)
        {
            var go = new GameObject("Bricks");
            bricks = go.transform;
            bricks.SetParent(game.transform, false);
            sb.AppendLine("создан объект Bricks");
        }
        else
        {
            sb.AppendLine("объект Bricks уже есть");
        }

        bricks.localPosition = Vector3.zero;
        bricks.localRotation = Quaternion.identity;
        bricks.localScale = Vector3.one;

        var field = bricks.GetComponent<BrickField>();
        if (field == null)
        {
            field = bricks.gameObject.AddComponent<BrickField>();
            sb.AppendLine("добавлен BrickField");
        }

        field.config = config;
        field.brickType1 = brick1;
        field.brickType2 = brick2;
        field.parent = bricks;

        EditorUtility.SetDirty(field);
        EditorUtility.SetDirty(bricks.gameObject);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        sb.AppendLine($"сохранено: {scene.path}");
        return sb.ToString();
    }

    private static GameObject Find(Scene scene, string name)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            var found = FindIn(root.transform, name);
            if (found != null) return found.gameObject;
        }
        return null;
    }

    private static Transform FindIn(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        for (int i = 0; i < parent.childCount; i++)
        {
            var found = FindIn(parent.GetChild(i), name);
            if (found != null) return found;
        }
        return null;
    }
}
