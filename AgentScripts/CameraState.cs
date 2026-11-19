using System.Text;
using UnityEngine;

public static class CameraState
{
    public static string Run()
    {
        var sb = new StringBuilder();
        var cam = Camera.main;
        if (!cam)
        {
            sb.AppendLine("Camera.main не найден");
            return sb.ToString();
        }

        sb.AppendLine($"Screen={Screen.width}x{Screen.height} dpi={Screen.dpi} fullScreen={Screen.fullScreen}");
        sb.AppendLine($"cam pixelRect={cam.pixelRect} pixels={cam.pixelWidth}x{cam.pixelHeight} aspect={cam.aspect:F4}");
        sb.AppendLine($"ortho={cam.orthographicSize} visible={cam.orthographicSize * 2f * cam.aspect:F1}x{cam.orthographicSize * 2f:F1} units");
        sb.AppendLine($"px per unit={cam.pixelHeight / (cam.orthographicSize * 2f):F4} (width) {cam.pixelWidth / (cam.orthographicSize * 2f * cam.aspect):F4}");
        return sb.ToString();
    }
}
