using System.Text;
using AstraBricks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Показывает состав сцены и настройки BrickField.</summary>
public static class InspectBricks
{
    public static string Run()
    {
        var sb = new StringBuilder();
        var scene = SceneManager.GetActiveScene();
        sb.AppendLine($"scene={scene.path}");

        foreach (var root in scene.GetRootGameObjects())
        {
            Walk(sb, root.transform, 0);
        }
        return sb.ToString();
    }

    private static void Walk(StringBuilder sb, Transform t, int depth)
    {
        sb.Append(' ', depth * 2).Append("- ").Append(t.name);
        foreach (var c in t.GetComponents<Component>())
        {
            if (c is Transform) continue;
            sb.Append(" [").Append(c.GetType().Name).Append(']');
        }

        var field = t.GetComponent<BrickField>();
        if (field != null)
        {
            sb.Append(" cfg=").Append(field.config ? field.config.name : "null");
            sb.Append(" t1=").Append(field.brickType1 ? $"{field.brickType1.rect.width}x{field.brickType1.rect.height}" : "null");
            sb.Append(" t2=").Append(field.brickType2 ? $"{field.brickType2.rect.width}x{field.brickType2.rect.height}" : "null");
        }
        sb.AppendLine();

        for (int i = 0; i < t.childCount; i++)
        {
            Walk(sb, t.GetChild(i), depth + 1);
        }
    }
}
