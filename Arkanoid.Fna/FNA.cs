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
        private SpriteFont font;
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
                font = Content.Load<SpriteFont>("GameFont");
                Console.WriteLine("Font loaded successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Font load error: {ex.Message}");
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
                spriteBatch.Draw(pixel, new Rectangle(b.X, b.Y, b.Width, b.Height), color);

                // Платформа
                var p = engine.Platform;
                spriteBatch.Draw(pixel, new Rectangle(p.X, p.Y, p.Width, p.Height), Color.Silver);

                spriteBatch.Draw(pixel, new Rectangle(b.X, b.Y, b.Width, 2), Color.Black * 0.5f); // Верх
                spriteBatch.Draw(pixel, new Rectangle(b.X, b.Y + b.Height - 2, b.Width, 2), Color.Black * 0.5f); // Низ
                spriteBatch.Draw(pixel, new Rectangle(b.X, b.Y, 2, b.Height), Color.Black * 0.5f); // Лево
                spriteBatch.Draw(pixel, new Rectangle(b.X + b.Width - 2, b.Y, 2, b.Height), Color.Black * 0.5f); // Право

                // Отображение здоровья для многоразовых кирпичей
                if (b.MaxHealth > 1 && font != null)
                {
                    var healthText = b.Health.ToString();
                    var measure = font.MeasureString(healthText);
                    Vector2 textPos = new Vector2(
                        b.X + b.Width / 2 - measure.X / 2,
                        b.Y + b.Height / 2 - measure.Y / 2
                    );
                    spriteBatch.DrawString(font, healthText, textPos, Color.White);
                }
            }
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

                spriteBatch.Draw(pixel, new Rectangle(pu.X, pu.Y, pu.Size, pu.Size), Color.Purple);

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
                    spriteBatch.DrawString(font, letter, textPos, Color.White);
                }
            }

            if (font != null)
            {
                var gs = engine.GameState;

                // Счёт и жизни
                spriteBatch.DrawString(font,
                    $"Score: {gs.Score}  |  Lives: {gs.Lives}",
                    new Vector2(10, 10), Color.White);

                // Сообщения
                if (!engine.GameState.IsBallLaunched && !engine.GameState.IsGameOver && !engine.GameState.IsGameWon)
                {
                    spriteBatch.DrawString(font,
                        "Press SPACE to start",
                        new Vector2(280, 250), Color.Yellow);
                }

                if (engine.GameState.IsPaused)
                {
                    spriteBatch.DrawString(font,
                        "PAUSED - Press P",
                        new Vector2(300, 250), Color.White);
                }

                if (engine.GameState.IsGameOver)
                {
                    spriteBatch.DrawString(font,
                        "GAME OVER - Press R",
                        new Vector2(280, 250), Color.Red);
                }

                if (engine.GameState.IsGameWon)
                {
                    spriteBatch.DrawString(font,
                        "YOU WIN! - Press R",
                        new Vector2(290, 250), Color.Gold);
                }

                // Легенда
                spriteBatch.DrawString(font, "Power-ups:", new Vector2(10, Height - 70), Color.Gray);
                spriteBatch.DrawString(font, "● Extra Ball", new Vector2(10, Height - 50), powerUpIndicatorColors[PowerUpType.ExtraBall]);
                spriteBatch.DrawString(font, "▲ Damage", new Vector2(120, Height - 50), powerUpIndicatorColors[PowerUpType.DamageBoost]);
                spriteBatch.DrawString(font, "◼ Wide", new Vector2(230, Height - 50), powerUpIndicatorColors[PowerUpType.WidePaddle]);
            }
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
