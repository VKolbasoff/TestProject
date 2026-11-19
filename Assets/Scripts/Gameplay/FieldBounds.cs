using UnityEngine;

namespace AstraBricks
{
    /// <summary>
    /// Расставляет стены по границам поля из GameConfig. Одно место правды для размеров поля.
    /// </summary>
    public class FieldBounds : MonoBehaviour
    {
        [Tooltip("Ассет с параметрами игры.")]
        public GameConfig config;

        [Tooltip("Левая стена.")]
        public Transform left;

        [Tooltip("Правая стена.")]
        public Transform right;

        [Tooltip("Верхняя стена.")]
        public Transform top;

        GameConfig Cfg => GameConfig.Resolve(config);

        void Start()
        {
            Apply();
        }

        void Update()
        {
            Apply();
        }

        public void Apply()
        {
            var cfg = Cfg;
            float halfW = cfg.fieldWidth * 0.5f;
            float topY = cfg.fieldHeight * 0.5f;
            float t = cfg.wallThickness;

            Place(left, new Vector3(-halfW - t * 0.5f, 0f, 0f), new Vector2(t, cfg.fieldHeight + t * 2f));
            Place(right, new Vector3(halfW + t * 0.5f, 0f, 0f), new Vector2(t, cfg.fieldHeight + t * 2f));
            Place(top, new Vector3(0f, topY + t * 0.5f, 0f), new Vector2(cfg.fieldWidth + t * 2f, t));
        }

        static void Place(Transform target, Vector3 position, Vector2 size)
        {
            if (target == null) return;

            if (target.position != position) target.position = position;

            var box = target.GetComponent<BoxCollider2D>();
            if (box != null && box.size != size) box.size = size;
        }
    }
}
