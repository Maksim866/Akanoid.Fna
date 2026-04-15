namespace Arkanoid.FNA
{
    /// <summary>
    /// Числовые константы для визуальной части игры
    /// </summary>
    public static class GameConstants
    {
        // === Экран ===
        /// <summary>Ширина экрана</summary>
        public const int ScreenWidth = 800;

        /// <summary>Высота экрана</summary>
        public const int ScreenHeight = 600;

        // === Движение ===
        /// <summary>Скорость движения платформы</summary>
        public const int PlatformMoveSpeed = 8;

        // === Прозрачность и эффекты ===
        /// <summary>Прозрачность границ кирпичей</summary>
        public const float BorderAlpha = 0.5f;

        /// <summary>Прозрачность трещин</summary>
        public const float CrackAlpha = 0.7f;

        /// <summary>Множитель эффекта мигания</summary>
        public const float FlashIntensityMultiplier = 0.5f;

        /// <summary>Делитель кадров для эффекта удара</summary>
        public const float HitFramesDivisor = 10.0f;

        // === Размеры ===
        /// <summary>Толщина границы кирпича</summary>
        public const int BrickBorderThickness = 2;

        /// <summary>Размер шрифта</summary>
        public const int FontSize = 12;

        // === Позиции текста ===
        /// <summary>Отступ текста от левого края</summary>
        public const int TextMarginLeft = 10;

        /// <summary>Отступ текста от верхнего края</summary>
        public const int TextMarginTop = 10;

        /// <summary>Вертикальный отступ между строками текста</summary>
        public const int TextLineSpacing = 18;

        /// <summary>Отступ легенды от низа экрана</summary>
        public const int LegendBottomOffset = 60;

        /// <summary>Позиция X первого элемента легенды</summary>
        public const int LegendItem1X = 10;

        /// <summary>Позиция X второго элемента легенды</summary>
        public const int LegendItem2X = 180;

        /// <summary>Позиция X третьего элемента легенды</summary>
        public const int LegendItem3X = 340;

        /// <summary>Y-позиция для сообщения о старте</summary>
        public const int StartMessageYOffset = 50;

        /// <summary>Y-позиция для первого сообщения</summary>
        public const int Message1YOffset = -20;

        /// <summary>Y-позиция для второго сообщения</summary>
        public const int Message2YOffset = 10;

        /// <summary>Y-позиция для отображения жизней</summary>
        public const int LivesYPosition = 28;
    }
}
