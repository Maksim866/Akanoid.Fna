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
    /// Основной класс игры Arkanoid на движке FNA
    /// </summary>
    public class FNA : Game
    {
        private readonly GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private IArkanoidEngine engine;
        private Texture2D pixelTexture;
        private BitmapFont font;
        private KeyboardState previousKeyboardState;

        // === Цвета кирпичей по рядам ===
        private readonly static Color[] brickColors = {
            new(231, 76, 60), new(230, 126, 34),
            new(241, 196, 15), new(46, 204, 113),
            new(52, 152, 219)
        };

        // === Цвета бонусов ===
        private readonly static Dictionary<PowerUpType, Color> powerUpColors = new()
        {
            { PowerUpType.ExtraBall, new(142, 68, 173) },
            { PowerUpType.DamageBoost, new(192, 57, 43) },
            { PowerUpType.WidePaddle, new(22, 160, 133) }
        };

        /// <summary>
        /// Инициализирует новый экземпляр игры
        /// </summary>
        public FNA()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = GameConstants.ScreenWidth;
            graphics.PreferredBackBufferHeight = GameConstants.ScreenHeight;
            IsMouseVisible = true;
        }

        /// <summary>
        /// Инициализирует игровые компоненты
        /// </summary>
        protected override void Initialize()
        {
            engine = new ArkanoidEngine(GameConstants.ScreenWidth, GameConstants.ScreenHeight);
            base.Initialize();
        }

        /// <summary>
        /// Загружает контент игры
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
            pixelTexture.SetData(new[] { Color.White });

            try
            {
                font = new BitmapFont(GraphicsDevice, "Arial", GameConstants.FontSize);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки шрифта: {ex.Message}");
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

            var currentKeyboardState = Keyboard.GetState();
            ProcessGameInput(currentKeyboardState);
            ProcessControlInput(currentKeyboardState);
            UpdateGameState();

            previousKeyboardState = currentKeyboardState;
            base.Update(gameTime);
        }

        /// <summary>
        /// Обрабатывает игровое управление (движение, запуск)
        /// </summary>
        private void ProcessGameInput(KeyboardState keyboardState)
        {
            if (engine.GameState.IsPaused
                || engine.GameState.IsGameOver
                || engine.GameState.IsGameWon)
            {
                return;
            }

            if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A))
            {
                engine.SetPlatformPosition(engine.Platform.X - GameConstants.PlatformMoveSpeed);
            }

            if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))
            {
                engine.SetPlatformPosition(engine.Platform.X + GameConstants.PlatformMoveSpeed);
            }

            if (keyboardState.IsKeyDown(Keys.Space) && !engine.GameState.IsBallLaunched)
            {
                engine.LaunchBall();
            }
        }

        /// <summary>
        /// Обрабатывает управляющие команды (пауза, рестарт)
        /// </summary>
        private void ProcessControlInput(KeyboardState keyboardState)
        {
            if (keyboardState.IsKeyDown(Keys.P) && previousKeyboardState.IsKeyUp(Keys.P))
            {
                engine.TogglePause();
            }

            if (keyboardState.IsKeyDown(Keys.R) && previousKeyboardState.IsKeyUp(Keys.R))
            {
                engine.RestartGame();
            }
        }

        /// <summary>
        /// Обновляет логику игры если не на паузе
        /// </summary>
        private void UpdateGameState()
        {
            if (!engine.GameState.IsPaused
                && !engine.GameState.IsGameOver
                && !engine.GameState.IsGameWon)
            {
                engine.Update();
            }
        }

        /// <summary>
        /// Отрисовывает кадр игры
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            var backgroundColor = new Color(20, 20, 30);
            GraphicsDevice.Clear(backgroundColor);
            spriteBatch.Begin();

            DrawGameElements();
            DrawUi();

            spriteBatch.End();
            base.Draw(gameTime);
        }

        /// <summary>
        /// Отрисовывает игровые объекты
        /// </summary>
        private void DrawGameElements()
        {
            DrawBricks();
            DrawPlatform();
            DrawBalls();
            DrawPowerUps();
        }

        /// <summary>
        /// Отрисовывает кирпичи с эффектами
        /// </summary>
        private void DrawBricks()
        {
            foreach (var brick in engine.Bricks)
            {
                if (!brick.IsActive)
                {
                    continue;
                }

                var baseColor = brickColors[brick.Row % brickColors.Length];
                var brickColor = ApplyHitEffect(baseColor, brick);
                brickColor = ApplyHealthDimming(brickColor, brick);

                var brickRect = new Rectangle(brick.X, brick.Y, brick.Width, brick.Height);
                spriteBatch.Draw(pixelTexture, brickRect, brickColor);
                DrawBrickBorders(brickRect);
                DrawCracks(brick);
            }
        }

        /// <summary>
        /// Рисует границы кирпича
        /// </summary>
        private void DrawBrickBorders(Rectangle brickRect)
        {
            var borderColor = Color.Black * GameConstants.BorderAlpha;
            var thickness = GameConstants.BrickBorderThickness;

            spriteBatch.Draw(pixelTexture, new Rectangle(brickRect.X, brickRect.Y, brickRect.Width, thickness), borderColor);
            spriteBatch.Draw(pixelTexture, new Rectangle(brickRect.X, brickRect.Y + brickRect.Height - thickness, brickRect.Width, thickness), borderColor);
            spriteBatch.Draw(pixelTexture, new Rectangle(brickRect.X, brickRect.Y, thickness, brickRect.Height), borderColor);
            spriteBatch.Draw(pixelTexture, new Rectangle(brickRect.X + brickRect.Width - thickness, brickRect.Y, thickness, brickRect.Height), borderColor);
        }

        /// <summary>
        /// Рисует трещины на кирпиче
        /// </summary>
        private void DrawCracks(BrickModel brick)
        {
            var damageLevel = brick.MaxHealth - brick.Health;
            if (damageLevel <= 0)
            {
                return;
            }

            var crackColor = Color.Black * GameConstants.CrackAlpha;
            var centerX = brick.X + brick.Width / 2;
            var centerY = brick.Y + brick.Height / 2;

            spriteBatch.Draw(pixelTexture, new Rectangle(centerX - 5, centerY - 1, 10, 2), crackColor);

            if (damageLevel > 1)
            {
                spriteBatch.Draw(pixelTexture, new Rectangle(centerX - 1, centerY - 5, 2, 10), crackColor);
            }

            if (damageLevel > 2)
            {
                spriteBatch.Draw(pixelTexture, new Rectangle(brick.X + 5, brick.Y + 5, 8, 2), crackColor);
                spriteBatch.Draw(pixelTexture, new Rectangle(brick.X + 5, brick.Y + brick.Height - 7, 8, 2), crackColor);
                spriteBatch.Draw(pixelTexture, new Rectangle(brick.X + brick.Width - 7, brick.Y + 5, 2, 8), crackColor);
            }
        }

        /// <summary>
        /// Применяет эффект мигания при ударе
        /// </summary>
        private static Color ApplyHitEffect(Color baseColor, BrickModel brick)
        {
            if (!brick.IsHit)
            {
                return baseColor;
            }

            var flashIntensity = 1.0f - (brick.HitFrames / GameConstants.HitFramesDivisor);
            return Color.Lerp(baseColor, Color.White, flashIntensity * GameConstants.FlashIntensityMultiplier);
        }

        /// <summary>
        /// Затемняет цвет в зависимости от здоровья
        /// </summary>
        private static Color ApplyHealthDimming(Color color, BrickModel brick)
        {
            var ratio = (float)brick.Health / brick.MaxHealth;
            return new Color(
                (byte)(color.R * ratio),
                (byte)(color.G * ratio),
                (byte)(color.B * ratio)
            );
        }

        /// <summary>
        /// Отрисовывает платформу
        /// </summary>
        private void DrawPlatform()
        {
            var platform = engine.Platform;
            spriteBatch.Draw(pixelTexture, new Rectangle(platform.X, platform.Y, platform.Width, platform.Height), Color.Silver);
        }

        /// <summary>
        /// Отрисовывает мячи
        /// </summary>
        private void DrawBalls()
        {
            foreach (var ball in engine.Balls)
            {
                if (!ball.IsActive)
                {
                    continue;
                }

                var ballColor = ball.Damage > 1 ? Color.OrangeRed : Color.White;
                spriteBatch.Draw(pixelTexture, new Rectangle(ball.X, ball.Y, ball.Size, ball.Size), ballColor);
            }
        }

        /// <summary>
        /// Отрисовывает падающие бонусы с символами
        /// </summary>
        private void DrawPowerUps()
        {
            foreach (var pu in engine.PowerUps)
            {
                if (!pu.IsActive)
                {
                    continue;
                }

                var puColor = powerUpColors.TryGetValue(pu.Type, out var c) ? c : Color.Purple;
                spriteBatch.Draw(pixelTexture, new Rectangle(pu.X, pu.Y, pu.Size, pu.Size), puColor);

                if (font != null)
                {
                    var symbol = GetPowerUpSymbol(pu.Type);
                    DrawCenteredSymbol(symbol, new Rectangle(pu.X, pu.Y, pu.Size, pu.Size), Color.White);
                }
            }
        }

        /// <summary>
        /// Возвращает символ для типа бонуса
        /// </summary>
        private static string GetPowerUpSymbol(PowerUpType type) => type switch
        {
            PowerUpType.ExtraBall => "●",
            PowerUpType.DamageBoost => "▲",
            PowerUpType.WidePaddle => "◼",
            _ => "?"
        };

        /// <summary>
        /// Рисует символ по центру прямоугольника
        /// </summary>
        private void DrawCenteredSymbol(string symbol, Rectangle area, Color color)
        {
            if (font == null)
            {
                return;
            }

            var measure = font.MeasureString(symbol);
            var pos = new Vector2(
                area.X + (area.Width - measure.X) / 2,
                area.Y + (area.Height - measure.Y) / 2 - 2
            );
            font.Draw(spriteBatch, symbol, pos, color);
        }

        /// <summary>
        /// Отрисовывает пользовательский интерфейс
        /// </summary>
        private void DrawUi()
        {
            if (font == null)
            {
                return;
            }

            var gs = engine.GameState;
            DrawScoreAndLives(gs);
            DrawGameMessages(gs);
            DrawLegend();
        }

        /// <summary>
        /// Отрисовывает счёт и жизни
        /// </summary>
        private void DrawScoreAndLives(GameStateModel gs)
        {
            font.Draw(spriteBatch, $"{UiTexts.Score} {gs.Score}",
                new Vector2(GameConstants.TextMarginLeft, GameConstants.TextMarginTop), Color.White);
            font.Draw(spriteBatch, $"{UiTexts.Lives} {gs.Lives}",
                new Vector2(GameConstants.TextMarginLeft, GameConstants.LivesYPosition), Color.White);
        }

        /// <summary>
        /// Отрисовывает сообщения по состоянию игры
        /// </summary>
        private void DrawGameMessages(GameStateModel gs)
        {
            if (!gs.IsBallLaunched && !gs.IsGameOver && !gs.IsGameWon)
            {
                DrawCenteredMessage(UiTexts.StartHint, Color.Yellow, GameConstants.StartMessageYOffset);
            }

            if (gs.IsPaused)
            {
                DrawCenteredMessage(UiTexts.Paused, Color.White, 0);
            }

            if (gs.IsGameOver)
            {
                DrawTwoLineMessage(UiTexts.GameOver, UiTexts.Restart, Color.Red, Color.White);
            }

            if (gs.IsGameWon)
            {
                DrawTwoLineMessage(UiTexts.Victory, UiTexts.Restart, Color.Gold, Color.White);
            }
        }

        /// <summary>
        /// Отрисовывает центрированное сообщение
        /// </summary>
        private void DrawCenteredMessage(string text, Color color, int yOffset)
        {
            var measure = font.MeasureString(text);
            var pos = new Vector2(
                GameConstants.ScreenWidth / 2f - measure.X / 2,
                GameConstants.ScreenHeight / 2f + yOffset
            );
            font.Draw(spriteBatch, text, pos, color);
        }

        /// <summary>
        /// Отрисовывает сообщение из двух строк
        /// </summary>
        private void DrawTwoLineMessage(string line1, string line2, Color color1, Color color2)
        {
            var m1 = font.MeasureString(line1);
            var m2 = font.MeasureString(line2);

            var pos1 = new Vector2(
                GameConstants.ScreenWidth / 2f - m1.X / 2,
                GameConstants.ScreenHeight / 2f + GameConstants.Message1YOffset
            );
            var pos2 = new Vector2(
                GameConstants.ScreenWidth / 2f - m2.X / 2,
                GameConstants.ScreenHeight / 2f + GameConstants.Message2YOffset
            );

            font.Draw(spriteBatch, line1, pos1, color1);
            font.Draw(spriteBatch, line2, pos2, color2);
        }

        /// <summary>
        /// Отрисовывает легенду бонусов
        /// </summary>
        private void DrawLegend()
        {
            var legendY = GameConstants.ScreenHeight - GameConstants.LegendBottomOffset;

            font.Draw(spriteBatch, UiTexts.LegendTitle,
                new Vector2(GameConstants.TextMarginLeft, legendY), Color.Gray);

            font.Draw(spriteBatch, $"● {UiTexts.BonusExtraBall}",
                new Vector2(GameConstants.LegendItem1X, legendY + GameConstants.TextLineSpacing),
                powerUpColors[PowerUpType.ExtraBall]);

            font.Draw(spriteBatch, $"▲ {UiTexts.BonusDamage}",
                new Vector2(GameConstants.LegendItem2X, legendY + GameConstants.TextLineSpacing),
                powerUpColors[PowerUpType.DamageBoost]);

            font.Draw(spriteBatch, $"◼ {UiTexts.BonusWide}",
                new Vector2(GameConstants.LegendItem3X, legendY + GameConstants.TextLineSpacing),
                powerUpColors[PowerUpType.WidePaddle]);
        }
    }
}
