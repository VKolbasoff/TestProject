using System;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class DumpBuildProfile
{
    public static string Run()
    {
        var sb = new StringBuilder();
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types;
            try { types = asm.GetTypes(); } catch (ReflectionTypeLoadException e) { types = Array.FindAll(e.Types, t => t != null); }
            foreach (var t in types)
            {
                if (t.Namespace == null || !t.Namespace.StartsWith("UnityEditor.Build.Profile"))
                {
                    continue;
                }

                if (t.Name.Contains("Setting") || t.Name.Contains("Player") || t.Name.Contains("Component"))
                {
                    sb.AppendLine($"== {t.FullName} (public={t.IsPublic}) : {t.BaseType?.Name}");
                    foreach (var f in t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                    {
                        sb.AppendLine($"   field [{f.FieldType.Name}] {f.Name}");
                    }

                    foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        sb.AppendLine($"   prop [{p.PropertyType.Name}] {p.Name}");
                    }
                }
            }
        }

        // структура сериализованных данных профиля
        var profile = AssetDatabase.LoadAssetAtPath<ScriptableObject>("Assets/Settings/Build Profiles/Android.asset");
        if (profile)
        {
            var so = new SerializedObject(profile);
            var it = so.GetIterator();
            while (it.NextVisible(true))
            {
                sb.AppendLine($"   {it.propertyPath} : {it.propertyType}");
            }
        }

        return sb.ToString();
    }
}
