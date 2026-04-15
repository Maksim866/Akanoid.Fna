namespace Arkanoid.Logic.Models
{
    /// <summary>
    /// Модель платформы
    /// </summary>
    public class PlatformModel
    {
        /// <summary>Координата X платформы</summary>
        public int X { get; set; }

        /// <summary>Координата Y платформы</summary>
        public int Y { get; set; }

        /// <summary>Ширина платформы</summary>
        public int Width { get; set; }

        /// <summary>Высота платформы</summary>
        public int Height { get; set; }
    }
}