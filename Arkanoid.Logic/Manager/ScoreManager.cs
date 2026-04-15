using System;
using System.Collections.Generic;
using System.Linq;
using Arkanoid.Logic.Models;

namespace Arkanoid.Logic.Manager
{
    /// <summary>
    /// Менеджер для управления таблицей рекордов
    /// </summary>
    public static class ScoreManager
    {
        private static List<ScoreRecord> scores = new List<ScoreRecord>();

        /// <summary>Сбрасывает все рекорды</summary>
        public static void ResetScores()
        {
            scores.Clear();
        }

        /// <summary>Добавляет новый рекорд</summary>
        public static void AddScore(string name, int score, int lives, string type)
        {
            scores.Add(new ScoreRecord
            {
                PlayerName = name,
                Score = score,
                Lives = lives,
                Date = DateTime.Now,
                GameEndType = type
            });

            scores = scores.OrderByDescending(s => s.Score).Take(10).ToList();
        }

        /// <summary>Получает все рекорды (отсортированные по убыванию)</summary>
        public static List<ScoreRecord> GetScores()
        {
            return scores.OrderByDescending(s => s.Score).ToList();
        }
    }
}
