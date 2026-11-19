using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Добавляет (если нужно) размер «Portrait 1080x2400» в Game view и выбирает его.
/// Рефлексия по внутренним API редактора, поэтому всё в try/catch.
/// </summary>
public static class SetGameViewPortrait
{
    private const string SizeName = "Portrait 1080x2400";

    public static string Run()
    {
        return SetSize(1080, 2400, SizeName);
    }

    public static string RunSmall()
    {
        return SetSize(720, 1280, "Portrait 720x1280");
    }

    /// <summary>Планшет 4:3 — проверка широкого экрана: по бокам должен быть фон.</summary>
    public static string RunTablet()
    {
        return SetSize(1536, 2048, "Tablet 4:3 1536x2048");
    }

    /// <summary>16:9 — эталонная раскладка: в кадре ровно поле 1080 x 1920.</summary>
    public static string Run16x9()
    {
        return SetSize(1080, 1920, "Portrait 1080x1920");
    }

    private static string SetSize(int width, int height, string name)
    {
        try
        {
            var asm = typeof(Editor).Assembly;
            var gameViewSizesType = asm.GetType("UnityEditor.GameViewSizes");
            var gameViewSizeType = asm.GetType("UnityEditor.GameViewSize");
            var gameViewSizeKindType = asm.GetType("UnityEditor.GameViewSizeType");
            var gameViewType = asm.GetType("UnityEditor.GameView");

            object instance = gameViewSizesType.BaseType
                .GetProperty("instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .GetValue(null);
            object currentGroup = instance.GetType()
                .GetProperty("currentGroup", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .GetValue(instance);

            var getTexts = currentGroup.GetType()
                .GetMethod("GetDisplayTexts", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var texts = (string[])getTexts.Invoke(currentGroup, null);
            int index = Array.IndexOf(texts, name);

            if (index < 0)
            {
                var sizeKind = Enum.Parse(gameViewSizeKindType, "FixedResolution");
                var ctor = gameViewSizeType.GetConstructor(new[]
                {
                    gameViewSizeKindType, typeof(int), typeof(int), typeof(string)
                });
                object newSize = ctor.Invoke(new[] { sizeKind, (object)width, height, name });
                currentGroup.GetType()
                    .GetMethod("AddCustomSize", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Invoke(currentGroup, new[] { newSize });

                texts = (string[])getTexts.Invoke(currentGroup, null);
                index = Array.IndexOf(texts, name);
            }

            var gameView = EditorWindow.GetWindow(gameViewType);
            gameViewType
                .GetMethod("SizeSelectionCallback", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Invoke(gameView, new object[] { index, null });

            return $"gameView size set: {texts[index]} (index {index})";
        }
        catch (Exception e)
        {
            return "gameView size: FAILED " + e.GetType().Name + ": " + e.Message;
        }
    }
}
