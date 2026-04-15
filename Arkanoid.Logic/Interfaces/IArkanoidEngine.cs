using System.Collections.Generic;
using Arkanoid.Logic.Models;

namespace Arkanoid.Logic.Interfaces
{
    /// <summary>
    /// Интерфейс игрового движка. Определяет контракт для всей игровой логики.
    /// </summary>
    public interface IArkanoidEngine
    {
        /// <summary>Получает модель платформы с текущими координатами и размерами</summary>
        PlatformModel Platform { get; }

        /// <summary>Получает список всех активных мячей в игре</summary>
        List<BallModel> Balls { get; }

        /// <summary>Получает текущее состояние игры (счет, жизни, флаги)</summary>
        GameStateModel GameState { get; }

        /// <summary>Получает список всех кирпичей на игровом поле</summary>
        IReadOnlyList<BrickModel> Bricks { get; }

        /// <summary>Получает список всех падающих усилений</summary>
        List<PowerUpModel> PowerUps { get; }

        /// <summary>Обновляет состояние игры. Вызывается каждый кадр.</summary>
        void Update();

        /// <summary>Устанавливает платформу в указанную позицию по X</summary>
        void SetPlatformPosition(int x);

        /// <summary>Запускает все мячи с платформы</summary>
        void LaunchBall();

        /// <summary>Полностью перезапускает игру (начальное состояние)</summary>
        void RestartGame();

        /// <summary>Переключает состояние паузы (вкл/выкл)</summary>
        void TogglePause();

        /// <summary>Принудительно завершает игру (проигрыш)</summary>
        void GameOver();
    }
}
