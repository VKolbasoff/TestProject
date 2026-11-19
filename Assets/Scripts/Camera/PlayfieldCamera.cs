using UnityEngine;

namespace AstraBricks
{
    /// <summary>
    /// Ортографическая камера, которая вписывает рабочее поле фиксированного размера
    /// (fieldWidth x fieldHeight в мировых единицах) в экран по меньшей стороне.
    /// Лишнее место по краям отдаётся фону, поле центрировано в начале координат.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class PlayfieldCamera : MonoBehaviour
    {
        [Tooltip("Ассет с параметрами игры. Если назначен — размеры поля берутся оттуда.")]
        public GameConfig config;

        [Tooltip("Ширина рабочего поля в мировых единицах. 100 единиц = 100% ширины поля.")]
        public float fieldWidth = 100f;

        [Tooltip("Высота рабочего поля в мировых единицах.")]
        public float fieldHeight = 170f;

        Camera cam;

        public Camera Camera => cam != null ? cam : (cam = GetComponent<Camera>());

        /// <summary>Видимая ширина в мировых единицах при текущем соотношении сторон.</summary>
        public float ViewWidth => 2f * Camera.orthographicSize * Camera.aspect;

        /// <summary>Видимая высота в мировых единицах.</summary>
        public float ViewHeight => 2f * Camera.orthographicSize;

        void OnEnable()
        {
            Apply();
        }

        void LateUpdate()
        {
            Apply();
        }

        float Width => config != null ? config.fieldWidth : fieldWidth;
        float Height => config != null ? config.fieldHeight : fieldHeight;

        public void Apply()
        {
            var c = Camera;
            float fieldWidth = Width;
            float fieldHeight = Height;
            if (c == null || fieldWidth <= 0f || fieldHeight <= 0f) return;

            float aspect = c.aspect > 0f ? c.aspect : 9f / 16f;

            c.orthographic = true;
            c.orthographicSize = Mathf.Max(fieldHeight * 0.5f, (fieldWidth * 0.5f) / aspect);

            var t = c.transform;
            var p = t.position;
            t.position = new Vector3(0f, 0f, p.z);
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.8f);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(Width, Height, 0.01f));
        }
    }
}
