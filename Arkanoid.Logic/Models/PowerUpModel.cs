using Arkanoid.Logic.Enums;

namespace Arkanoid.Logic.Models
{
    /// <summary>
    /// Модель падающего усиления
    /// </summary>
    public class PowerUpModel
    {
        /// <summary>Координата X усиления</summary>
        public int X { get; set; }

        /// <summary>Координата Y усиления</summary>
        public int Y { get; set; }

        /// <summary>Размер усиления</summary>
        public int Size { get; set; }

        /// <summary>Скорость падения</summary>
        public int SpeedY { get; set; }

        /// <summary>Тип усиления</summary>
        public PowerUpType Type { get; set; }

        /// <summary>Активно ли усиление</summary>
        public bool IsActive { get; set; }
    }
}