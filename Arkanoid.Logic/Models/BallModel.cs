namespace Arkanoid.Logic.Models
{
    /// <summary>
    /// Модель мяча
    /// </summary>
    public class BallModel
    {
        /// <summary>Координата X мяча</summary>
        public int X { get; set; }

        /// <summary>Координата Y мяча</summary>
        public int Y { get; set; }

        /// <summary>Размер мяча (диаметр)</summary>
        public int Size { get; set; }

        /// <summary>Скорость по горизонтали</summary>
        public int SpeedX { get; set; }

        /// <summary>Скорость по вертикали</summary>
        public int SpeedY { get; set; }

        /// <summary>Урон, наносимый мячом</summary>
        public int Damage { get; set; } = 1;

        /// <summary>Активен ли мяч</summary>
        public bool IsActive { get; set; } = true;
    }
}
