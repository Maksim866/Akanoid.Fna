using Arkanoid.Logic.Constants;
using Arkanoid.Logic.Enums;
using Arkanoid.Logic.Interfaces;
using Arkanoid.Logic.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Arkanoid.Logic.Engine
{
    /// <summary>
    /// Игровой движок арканоида. Содержит всю логику игры: физику, столкновения, 
    /// подсчет очков, управление усилениями и дополнительными мячами.
    /// </summary>
    public class ArkanoidEngine : IArkanoidEngine
    {
        private PlatformModel platform;
        private List<BallModel> balls;
        private List<BrickModel> bricks;
        private List<PowerUpModel> powerUps;
        private GameStateModel gameState;
        private Random random = new();
        private int wideTimer;
        private int originalPlatformWidth;

        /// <summary>Получает модель платформы</summary>
        public PlatformModel Platform => platform;

        /// <summary>Получает список всех активных мячей</summary>
        public List<BallModel> Balls => balls;

        /// <summary>Получает состояние игры</summary>
        public GameStateModel GameState => gameState;

        /// <summary>Получает список всех кирпичей</summary>
        public IReadOnlyList<BrickModel> Bricks => bricks;

        /// <summary>Получает список всех падающих усилений</summary>
        public List<PowerUpModel> PowerUps => powerUps;

        /// <summary>Инициализирует новый экземпляр игрового движка</summary>
        public ArkanoidEngine(int width, int height)
        {
            gameState = new GameStateModel
            {
                GameWidth = width,
                GameHeight = height,
                Lives = ArkanoidConstants.StartLives
            };

            platform = new();
            balls = [];
            bricks = [];
            powerUps = [];

            RestartGame();
        }

        /// <summary>Полностью перезапускает игру</summary>
        public void RestartGame()
        {
            // Платформа
            platform.X = (gameState.GameWidth - ArkanoidConstants.PlatformWidth) / 2;
            platform.Y = gameState.GameHeight - ArkanoidConstants.PlatformYOffset;
            platform.Width = ArkanoidConstants.PlatformWidth;
            platform.Height = ArkanoidConstants.PlatformHeight;
            originalPlatformWidth = ArkanoidConstants.PlatformWidth;

            // Мячи
            balls.Clear();
            balls.Add(new BallModel
            {
                X = (gameState.GameWidth - ArkanoidConstants.BallSize) / 2,
                Y = platform.Y - ArkanoidConstants.BallSize - ArkanoidConstants.BallPlatformOffset,
                Size = ArkanoidConstants.BallSize,
                Damage = 1,
                IsActive = true
            });

            // Кирпичи
            bricks.Clear();
            var startX = (gameState.GameWidth - ArkanoidConstants.BrickWidth * ArkanoidConstants.BricksPerRow) / 2;

            var allBricks = new List<(int, int)>();
            for (var row = 0; row < ArkanoidConstants.BrickRows; row++)
            {
                for (var column = 0; column < ArkanoidConstants.BricksPerRow; column++)
                {
                    allBricks.Add((row, column));
                }
            }

            allBricks = allBricks.OrderBy(_ => random.Next()).ToList();

            var powerUpTypes = new List<PowerUpType>();
            var powerUpCount = (int)(allBricks.Count * ArkanoidConstants.PowerUpChance);

            for (var i = 0; i < powerUpCount / 3; i++)
            {
                powerUpTypes.Add(PowerUpType.ExtraBall);
                powerUpTypes.Add(PowerUpType.DamageBoost);
                powerUpTypes.Add(PowerUpType.WidePaddle);
            }
            powerUpTypes = powerUpTypes.OrderBy(_ => random.Next()).ToList();

            var powerUpMap = new Dictionary<(int, int), PowerUpType>();
            for (var i = 0; i < powerUpTypes.Count; i++)
            {
                powerUpMap[allBricks[i]] = powerUpTypes[i];
            }

            for (var row = 0; row < ArkanoidConstants.BrickRows; row++)
            {
                for (var column = 0; column < ArkanoidConstants.BricksPerRow; column++)
                {
                    var brick = new BrickModel
                    {
                        X = startX + column * ArkanoidConstants.BrickWidth,
                        Y = ArkanoidConstants.BrickStartY + row * ArkanoidConstants.BrickHeight,
                        Width = ArkanoidConstants.BrickWidth,
                        Height = ArkanoidConstants.BrickHeight,
                        IsActive = true,
                        Row = row,
                        Health = ArkanoidConstants.BrickHealthByRow[row],
                        MaxHealth = ArkanoidConstants.BrickHealthByRow[row],
                        HasPowerUp = powerUpMap.ContainsKey((row, column)),
                        PowerUpType = powerUpMap.ContainsKey((row, column)) ? powerUpMap[(row, column)] : PowerUpType.ExtraBall
                    };
                    bricks.Add(brick);
                }
            }

            powerUps.Clear();

            gameState.Score = 0;
            gameState.Lives = ArkanoidConstants.StartLives;
            gameState.IsGameOver = false;
            gameState.IsGameWon = false;
            gameState.IsBallLaunched = false;
            gameState.IsPaused = false;
            wideTimer = 0;
        }

        /// <summary>Запускает все мячи с платформы</summary>
        public void LaunchBall()
        {
            if (gameState is { IsBallLaunched: false, IsGameOver: false, IsGameWon: false })
            {
                gameState.IsBallLaunched = true;
                foreach (var ball in balls.Where(b => b.IsActive && b.SpeedX == 0 && b.SpeedY == 0))
                {
                    var angle = (random.NextDouble() * (ArkanoidConstants.MaxLaunchAngle * 2) - ArkanoidConstants.MaxLaunchAngle)
                        * Math.PI / ArkanoidConstants.DegreesToRadiansDivisor;
                    ball.SpeedX = (int)(ArkanoidConstants.BallBaseSpeed * Math.Sin(angle));
                    ball.SpeedY = -(int)(ArkanoidConstants.BallBaseSpeed * Math.Cos(angle));

                    NormalizeBallSpeed(ball);
                }
            }
        }

        /// <summary>Нормализует скорость мяча до базовой</summary>
        private void NormalizeBallSpeed(BallModel ball)
        {
            if (ball.SpeedX == 0 && ball.SpeedY == 0)
            {
                return;
            }

            var currentSpeed = (float)Math.Sqrt(ball.SpeedX * ball.SpeedX + ball.SpeedY * ball.SpeedY);
            var scale = ArkanoidConstants.BallBaseSpeed / currentSpeed;

            ball.SpeedX = (int)(ball.SpeedX * scale);
            ball.SpeedY = (int)(ball.SpeedY * scale);

            if (Math.Abs(ball.SpeedY) < ArkanoidConstants.MinSpeedValue)
            {
                ball.SpeedY = ball.SpeedY < 0 ? -ArkanoidConstants.MinSpeedValue : ArkanoidConstants.MinSpeedValue;
            }
            if (Math.Abs(ball.SpeedX) < ArkanoidConstants.MinSpeedValue)
            {
                ball.SpeedX = ball.SpeedX < 0 ? -ArkanoidConstants.MinSpeedValue : ArkanoidConstants.MinSpeedValue;
            }
        }

        /// <summary>Обновляет состояние игры. Вызывается каждый кадр.</summary>
        public void Update()
        {
            if (gameState.IsGameOver || gameState.IsGameWon || gameState.IsPaused)
            {
                return;
            }

            // Эффекты ударов
            foreach (var brick in bricks.Where(b => b.IsHit))
            {
                brick.HitFrames--;
                if (brick.HitFrames <= 0)
                {
                    brick.IsHit = false;
                }
            }

            // Широкая платформа
            if (wideTimer > 0)
            {
                wideTimer--;
                if (wideTimer <= 0)
                {
                    platform.Width = originalPlatformWidth;
                }
            }

            // Падающие усиления
            for (var i = powerUps.Count - 1; i >= 0; i--)
            {
                var powerUp = powerUps[i];
                if (!powerUp.IsActive)
                {
                    continue;
                }

                powerUp.Y += ArkanoidConstants.PowerUpSpeed;

                var powerUpRect = new Rectangle(powerUp.X, powerUp.Y, powerUp.Size, powerUp.Size);
                var platformRect = new Rectangle(platform.X, platform.Y, platform.Width, platform.Height);

                if (powerUpRect.IntersectsWith(platformRect))
                {
                    switch (powerUp.Type)
                    {
                        case PowerUpType.ExtraBall:
                            var newBall = new BallModel
                            {
                                X = platform.X + platform.Width / 2 - ArkanoidConstants.BallSize / 2,
                                Y = platform.Y - ArkanoidConstants.BallSize - ArkanoidConstants.BallPlatformOffset,
                                Size = ArkanoidConstants.BallSize,
                                Damage = balls[0].Damage,
                                IsActive = true
                            };
                            if (gameState.IsBallLaunched)
                            {
                                var angle = (random.NextDouble() * (ArkanoidConstants.MaxPowerUpAngle * 2) - ArkanoidConstants.MaxPowerUpAngle)
                                    * Math.PI / ArkanoidConstants.DegreesToRadiansDivisor;
                                newBall.SpeedX = (int)(ArkanoidConstants.BallBaseSpeed * Math.Sin(angle));
                                newBall.SpeedY = -(int)(ArkanoidConstants.BallBaseSpeed * Math.Cos(angle));
                                NormalizeBallSpeed(newBall);
                            }
                            balls.Add(newBall);
                            break;

                        case PowerUpType.DamageBoost:
                            foreach (var ball in balls)
                            {
                                ball.Damage++;
                            }
                            break;

                        case PowerUpType.WidePaddle:
                            if (wideTimer <= 0)
                            {
                                originalPlatformWidth = platform.Width;
                            }
                            platform.Width = ArkanoidConstants.WidePaddleWidth;
                            wideTimer = ArkanoidConstants.WidePaddleDuration;
                            break;
                    }
                    powerUp.IsActive = false;
                }
                else if (powerUp.Y > gameState.GameHeight)
                {
                    powerUp.IsActive = false;
                }
            }
            powerUps.RemoveAll(p => !p.IsActive);

            var hasActiveBall = false;
            foreach (var ball in balls)
            {
                if (!ball.IsActive)
                {
                    continue;
                }

                if (!gameState.IsBallLaunched)
                {
                    ball.X = platform.X + platform.Width / 2 - ball.Size / 2;
                    ball.Y = platform.Y - ball.Size - ArkanoidConstants.BallPlatformOffset;
                    continue;
                }

                hasActiveBall = true;

                ball.X += ball.SpeedX;
                ball.Y += ball.SpeedY;

                // Стены
                if (ball.X <= 0)
                {
                    ball.X = 0;
                    ball.SpeedX = Math.Abs(ball.SpeedX);
                    NormalizeBallSpeed(ball);
                }
                else if (ball.X + ball.Size >= gameState.GameWidth)
                {
                    ball.X = gameState.GameWidth - ball.Size;
                    ball.SpeedX = -Math.Abs(ball.SpeedX);
                    NormalizeBallSpeed(ball);
                }

                if (ball.Y <= 0)
                {
                    ball.Y = 0;
                    ball.SpeedY = Math.Abs(ball.SpeedY);
                    NormalizeBallSpeed(ball);
                }

                // Платформа
                if (ball.X < platform.X + platform.Width && ball.X + ball.Size > platform.X &&
                    ball.Y < platform.Y + platform.Height && ball.Y + ball.Size > platform.Y && ball.SpeedY > 0)
                {
                    ball.Y = platform.Y - ball.Size;

                    var hitPosition = (float)(ball.X + ball.Size / 2 - (platform.X + platform.Width / 2)) / (platform.Width / 2);
                    hitPosition = Math.Max(-1, Math.Min(1, hitPosition));

                    var newSpeedX = (int)(ArkanoidConstants.BallBaseSpeed * hitPosition * ArkanoidConstants.PlatformBounceFactor);
                    if (Math.Abs(newSpeedX) < ArkanoidConstants.BallMinSpeed)
                    {
                        newSpeedX = hitPosition > 0 ? ArkanoidConstants.BallMinSpeed : -ArkanoidConstants.BallMinSpeed;
                    }

                    var newSpeedY = (int)Math.Sqrt(
                        ArkanoidConstants.BallBaseSpeed * ArkanoidConstants.BallBaseSpeed -
                        newSpeedX * newSpeedX);
                    if (newSpeedY < ArkanoidConstants.BallMinSpeed)
                    {
                        newSpeedY = ArkanoidConstants.BallMinSpeed;
                        newSpeedX = (int)Math.Sqrt(
                            ArkanoidConstants.BallBaseSpeed * ArkanoidConstants.BallBaseSpeed -
                            newSpeedY * newSpeedY);
                        if (hitPosition < 0)
                        {
                            newSpeedX = -newSpeedX;
                        }
                    }

                    ball.SpeedX = newSpeedX;
                    ball.SpeedY = -newSpeedY;

                    NormalizeBallSpeed(ball);
                }

                // Кирпичи
                for (var j = bricks.Count - 1; j >= 0; j--)
                {
                    var brick = bricks[j];
                    if (!brick.IsActive)
                    {
                        continue;
                    }

                    if (ball.X < brick.X + brick.Width && ball.X + ball.Size > brick.X &&
                        ball.Y < brick.Y + brick.Height && ball.Y + ball.Size > brick.Y)
                    {
                        brick.Health -= ball.Damage;
                        brick.IsHit = true;
                        brick.HitFrames = ArkanoidConstants.HitEffectDuration;

                        if (brick.Health <= 0)
                        {
                            brick.IsActive = false;
                            gameState.Score += ArkanoidConstants.PointsPerBrick * brick.MaxHealth;
                            if (brick.HasPowerUp)
                            {
                                powerUps.Add(new PowerUpModel
                                {
                                    X = brick.X + brick.Width / 2 - ArkanoidConstants.PowerUpSize / 2,
                                    Y = brick.Y,
                                    Size = ArkanoidConstants.PowerUpSize,
                                    Type = brick.PowerUpType,
                                    IsActive = true,
                                    SpeedY = ArkanoidConstants.PowerUpSpeed
                                });
                            }
                        }

                        var overlapLeft = ball.X + ball.Size - brick.X;
                        var overlapRight = brick.X + brick.Width - ball.X;
                        var overlapTop = ball.Y + ball.Size - brick.Y;
                        var overlapBottom = brick.Y + brick.Height - ball.Y;
                        var minOverlap = Math.Min(Math.Min(overlapLeft, overlapRight), Math.Min(overlapTop, overlapBottom));

                        if (minOverlap == overlapLeft || minOverlap == overlapRight)
                        {
                            ball.SpeedX = -ball.SpeedX;
                        }
                        else
                        {
                            ball.SpeedY = -ball.SpeedY;
                        }

                        NormalizeBallSpeed(ball);
                        break;
                    }
                }

                if (ball.Y > gameState.GameHeight)
                {
                    ball.IsActive = false;
                }
            }

            // Если нет активных мячей
            if (!hasActiveBall && gameState.IsBallLaunched && !gameState.IsGameOver)
            {
                gameState.Lives--;
                if (gameState.Lives <= 0)
                {
                    gameState.IsGameOver = true;
                }
                else
                {
                    gameState.IsBallLaunched = false;
                    for (var i = balls.Count - 1; i > 0; i--)
                    {
                        balls.RemoveAt(i);
                    }
                    balls[0].X = platform.X + platform.Width / 2 - balls[0].Size / 2;
                    balls[0].Y = platform.Y - balls[0].Size - ArkanoidConstants.BallPlatformOffset;
                    balls[0].SpeedX = 0;
                    balls[0].SpeedY = 0;
                    balls[0].IsActive = true;
                }
            }

            // Победа
            if (bricks.All(b => !b.IsActive))
            {
                gameState.IsGameWon = true;
            }
        }

        /// <summary>Устанавливает платформу в указанную позицию</summary>
        public void SetPlatformPosition(int x)
        {
            var newX = Math.Max(0, Math.Min(x, gameState.GameWidth - platform.Width));
            platform.X = newX;
            if (!gameState.IsBallLaunched && balls.Count > 0)
            {
                balls[0].X = platform.X + platform.Width / 2 - balls[0].Size / 2;
            }
        }

        /// <summary>Переключает состояние паузы (вкл/выкл)</summary>
        public void TogglePause()
        {
            if (!gameState.IsGameOver && !gameState.IsGameWon && gameState.IsBallLaunched)
            {
                gameState.IsPaused = !gameState.IsPaused;
            }
        }

        /// <summary>Принудительно завершает игру</summary>
        public void GameOver()
        {
            gameState.IsGameOver = true;
        }
    }
}
