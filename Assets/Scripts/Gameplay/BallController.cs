using UnityEngine;

namespace AstraBricks
{
    /// <summary>
    /// Мяч. Движение ручное, без физики: шаг за кадр, луч вперёд, два боковых луча,
    /// отражение от нормали, от биты — угол от точки удара.
    /// Все числовые параметры берутся из GameConfig.
    /// </summary>
    public class BallController : MonoBehaviour
    {
        [Tooltip("Ассет с параметрами игры. Если пусто — значения по умолчанию.")]
        public GameConfig config;

        [Tooltip("Бита, с которой мяч стартует и от которой отскакивает.")]
        public PaddleController paddle;

        [Tooltip("Слои, с которыми считаются столкновения.")]
        public LayerMask collisionMask = ~0;

        Vector2 direction = Vector2.up;
        bool attached = true;
        float idleTimer;
        int lastSideHitFrame = -100;
        Vector3 lastVisualScale = Vector3.zero;

        GameConfig Cfg => GameConfig.Resolve(config);

        public bool Attached => attached;
        public float Radius => Cfg.ballRadius;

        void Awake()
        {
            if (paddle == null) paddle = FindFirstObjectByType<PaddleController>();
        }

        void Update()
        {
            ApplyVisualSize();

            if (attached)
            {
                FollowPaddle();
                idleTimer += Time.deltaTime;
                if (idleTimer >= Cfg.autoStartDelay || LaunchRequested())
                {
                    Launch();
                }
                return;
            }

            Move(Time.deltaTime);
        }

        /// <summary>Спрайт масштабируется так, чтобы визуальный радиус совпал с конфигом.</summary>
        void ApplyVisualSize()
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr == null || sr.sprite == null) return;

            var sprite = sr.sprite.bounds.size;
            if (sprite.x <= 0f || sprite.y <= 0f) return;

            float diameter = Cfg.ballRadius * 2f;
            var scale = new Vector3(diameter / sprite.x, diameter / sprite.y, 1f);
            if ((scale - lastVisualScale).sqrMagnitude > 0.000001f)
            {
                lastVisualScale = scale;
                transform.localScale = scale;
            }
        }

        bool LaunchRequested()
        {
            var mouse = UnityEngine.InputSystem.Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame) return true;

            var touch = UnityEngine.InputSystem.Touchscreen.current;
            if (touch != null && touch.primaryTouch.press.wasPressedThisFrame) return true;

            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null && kb.spaceKey.wasPressedThisFrame) return true;

            return false;
        }

        void FollowPaddle()
        {
            if (paddle == null) return;
            transform.position = new Vector3(
                paddle.transform.position.x,
                paddle.TopEdgeY + Cfg.ballRadius,
                transform.position.z);
        }

        void Launch()
        {
            attached = false;
            idleTimer = 0f;

            float angle = Random.Range(-Cfg.startAngleSpread, Cfg.startAngleSpread);
            direction = Quaternion.AngleAxis(angle, Vector3.forward) * Vector3.up;
            direction.Normalize();
        }

        void Move(float dt)
        {
            var cfg = Cfg;
            float step = cfg.ballSpeed * dt;
            if (step <= 0f) return;

            float radius = cfg.ballRadius;
            float fieldBottom = -cfg.fieldHeight * 0.5f;

            Vector2 position = transform.position;
            Vector2 delta = direction * step;

            var hit = Cast(position, delta, radius);
            if (hit.collider != null)
            {
                OnHit(hit, cfg);
                return;
            }

            Vector2 next = position + delta;
            if (next.y < fieldBottom - radius * 2f)
            {
                ResetToPaddle();
                return;
            }

            transform.position = new Vector3(next.x, next.y, transform.position.z);
        }

        RaycastHit2D Cast(Vector2 position, Vector2 delta, float radius)
        {
            float length = delta.magnitude;
            if (length <= 0f) return default;

            Vector2 dir = delta.normalized;
            Vector2 offset = dir * radius;

            var hit = Physics2D.Raycast(position + offset, dir, length, collisionMask);
            if (hit.collider != null) return hit;

            if (lastSideHitFrame + Cfg.sideRayCooldownFrames >= Time.frameCount) return default;

            Vector2 left = position + new Vector2(-offset.y, offset.x);
            hit = Physics2D.Raycast(left, dir, length, collisionMask);
            if (hit.collider != null)
            {
                lastSideHitFrame = Time.frameCount;
                return hit;
            }

            Vector2 right = position + new Vector2(offset.y, -offset.x);
            hit = Physics2D.Raycast(right, dir, length, collisionMask);
            if (hit.collider != null)
            {
                lastSideHitFrame = Time.frameCount;
                return hit;
            }

            return default;
        }

        void OnHit(RaycastHit2D hit, GameConfig cfg)
        {
            var bat = hit.collider.GetComponent<PaddleController>();

            if (bat != null)
            {
                direction = bat.GetBoundDirection(hit.point);
                transform.position = new Vector3(
                    transform.position.x,
                    bat.TopEdgeY + cfg.ballRadius,
                    transform.position.z);
                return;
            }

            direction = Vector2.Reflect(direction, hit.normal);

            if (Mathf.Abs(direction.x) > cfg.maxDeviation)
            {
                direction.y -= 1f - cfg.maxDeviation;
                direction.Normalize();
            }

            float rnd = direction.x > 0f
                ? Random.Range(0f, cfg.randomDeviation)
                : Random.Range(-cfg.randomDeviation, 0f);
            direction.x += rnd;
            direction.y -= rnd;
            direction.Normalize();
        }

        public void ResetToPaddle()
        {
            attached = true;
            idleTimer = 0f;
            direction = Vector2.up;
            FollowPaddle();
        }
    }
}
