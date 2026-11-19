using System.Text;
using UnityEditor;
using UnityEngine;

public static class ImportBricks
{
    public static string Run()
    {
        var sb = new StringBuilder();
        string[] paths =
        {
            "Assets/Art/Bricks/blue_brick_01.png",
            "Assets/Art/Bricks/blue_brick_02.png"
        };

        foreach (var path in paths)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                sb.AppendLine($"нет импортера: {path}");
                continue;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 1f;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            sb.AppendLine($"{path}: {(sprite ? $"sprite {sprite.bounds.size.x}x{sprite.bounds.size.y}" : "спрайт не получен")}");
        }

        AssetDatabase.Refresh();
        return sb.ToString();
    }
}
