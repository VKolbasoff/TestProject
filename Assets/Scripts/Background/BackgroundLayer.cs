using UnityEngine;

namespace AstraBricks
{
    /// <summary>
    /// Слой бесшовного тайлящегося фона.
    /// Размер подобран так, чтобы перекрывать камеру с запасом, а сдвиг идёт по кругу
    /// ровно на один тайл — стыка не видно, фон «бесконечный».
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    public class BackgroundLayer : MonoBehaviour
    {
        [Tooltip("Камера, под которую подгоняется размер слоя. Если пусто — Camera.main.")]
        public Camera targetCamera;

        [Tooltip("Скорость дрейфа в мировых единицах в секунду.")]
        public Vector2 scrollSpeed = new Vector2(0.6f, 0.2f);

        [Tooltip("Масштаб одного тайла. 1 = размер спрайта как есть, 2 = тайл вдвое крупнее.")]
        public float tileScale = 1f;

        [Tooltip("Запас в тайлах с каждой стороны, чтобы при сдвиге не показался край.")]
        public float overscanTiles = 1f;

        SpriteRenderer sr;
        float spriteW = 1f;
        float spriteH = 1f;
        Vector2 lastSize = new Vector2(-1f, -1f);

        float WorldTileW => spriteW * Mathf.Max(0.0001f, tileScale);
        float WorldTileH => spriteH * Mathf.Max(0.0001f, tileScale);

        void OnEnable()
        {
            sr = GetComponent<SpriteRenderer>();
            RefreshSpriteMetrics();
            Apply();
        }

        void Update()
        {
            Apply();
            Scroll();
        }

        void RefreshSpriteMetrics()
        {
            if (sr == null) sr = GetComponent<SpriteRenderer>();
            if (sr == null || sr.sprite == null) return;
            spriteW = sr.sprite.bounds.size.x;
            spriteH = sr.sprite.bounds.size.y;
        }

        void Scroll()
        {
            if (!Application.isPlaying) return;
            if (scrollSpeed == Vector2.zero) return;

            float t = Time.unscaledTime;
            if (t <= 0f) return;

            var p = transform.localPosition;
            transform.localPosition = new Vector3(
                Mathf.Repeat(scrollSpeed.x * t, WorldTileW) / tileScale,
                Mathf.Repeat(scrollSpeed.y * t, WorldTileH) / tileScale,
                p.z);
        }

        public void Apply()
        {
            RefreshSpriteMetrics();
            if (spriteW <= 0f || spriteH <= 0f) return;

            var cam = targetCamera != null ? targetCamera : Camera.main;
            if (cam == null || !cam.orthographic) return;

            float viewH = 2f * cam.orthographicSize;
            float viewW = viewH * cam.aspect;

            if (!Mathf.Approximately(transform.localScale.x, tileScale) ||
                !Mathf.Approximately(transform.localScale.y, tileScale))
            {
                transform.localScale = new Vector3(tileScale, tileScale, 1f);
            }

            // В локальных координатах: видимая зона / масштаб + запас в тайлах спрайта.
            var size = new Vector2(
                viewW / tileScale + spriteW * overscanTiles * 2f,
                viewH / tileScale + spriteH * overscanTiles * 2f);

            if ((size - lastSize).sqrMagnitude > 0.0001f)
            {
                lastSize = size;
                sr.drawMode = SpriteDrawMode.Tiled;
                sr.tileMode = SpriteTileMode.Continuous;
                sr.size = size;
            }
        }
    }
}
