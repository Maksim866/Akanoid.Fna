namespace Arkanoid.Logic.Constants
{
    /// <summary>
    /// Централизованное хранилище всех констант игры
    /// </summary>
    public static class ArkanoidConstants
    {
        // === Платформа ===
        /// <summary>Ширина платформы по умолчанию</summary>
        public const int PlatformWidth = 100;

        /// <summary>Высота платформы</summary>
        public const int PlatformHeight = 20;

        /// <summary>Расстояние от нижнего края до платформы</summary>
        public const int PlatformYOffset = 50;

        /// <summary>Ширина платформы при усилении</summary>
        public const int WidePaddleWidth = 180;

        /// <summary>Длительность усиления широкой платформы (в кадрах)</summary>
        public const int WidePaddleDuration = 180;

        // === Мяч ===
        /// <summary>Размер мяча</summary>
        public const int BallSize = 15;

        /// <summary>Базовая скорость мяча</summary>
        public const int BallBaseSpeed = 8;

        /// <summary>Минимальная скорость мяча</summary>
        public const int BallMinSpeed = 4;

        /// <summary>Расстояние между мячом и платформой</summary>
        public const int BallPlatformOffset = 5;

        // === Кирпичи ===
        /// <summary>Ширина кирпича</summary>
        public const int BrickWidth = 60;

        /// <summary>Высота кирпича</summary>
        public const int BrickHeight = 20;

        /// <summary>Количество кирпичей в ряду</summary>
        public const int BricksPerRow = 10;

        /// <summary>Количество рядов кирпичей</summary>
        public const int BrickRows = 5;

        /// <summary>Начальная Y-координата кирпичей</summary>
        public const int BrickStartY = 50;

        /// <summary>Очки за уничтожение кирпича</summary>
        public const int PointsPerBrick = 10;

        // === Здоровье кирпичей по рядам ===
        /// <summary>Здоровье кирпичей для каждого ряда (сверху вниз)</summary>
        public static readonly int[] BrickHealthByRow = { 5, 4, 3, 2, 1 };

        // === Игровые параметры ===
        /// <summary>Начальное количество жизней</summary>
        public const int StartLives = 3;

        /// <summary>Коэффициент отскока от платформы</summary>
        public const float PlatformBounceFactor = 1.8f;

        /// <summary>Длительность эффекта удара (в кадрах)</summary>
        public const int HitEffectDuration = 5;

        // === Усиления ===
        /// <summary>Размер квадратика усиления</summary>
        public const int PowerUpSize = 20;

        /// <summary>Скорость падения усиления</summary>
        public const int PowerUpSpeed = 3;

        /// <summary>Процент кирпичей с усилениями</summary>
        public const double PowerUpChance = 0.6;

        // === Углы запуска ===
        /// <summary>Максимальный угол запуска мяча в градусах</summary>
        public const int MaxLaunchAngle = 5;

        /// <summary>Максимальный угол для усиления в градусах</summary>
        public const int MaxPowerUpAngle = 10;

        /// <summary>Делитель для перевода градусов в радианы</summary>
        public const int DegreesToRadiansDivisor = 180;

        // === Скорость ===
        /// <summary>Минимальное значение скорости</summary>
        public const int MinSpeedValue = 1;
    }
}