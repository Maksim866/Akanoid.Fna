using System;

namespace Arkanoid.Logic.Models
{
    /// <summary>
    /// Запись в таблице рекордов
    /// </summary>
    public class ScoreRecord
    {
        /// <summary>Имя игрока</summary>
        public string PlayerName { get; set; }

        /// <summary>Тип окончания игры</summary>
        public string GameEndType { get; set; }

        /// <summary>Количество очков</summary>
        public int Score { get; set; }

        /// <summary>Оставшиеся жизни</summary>
        public int Lives { get; set; }

        /// <summary>Дата рекорда</summary>
        public DateTime Date { get; set; }
    }
}
