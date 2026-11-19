using UnityEngine;

namespace AstraBricks
{
    /// <summary>Как бита слушает палец.</summary>
    public enum PaddleControlMode
    {
        /// <summary>Центр биты идёт под палец (абсолютное управление).</summary>
        Absolute = 0,
        /// <summary>Бита сдвигается на приращение движения пальца (свайп).</summary>
        Relative = 1
    }

    /// <summary>
    /// Единая точка настройки игровых параметров: поле, бита, мяч.
    /// Лежит в Assets/Config/GameConfig.asset, правится в инспекторе.
    /// Значения читаются каждый кадр, поэтому в Play их можно менять на ходу.
    /// </summary>
    [CreateAssetMenu(menuName = "Astra Bricks/Game Config", fileName = "GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [Header("Поле")]
        [Tooltip("Ширина рабочего поля в единицах. 1 единица = 1% ширины поля.")]
        public float fieldWidth = 100f;

        [Tooltip("Высота рабочего поля в единицах.")]
        public float fieldHeight = 170f;

        [Tooltip("Толщина стен и коллайдеров границ.")]
        public float wallThickness = 2f;

        [Header("Кирпичи")]
        [Tooltip("Колонок в сетке. Ширина кирпича = ширина поля / колонки.")]
        public int brickColumns = 10;

        [Tooltip("Рядов в сетке. Сетка растёт сверху вниз от верхнего края поля.")]
        public int brickRows = 20;

        [Tooltip("Высота кирпича в единицах. Ширина считается из числа колонок.")]
        public float brickHeight = 5f;

        [Tooltip("Отступ верхнего края сетки от верхнего края поля.")]
        public float brickTopOffset = 0f;

        [Header("Бита")]
        [Tooltip("Ширина биты в единицах.")]
        public float paddleWidth = 25f;

        [Tooltip("Высота биты в единицах.")]
        public float paddleHeight = 5f;

        [Tooltip("Высота центра биты над нижним краем поля.")]
        public float paddleHeightAboveBottom = 12f;

        [Tooltip("Как бита слушает палец: Absolute — центр биты под пальцем, Relative — свайп по приращению.")]
        public PaddleControlMode paddleControlMode = PaddleControlMode.Absolute;

        [Tooltip("Чувствительность свайпа. 1 = палец двигает биту один к одному.")]
        public float swipeSensitivity = 1f;

        [Tooltip("Скорость перемещения стрелками и A/D, единиц в секунду.")]
        public float keyboardSpeed = 120f;

        [Tooltip("Сглаживание догона цели, секунды. 0 = бита идёт за вводом жёстко.")]
        public float paddleFollowTime = 0.33f;

        [Tooltip("Максимальная скорость догона пальца в Absolute, единиц/с. 0 = без ограничения.")]
        public float paddleMaxCatchUpSpeed = 300f;

        [Tooltip("Максимальный угол отскока мяча от биты, градусы.")]
        public float boundDirection = 65f;

        [Header("Мяч")]
        [Tooltip("Скорость мяча, единиц в секунду.")]
        public float ballSpeed = 43.75f;

        [Tooltip("Радиус мяча в единицах.")]
        public float ballRadius = 2f;

        [Tooltip("Защита от горизонтали: при большем отклонении мяч подталкивает вверх.")]
        public float maxDeviation = 0.9f;

        [Tooltip("Случайное отклонение после отскока.")]
        public float randomDeviation = 0.1f;

        [Tooltip("Через сколько секунд мяч стартует сам, если игрок не запустил.")]
        public float autoStartDelay = 3f;

        [Tooltip("Разброс угла при старте, градусы.")]
        public float startAngleSpread = 5f;

        [Tooltip("Пауза между срабатываниями боковых лучей, кадры.")]
        public int sideRayCooldownFrames = 5;

        static GameConfig runtimeFallback;

        /// <summary>Возвращает назначенный ассет, а если его нет — временный со значениями по умолчанию.</summary>
        public static GameConfig Resolve(GameConfig assigned)
        {
            if (assigned != null) return assigned;
            if (runtimeFallback == null) runtimeFallback = CreateInstance<GameConfig>();
            return runtimeFallback;
        }
    }
}
