using System;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class DeviceSimWindow
{
    private static Type WindowType => Type.GetType("UnityEditor.DeviceSimulation.SimulatorWindow, UnityEditor.DeviceSimulatorModule");
    private static Type StateType => Type.GetType("UnityEditor.DeviceSimulation.SimulatorState, UnityEditor.DeviceSimulatorModule");

    public static string Dump()
    {
        var sb = new StringBuilder();
        var t = WindowType;
        sb.AppendLine($"window type: {t}");
        foreach (var f in t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            sb.AppendLine($"  field {f.FieldType.Name} {f.Name}");
        }

        foreach (var m in t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
        {
            if (m.Name.Contains("Device") || m.Name.Contains("State"))
            {
                sb.AppendLine($"  method {m.Name}({string.Join(", ", Array.ConvertAll(m.GetParameters(), p => p.ParameterType.Name))})");
            }
        }

        var mainType = Type.GetType("UnityEditor.DeviceSimulation.DeviceSimulatorMain, UnityEditor.DeviceSimulatorModule");
        sb.AppendLine($"main type: {mainType}");
        if (mainType != null)
        {
            foreach (var f in mainType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (f.Name.ToLower().Contains("device") || f.Name.ToLower().Contains("info"))
                {
                    sb.AppendLine($"  main field {f.FieldType.Name} {f.Name}");
                }
            }

            foreach (var m in mainType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            {
                if (m.Name.ToLower().Contains("device"))
                {
                    sb.AppendLine($"  main method {m.Name}({string.Join(", ", Array.ConvertAll(m.GetParameters(), x => x.ParameterType.Name))})");
                }
            }
        }

        return sb.ToString();
    }

    public static string Open(string friendlyName)
    {
        var sb = new StringBuilder();
        try
        {
            var window = EditorWindow.GetWindow(WindowType);
            window.Show();

            object main = WindowType.GetField("m_Main", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(window);
            if (main == null)
            {
                return "m_Main пусто";
            }

            var mainType = main.GetType();
            mainType.GetMethod("UpdateDeviceList", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Invoke(main, null);

            var devices = (Array)mainType.GetProperty("devices", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .GetValue(main);
            var names = new StringBuilder();
            int index = -1;
            for (int i = 0; i < devices.Length; i++)
            {
                var asset = devices.GetValue(i) as UnityEngine.Object;
                var name = asset ? asset.name : "?";
                if (i < 8 || i > devices.Length - 4)
                {
                    names.Append(name).Append(" | ");
                }

                if (name == friendlyName)
                {
                    index = i;
                }
            }

            sb.AppendLine($"устройств в списке: {devices.Length}");
            sb.AppendLine($"крайние: {names}");
            sb.AppendLine($"нашли '{friendlyName}': индекс {index}");

            if (index >= 0)
            {
                mainType.GetProperty("deviceIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .SetValue(main, index);
                sb.AppendLine($"выбрано: {friendlyName}");
            }

            sb.AppendLine($"заголовок окна: {window.titleContent.text}");
        }
        catch (Exception e)
        {
            sb.AppendLine("FAILED " + e.GetType().Name + ": " + e.Message);
        }

        return sb.ToString();
    }
}
