namespace Arkanoid.Logic.Models
{
    /// <summary>
    /// Состояние игры
    /// </summary>
    public class GameStateModel
    {
        /// <summary>Текущий счет</summary>
        public int Score { get; set; }

        /// <summary>Оставшиеся жизни</summary>
        public int Lives { get; set; }

        /// <summary>Ширина игрового поля</summary>
        public int GameWidth { get; set; }

        /// <summary>Высота игрового поля</summary>
        public int GameHeight { get; set; }

        /// <summary>Флаг окончания игры</summary>
        public bool IsGameOver { get; set; }

        /// <summary>Флаг победы</summary>
        public bool IsGameWon { get; set; }

        /// <summary>Запущен ли мяч</summary>
        public bool IsBallLaunched { get; set; }

        /// <summary>На паузе ли игра</summary>
        public bool IsPaused { get; set; }
    }
}