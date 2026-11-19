using UnityEngine;
using UnityEngine.InputSystem;

namespace AstraBricks
{
    /// <summary>
    /// Бита. Управление пальцем/мышью (Absolute — центр под пальцем, Relative — свайп)
    /// и стрелки на PC. Режим задаётся в GameConfig.
    /// Размеры, скорость, сглаживание и угол отскока берутся из GameConfig;
    /// коллайдер и дочерний спрайт каждый кадр подгоняются под конфиг.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class PaddleController : MonoBehaviour
    {
        [Tooltip("Ассет с параметрами игры. Если пусто — значения по умолчанию.")]
        public GameConfig config;

        [Tooltip("Камера, по которой считается перевод пикселей в единицы.")]
        public Camera targetCamera;

        [Tooltip("Дочерний объект со спрайтом биты.")]
        public Transform visual;

        BoxCollider2D box;
        float targetX;
        Vector2 lastPointerScreen;
        bool pointerHeld;

        GameConfig Cfg => GameConfig.Resolve(config);

        public float HalfWidth => box != null ? box.size.x * 0.5f : 0f;
        public float HalfHeight => box != null ? box.size.y * 0.5f : 0f;

        /// <summary>Верхний край биты: от него мяч стартует и отскакивает.</summary>
        public float TopEdgeY => transform.position.y + HalfHeight;

        float FieldBottom => -Cfg.fieldHeight * 0.5f;

        void Awake()
        {
            box = GetComponent<BoxCollider2D>();
            if (targetCamera == null) targetCamera = Camera.main;
            if (visual == null) visual = transform.Find("Sprite");

            var p = transform.position;
            transform.position = new Vector3(p.x, FieldBottom + Cfg.paddleHeightAboveBottom, p.z);
            targetX = transform.position.x;
            ApplySize();
        }

        void Update()
        {
            var cfg = Cfg;
            float dt = Time.deltaTime;
            if (targetCamera == null) targetCamera = Camera.main;

            ApplySize();
            PlaceAtHeight();

            float axis = ReadKeyboardAxis();
            if (!Mathf.Approximately(axis, 0f)) targetX += axis * cfg.keyboardSpeed * dt;

            ReadPointer();

            float limit = Mathf.Max(0f, cfg.fieldWidth * 0.5f - HalfWidth);
            targetX = Mathf.Clamp(targetX, -limit, limit);

            float x = transform.position.x;
            float step = targetX - x;
            if (cfg.paddleFollowTime > 0.0001f)
                step *= 1f - Mathf.Exp(-dt / cfg.paddleFollowTime);
            if (cfg.paddleControlMode == PaddleControlMode.Absolute && cfg.paddleMaxCatchUpSpeed > 0.0001f)
                step = Mathf.Clamp(step, -cfg.paddleMaxCatchUpSpeed * dt, cfg.paddleMaxCatchUpSpeed * dt);
            x += step;

            var p = transform.position;
            transform.position = new Vector3(x, p.y, p.z);
        }

        void PlaceAtHeight()
        {
            float y = FieldBottom + Cfg.paddleHeightAboveBottom;
            var p = transform.position;
            if (!Mathf.Approximately(p.y, y)) transform.position = new Vector3(p.x, y, p.z);
        }

        /// <summary>Подгоняет коллайдер и спрайт под paddleWidth/paddleHeight из конфига.</summary>
        public void ApplySize()
        {
            var cfg = Cfg;

            if (box == null) box = GetComponent<BoxCollider2D>();
            if (box != null)
            {
                var size = new Vector2(cfg.paddleWidth, cfg.paddleHeight);
                if (box.size != size) box.size = size;
            }

            if (visual == null) visual = transform.Find("Sprite");
            var sr = visual != null ? visual.GetComponent<SpriteRenderer>() : null;
            if (sr == null || sr.sprite == null) return;

            var spriteSize = sr.sprite.bounds.size;
            if (spriteSize.x <= 0f || spriteSize.y <= 0f) return;

            visual.localScale = new Vector3(
                cfg.paddleWidth / spriteSize.x,
                cfg.paddleHeight / spriteSize.y,
                1f);
        }

        float ReadKeyboardAxis()
        {
            var kb = Keyboard.current;
            if (kb == null) return 0f;

            float axis = 0f;
            if (kb.leftArrowKey.isPressed || kb.aKey.isPressed) axis -= 1f;
            if (kb.rightArrowKey.isPressed || kb.dKey.isPressed) axis += 1f;
            return axis;
        }

        void ReadPointer()
        {
            Vector2 pointer = Vector2.zero;
            bool held = false;

            var touch = Touchscreen.current;
            if (touch != null && touch.primaryTouch.press.isPressed)
            {
                pointer = touch.primaryTouch.position.ReadValue();
                held = true;
            }
            else
            {
                var mouse = Mouse.current;
                if (mouse != null && mouse.leftButton.isPressed)
                {
                    pointer = mouse.position.ReadValue();
                    held = true;
                }
            }

            if (!held)
            {
                pointerHeld = false;
                return;
            }

            if (Cfg.paddleControlMode == PaddleControlMode.Absolute)
            {
                targetX = PointerWorldX(pointer);
                pointerHeld = true;
                lastPointerScreen = pointer;
                return;
            }

            if (!pointerHeld)
            {
                pointerHeld = true;
                lastPointerScreen = pointer;
                return;
            }

            targetX += (pointer.x - lastPointerScreen.x) * WorldPerPixel() * Cfg.swipeSensitivity;
            lastPointerScreen = pointer;
        }

        /// <summary>Сколько мировых единиц приходится на один пиксель экрана по X.</summary>
        float WorldPerPixel()
        {
            if (targetCamera == null || Screen.width <= 0) return 0f;
            return 2f * targetCamera.orthographicSize * targetCamera.aspect / Screen.width;
        }

        /// <summary>Мировая X-координата точки экрана.</summary>
        float PointerWorldX(Vector2 screenPoint)
        {
            if (targetCamera == null) return 0f;
            return targetCamera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, 0f)).x;
        }

        /// <summary>Направление отскока от точки удара: от -boundDirection до +boundDirection.</summary>
        public Vector2 GetBoundDirection(Vector2 point)
        {
            float half = Mathf.Max(0.0001f, HalfWidth);
            float offset = Mathf.Clamp((point.x - transform.position.x) / half, -1f, 1f);
            float angle = Cfg.boundDirection * offset * Mathf.Deg2Rad;
            return new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)).normalized;
        }
    }
}
