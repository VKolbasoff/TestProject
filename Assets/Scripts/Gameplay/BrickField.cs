using System.Collections.Generic;
using UnityEngine;

namespace AstraBricks
{
    /// <summary>
    /// Раскладка кирпичей: сетка brickColumns x brickRows из GameConfig,
    /// кирпич занимает всю клетку, два типа чередуются шахматкой.
    /// Кирпичи пока заглушки — спрайт и коллайдер, без разрушения и очков.
    /// </summary>
    public class BrickField : MonoBehaviour
    {
        [Tooltip("Ассет с параметрами игры. Если пусто — значения по умолчанию.")]
        public GameConfig config;

        [Tooltip("Спрайт кирпича 1-го типа.")]
        public Sprite brickType1;

        [Tooltip("Спрайт кирпича 2-го типа.")]
        public Sprite brickType2;

        [Tooltip("Родитель для кирпичей. Если пусто — этот объект.")]
        public Transform parent;

        readonly List<GameObject> bricks = new List<GameObject>();
        int signature = int.MinValue;

        GameConfig Cfg => GameConfig.Resolve(config);

        void Awake()
        {
            Rebuild();
        }

        void Update()
        {
            int s = Signature();
            if (s != signature) Rebuild(s);
        }

        /// <summary>Отпечаток настроек раскладки: меняется — перестраиваем сетку.</summary>
        int Signature()
        {
            var cfg = Cfg;
            unchecked
            {
                int h = 17;
                h = h * 31 + cfg.brickColumns;
                h = h * 31 + cfg.brickRows;
                h = h * 31 + Mathf.RoundToInt(cfg.fieldWidth * 100f);
                h = h * 31 + Mathf.RoundToInt(cfg.fieldHeight * 100f);
                h = h * 31 + Mathf.RoundToInt(cfg.brickHeight * 100f);
                h = h * 31 + Mathf.RoundToInt(cfg.brickTopOffset * 100f);
                return h;
            }
        }

        public void Rebuild()
        {
            Rebuild(Signature());
        }

        public void Rebuild(int newSignature)
        {
            signature = newSignature;
            Clear();

            var cfg = Cfg;
            int cols = Mathf.Max(1, cfg.brickColumns);
            int rows = Mathf.Max(1, cfg.brickRows);
            float brickW = cfg.fieldWidth / cols;
            float brickH = cfg.brickHeight;
            float left = -cfg.fieldWidth * 0.5f;
            float top = cfg.fieldHeight * 0.5f - cfg.brickTopOffset;
            var root = parent != null ? parent : transform;
            var size = new Vector2(brickW, brickH);

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    bool first = ((row + col) & 1) == 0;
                    float x = left + (col + 0.5f) * brickW;
                    float y = top - (row + 0.5f) * brickH;
                    bricks.Add(CreateBrick(root, col, row, first ? 1 : 2, first ? brickType1 : brickType2,
                        new Vector3(x, y, 0f), size));
                }
            }
        }

        void Clear()
        {
            for (int i = 0; i < bricks.Count; i++)
            {
                var go = bricks[i];
                if (go == null) continue;
                if (Application.isPlaying) Destroy(go);
                else DestroyImmediate(go);
            }
            bricks.Clear();
        }

        GameObject CreateBrick(Transform root, int col, int row, int type, Sprite sprite, Vector3 position, Vector2 size)
        {
            var go = new GameObject($"Brick_{type}_{col}_{row}");
            go.transform.SetParent(root, false);
            go.transform.localPosition = position;

            var scale = SpriteScale(sprite, size);
            go.transform.localScale = scale;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;

            // Коллайдер живёт в локальных единицах, а объект масштабирован под клетку,
            // поэтому делим размер клетки на масштаб — в мире получится ровно size.
            var box = go.AddComponent<BoxCollider2D>();
            box.size = new Vector2(
                size.x / Mathf.Max(0.0001f, scale.x),
                size.y / Mathf.Max(0.0001f, scale.y));

            return go;
        }

        /// <summary>Масштаб, при котором спрайт ровно совпадает с клеткой.</summary>
        static Vector3 SpriteScale(Sprite sprite, Vector2 size)
        {
            if (sprite == null) return Vector3.one;
            var bounds = sprite.bounds.size;
            if (bounds.x <= 0f || bounds.y <= 0f) return Vector3.one;
            return new Vector3(size.x / bounds.x, size.y / bounds.y, 1f);
        }
    }
}
