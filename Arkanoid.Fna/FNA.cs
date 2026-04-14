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
    public class FNA : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        private IArkanoidEngine engine;
        private Texture2D pixel;
        private BitmapFont font;
        private KeyboardState previousKeyboardState;

        // Простые цвета
        private readonly Color[] brickColors = {
            new Color(231, 76, 60), new Color(230, 126, 34),
            new Color(241, 196, 15), new Color(46, 204, 113),
            new Color(52, 152, 219)
        };

        // Цвета для типов усилений
        private readonly Dictionary<PowerUpType, Color> powerUpIndicatorColors = new Dictionary<PowerUpType, Color>
        {
            { PowerUpType.ExtraBall, new Color(142, 68, 173) },      // Фиолетовый
            { PowerUpType.DamageBoost, new Color(192, 57, 43) },     // Красный
            { PowerUpType.WidePaddle, new Color(22, 160, 133) }      // Бирюзовый
        };
        private const int Width = 800, Height = 600;

        public FNA()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = Width;
            graphics.PreferredBackBufferHeight = Height;
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            engine = new ArkanoidEngine(Width, Height);
            base.Initialize();
        }

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
                Console.WriteLine($"Font error: {ex.Message}");
                font = null;
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            // Управление платформой
            var kb = Keyboard.GetState();
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
                    float flashIntensity = 1.0f - (b.HitFrames / 10.0f);
                    color = Color.Lerp(color, Color.White, flashIntensity * 0.5f);
                }

                // ✅ Затемнение цвета в зависимости от здоровья
                float healthRatio = (float)b.Health / b.MaxHealth;
                Color brickColor = new Color(
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

                // ✅ ТРЕЩИНЫ вместо цифр
                int damageLevel = b.MaxHealth - b.Health;
                if (damageLevel > 0)
                {
                    Color crackColor = Color.Black * 0.7f;

                    // Рисуем трещины в зависимости от уровня повреждения
                    switch (damageLevel)
                    {
                        case 1:
                            // Одна маленькая трещина
                            spriteBatch.Draw(pixel, new Rectangle(b.X + b.Width / 2 - 5, b.Y + b.Height / 2 - 1, 10, 2), crackColor);
                            break;
                        case 2:
                            // Две трещины (крест)
                            spriteBatch.Draw(pixel, new Rectangle(b.X + b.Width / 2 - 5, b.Y + b.Height / 2 - 1, 10, 2), crackColor);
                            spriteBatch.Draw(pixel, new Rectangle(b.X + b.Width / 2 - 1, b.Y + b.Height / 2 - 5, 2, 10), crackColor);
                            break;
                        case 3:
                            // Три трещины (диагонали)
                            spriteBatch.Draw(pixel, new Rectangle(b.X + b.Width / 2 - 5, b.Y + b.Height / 2 - 1, 10, 2), crackColor);
                            spriteBatch.Draw(pixel, new Rectangle(b.X + b.Width / 2 - 1, b.Y + b.Height / 2 - 5, 2, 10), crackColor);
                            spriteBatch.Draw(pixel, new Rectangle(b.X + 5, b.Y + 5, 8, 2), crackColor);
                            break;
                        default:
                            // Много трещин (сетка)
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

                spriteBatch.Draw(pixel, new Rectangle(ball.X, ball.Y, ball.Size, ball.Size),
                    ball.Damage > 1 ? Color.OrangeRed : Color.White);
            }

            // Power-ups
            foreach (var pu in engine.PowerUps)
            {
                if (!pu.IsActive)
                {
                    continue;
                }

                var puColor = powerUpIndicatorColors.ContainsKey(pu.Type)
                    ? powerUpIndicatorColors[pu.Type]
                    : Color.Purple;

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
                    Vector2 textPos = new Vector2(
                        pu.X + pu.Size / 2 - measure.X / 2,
                        pu.Y + pu.Size / 2 - measure.Y / 2
                    );
                    font.Draw(spriteBatch, letter, textPos, Color.White);
                }
            }

            // ✅ UI - УЛУЧШЕННОЕ РАСПОЛОЖЕНИЕ
            if (font != null)
            {
                var gs = engine.GameState;

                // Счёт и жизни - верхний левый угол
                font.Draw(spriteBatch, $"Score: {gs.Score}", new Vector2(10, 10), Color.White);
                font.Draw(spriteBatch, $"Lives: {gs.Lives}", new Vector2(10, 28), Color.White);

                // Сообщения по центру экрана
                if (!engine.GameState.IsBallLaunched && !engine.GameState.IsGameOver && !engine.GameState.IsGameWon)
                {
                    string msg = "Press SPACE to start";
                    var measure = font.MeasureString(msg);
                    Vector2 pos = new Vector2(
                        Width / 2f - measure.X / 2,
                        Height / 2f + 50
                    );
                    font.Draw(spriteBatch, msg, pos, Color.Yellow);
                }

                if (engine.GameState.IsPaused)
                {
                    string msg = "PAUSED - Press P";
                    var measure = font.MeasureString(msg);
                    Vector2 pos = new Vector2(
                        Width / 2f - measure.X / 2,
                        Height / 2f
                    );
                    font.Draw(spriteBatch, msg, pos, Color.White);
                }

                if (engine.GameState.IsGameOver)
                {
                    string msg1 = "GAME OVER";
                    string msg2 = "Press R to Restart";
                    var measure1 = font.MeasureString(msg1);
                    var measure2 = font.MeasureString(msg2);
                    Vector2 pos1 = new Vector2(
                        Width / 2f - measure1.X / 2,
                        Height / 2f - 20
                    );
                    Vector2 pos2 = new Vector2(
                        Width / 2f - measure2.X / 2,
                        Height / 2f + 10
                    );
                    font.Draw(spriteBatch, msg1, pos1, Color.Red);
                    font.Draw(spriteBatch, msg2, pos2, Color.White);
                }

                if (engine.GameState.IsGameWon)
                {
                    string msg1 = "YOU WIN!";
                    string msg2 = "Press R to Restart";
                    var measure1 = font.MeasureString(msg1);
                    var measure2 = font.MeasureString(msg2);
                    Vector2 pos1 = new Vector2(
                        Width / 2f - measure1.X / 2,
                        Height / 2f - 20
                    );
                    Vector2 pos2 = new Vector2(
                        Width / 2f - measure2.X / 2,
                        Height / 2f + 10
                    );
                    font.Draw(spriteBatch, msg1, pos1, Color.Gold);
                    font.Draw(spriteBatch, msg2, pos2, Color.White);
                }

                // ✅ Легенда - нижний левый угол, компактно
                int legendY = Height - 60;
                font.Draw(spriteBatch, "Power-ups:", new Vector2(10, legendY), Color.Gray);
                font.Draw(spriteBatch, "● Extra", new Vector2(10, legendY + 18), powerUpIndicatorColors[PowerUpType.ExtraBall]);
                font.Draw(spriteBatch, "▲ Damage", new Vector2(90, legendY + 18), powerUpIndicatorColors[PowerUpType.DamageBoost]);
                font.Draw(spriteBatch, "◼ Wide", new Vector2(180, legendY + 18), powerUpIndicatorColors[PowerUpType.WidePaddle]);
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
