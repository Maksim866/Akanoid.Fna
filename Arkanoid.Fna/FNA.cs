using System;
using System.Collections.Generic;
using Arkanoid.Logic.Engine;
using Arkanoid.Logic.Enums;
using Arkanoid.Logic.Interfaces;
using Arkanoid.Logic.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Arkanoid.FNA
{
    /// <summary>
    /// Основной класс игры Arkanoid на движке FNA.
    /// Отвечает за отрисовку, ввод и связь с игровым движком.
    /// </summary>
    public class FNA : Game
    {
        private readonly GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private IArkanoidEngine engine;
        private Texture2D pixel;
        private BitmapFont font;
        private KeyboardState previousKeyboardState;

        // Простые цвета
        private static readonly Color[] brickColors = {
            new(231, 76, 60), new(230, 126, 34),
            new(241, 196, 15), new(46, 204, 113),
            new(52, 152, 219)
        };

        // Цвета для типов усилений
        private static readonly Dictionary<PowerUpType, Color> powerUpColors = new()
        {
            { PowerUpType.ExtraBall, new(142, 68, 173) },
            { PowerUpType.DamageBoost, new(192, 57, 43) },
            { PowerUpType.WidePaddle, new(22, 160, 133) }
        };

        private const int Width = 800, Height = 600;

        /// <summary>
        /// Инициализирует новый экземпляр игры
        /// </summary>
        public FNA()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = Width;
            graphics.PreferredBackBufferHeight = Height;
            IsMouseVisible = true;
        }

        /// <summary>
        /// Инициализирует игровые компоненты
        /// </summary>
        protected override void Initialize()
        {
            engine = new ArkanoidEngine(Width, Height);
            base.Initialize();
        }

        /// <summary>
        /// Загружает контент игры (текстуры, шрифты)
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            try
            {
                font = new BitmapFont(GraphicsDevice, "Arial", 12);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка шрифта: {ex.Message}");
                font = null;
            }
        }

        /// <summary>
        /// Обновляет состояние игры каждый кадр
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            var kb = Keyboard.GetState();

            // Управление платформой
            if (!engine.GameState.IsPaused && !engine.GameState.IsGameOver && !engine.GameState.IsGameWon)
            {
                if (kb.IsKeyDown(Keys.Left) || kb.IsKeyDown(Keys.A))
                {
                    engine.SetPlatformPosition(engine.Platform.X - 8);
                }

                if (kb.IsKeyDown(Keys.Right) || kb.IsKeyDown(Keys.D))
                {
                    engine.SetPlatformPosition(engine.Platform.X + 8);
                }

                if (kb.IsKeyDown(Keys.Space) && !engine.GameState.IsBallLaunched)
                {
                    engine.LaunchBall();
                }
            }

            // Пауза
            if (kb.IsKeyDown(Keys.P) && previousKeyboardState.IsKeyUp(Keys.P))
            {
                engine.TogglePause();
            }

            // Рестарт
            if (kb.IsKeyDown(Keys.R) && previousKeyboardState.IsKeyUp(Keys.R))
            {
                engine.RestartGame();
            }

            // Обновление логики
            if (!engine.GameState.IsPaused && !engine.GameState.IsGameOver && !engine.GameState.IsGameWon)
            {
                engine.Update();
            }

            previousKeyboardState = kb;
            base.Update(gameTime);
        }

        /// <summary>
        /// Отрисовывает кадр игры
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(20, 20, 30));
            spriteBatch.Begin();

            // Кирпичи
            foreach (var b in engine.Bricks)
            {
                if (!b.IsActive)
                {
                    continue;
                }

                var color = brickColors[b.Row % brickColors.Length];

                if (b.IsHit)
                {
                    var flashIntensity = 1.0f - (b.HitFrames / 10.0f);
                    color = Color.Lerp(color, Color.White, flashIntensity * 0.5f);
                }

                // Затемнение цвета в зависимости от здоровья
                var healthRatio = (float)b.Health / b.MaxHealth;
                var brickColor = new Color(
                    (byte)(color.R * healthRatio),
                    (byte)(color.G * healthRatio),
                    (byte)(color.B * healthRatio)
                );

                spriteBatch.Draw(pixel, new Rectangle(b.X, b.Y, b.Width, b.Height), brickColor);

                // Границы кирпичика
                spriteBatch.Draw(pixel, new Rectangle(b.X, b.Y, b.Width, 2), Color.Black * 0.5f);
                spriteBatch.Draw(pixel, new Rectangle(b.X, b.Y + b.Height - 2, b.Width, 2), Color.Black * 0.5f);
                spriteBatch.Draw(pixel, new Rectangle(b.X, b.Y, 2, b.Height), Color.Black * 0.5f);
                spriteBatch.Draw(pixel, new Rectangle(b.X + b.Width - 2, b.Y, 2, b.Height), Color.Black * 0.5f);

                // Трещины
                var damageLevel = b.MaxHealth - b.Health;
                if (damageLevel > 0)
                {
                    var crackColor = Color.Black * 0.7f;

                    switch (damageLevel)
                    {
                        case 1:
                            spriteBatch.Draw(pixel, new Rectangle(b.X + b.Width / 2 - 5, b.Y + b.Height / 2 - 1, 10, 2), crackColor);
                            break;
                        case 2:
                            spriteBatch.Draw(pixel, new Rectangle(b.X + b.Width / 2 - 5, b.Y + b.Height / 2 - 1, 10, 2), crackColor);
                            spriteBatch.Draw(pixel, new Rectangle(b.X + b.Width / 2 - 1, b.Y + b.Height / 2 - 5, 2, 10), crackColor);
                            break;
                        case 3:
                            spriteBatch.Draw(pixel, new Rectangle(b.X + b.Width / 2 - 5, b.Y + b.Height / 2 - 1, 10, 2), crackColor);
                            spriteBatch.Draw(pixel, new Rectangle(b.X + b.Width / 2 - 1, b.Y + b.Height / 2 - 5, 2, 10), crackColor);
                            spriteBatch.Draw(pixel, new Rectangle(b.X + 5, b.Y + 5, 8, 2), crackColor);
                            break;
                        default:
                            spriteBatch.Draw(pixel, new Rectangle(b.X + b.Width / 2 - 5, b.Y + b.Height / 2 - 1, 10, 2), crackColor);
                            spriteBatch.Draw(pixel, new Rectangle(b.X + b.Width / 2 - 1, b.Y + b.Height / 2 - 5, 2, 10), crackColor);
                            spriteBatch.Draw(pixel, new Rectangle(b.X + 5, b.Y + 5, 8, 2), crackColor);
                            spriteBatch.Draw(pixel, new Rectangle(b.X + 5, b.Y + b.Height - 7, 8, 2), crackColor);
                            spriteBatch.Draw(pixel, new Rectangle(b.X + b.Width - 7, b.Y + 5, 2, 8), crackColor);
                            break;
                    }
                }
            }

            // Платформа
            var p = engine.Platform;
            spriteBatch.Draw(pixel, new Rectangle(p.X, p.Y, p.Width, p.Height), Color.Silver);

            // Мячи
            foreach (var ball in engine.Balls)
            {
                if (!ball.IsActive)
                {
                    continue;
                }

                var ballColor = ball.Damage > 1 ? Color.OrangeRed : Color.White;
                spriteBatch.Draw(pixel, new Rectangle(ball.X, ball.Y, ball.Size, ball.Size), ballColor);
            }

            // Power-ups
            foreach (var pu in engine.PowerUps)
            {
                if (!pu.IsActive)
                {
                    continue;
                }

                var puColor = powerUpColors.TryGetValue(pu.Type, out var c) ? c : Color.Purple;
                spriteBatch.Draw(pixel, new Rectangle(pu.X, pu.Y, pu.Size, pu.Size), puColor);

                if (font != null)
                {
                    var letter = pu.Type switch
                    {
                        PowerUpType.ExtraBall => "●",
                        PowerUpType.DamageBoost => "▲",
                        PowerUpType.WidePaddle => "◼",
                        _ => "?"
                    };

                    var measure = font.MeasureString(letter);
                    var textPos = new Vector2(
                        pu.X + (pu.Size - measure.X) / 2,
                        pu.Y + (pu.Size - measure.Y) / 2 - 2
                    );
                    font.Draw(spriteBatch, letter, textPos, Color.White);
                }
            }

            // UI
            if (font != null)
            {
                var gs = engine.GameState;

                // Счёт и жизни
                font.Draw(spriteBatch, $"Счёт: {gs.Score}", new Vector2(10, 10), Color.White);
                font.Draw(spriteBatch, $"Жизни: {gs.Lives}", new Vector2(10, 28), Color.White);

                // Сообщения
                if (!gs.IsBallLaunched && !gs.IsGameOver && !gs.IsGameWon)
                {
                    var msg = "Нажмите ПРОБЕЛ для начала";
                    var measure = font.MeasureString(msg);
                    var pos = new Vector2(Width / 2f - measure.X / 2, Height / 2f + 50);
                    font.Draw(spriteBatch, msg, pos, Color.Yellow);
                }

                if (gs.IsPaused)
                {
                    var msg = "ПАУЗА - Нажмите P";
                    var measure = font.MeasureString(msg);
                    var pos = new Vector2(Width / 2f - measure.X / 2, Height / 2f);
                    font.Draw(spriteBatch, msg, pos, Color.White);
                }

                if (gs.IsGameOver)
                {
                    var msg1 = "ИГРА ОКОНЧЕНА";
                    var msg2 = "Нажмите R для перезапуска";
                    var measure1 = font.MeasureString(msg1);
                    var measure2 = font.MeasureString(msg2);
                    var pos1 = new Vector2(Width / 2f - measure1.X / 2, Height / 2f - 20);
                    var pos2 = new Vector2(Width / 2f - measure2.X / 2, Height / 2f + 10);
                    font.Draw(spriteBatch, msg1, pos1, Color.Red);
                    font.Draw(spriteBatch, msg2, pos2, Color.White);
                }

                if (gs.IsGameWon)
                {
                    var msg1 = "ВЫ ПОБЕДИЛИ!";
                    var msg2 = "Нажмите R для перезапуска";
                    var measure1 = font.MeasureString(msg1);
                    var measure2 = font.MeasureString(msg2);
                    var pos1 = new Vector2(Width / 2f - measure1.X / 2, Height / 2f - 20);
                    var pos2 = new Vector2(Width / 2f - measure2.X / 2, Height / 2f + 10);
                    font.Draw(spriteBatch, msg1, pos1, Color.Gold);
                    font.Draw(spriteBatch, msg2, pos2, Color.White);
                }

                // Легенда
                var legendY = Height - 60;
                font.Draw(spriteBatch, "Бонусы:", new Vector2(10, legendY), Color.Gray);
                font.Draw(spriteBatch, "● Доп. мяч", new Vector2(10, legendY + 18), powerUpColors[PowerUpType.ExtraBall]);
                font.Draw(spriteBatch, "▲ Урон", new Vector2(180, legendY + 18), powerUpColors[PowerUpType.DamageBoost]);
                font.Draw(spriteBatch, "◼ Платформа", new Vector2(340, legendY + 18), powerUpColors[PowerUpType.WidePaddle]);
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
