using Arkanoid.Logic.Enums;

namespace Arkanoid.Logic.Models
{
    /// <summary>
    /// Модель кирпича
    /// </summary>
    public class BrickModel
    {
        /// <summary>Координата X кирпича</summary>
        public int X { get; set; }

        /// <summary>Координата Y кирпича</summary>
        public int Y { get; set; }

        /// <summary>Ширина кирпича</summary>
        public int Width { get; set; }

        /// <summary>Высота кирпича</summary>
        public int Height { get; set; }

        /// <summary>Номер ряда (для цвета)</summary>
        public int Row { get; set; }

        /// <summary>Текущее здоровье</summary>
        public int Health { get; set; }

        /// <summary>Максимальное здоровье</summary>
        public int MaxHealth { get; set; }

        /// <summary>Счетчик кадров после удара</summary>
        public int HitFrames { get; set; }

        /// <summary>Активен ли кирпич</summary>
        public bool IsActive { get; set; }

        /// <summary>Был ли недавно ударен</summary>
        public bool IsHit { get; set; }

        /// <summary>Есть ли усиление</summary>
        public bool HasPowerUp { get; set; }

        /// <summary>Тип усиления</summary>
        public PowerUpType PowerUpType { get; set; }
    }
}