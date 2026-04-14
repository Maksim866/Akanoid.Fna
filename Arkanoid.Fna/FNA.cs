using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Arkanoid.Logic.Engine;
using Arkanoid.Logic.Interfaces;
using Arkanoid.Logic.Models;
using Arkanoid.Logic.Enums;
using Arkanoid.Logic.Manager;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Arkanoid.FNA
{
    public class FNA : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // Game engine
        private IArkanoidEngine engine;

        // Textures
        private Texture2D pixelTexture;
        private Texture2D ballTexture;
        private Texture2D platformTexture;
        private Texture2D brickTexture;
        private Texture2D powerUpTexture;

        // Fonts
        private SpriteFont gameFont;
        private SpriteFont largeFont;

        // Colors for bricks by row
        private readonly Color[] brickColors = new Color[]
        {
            new Color(231, 76, 60),    // Red (row 0)
            new Color(230, 126, 34),   // Orange (row 1)
            new Color(241, 196, 15),   // Yellow (row 2)
            new Color(46, 204, 113),   // Green (row 3)
            new Color(52, 152, 219)    // Blue (row 4)
        };

        // Power-up colors
        private readonly Dictionary<PowerUpType, Color> powerUpColors = new Dictionary<PowerUpType, Color>
        {
            { PowerUpType.ExtraBall, new Color(142, 68, 173) },      // Purple
            { PowerUpType.DamageBoost, new Color(192, 57, 43) },     // Dark Red
            { PowerUpType.WidePaddle, new Color(22, 160, 133) }      // Teal
        };

        // Screen dimensions
        private const int GameWidth = 800;
        private const int GameHeight = 600;

        // Input
        private KeyboardState previousKeyboardState;

        // UI state
        private string playerName = "";
        private bool showingScoreInput = false;
        private bool showingLeaderboard = false;
        private int cursorPosition = 0;
        private readonly List<char> allowedChars = new List<char>
        {
            'A','B','C','D','E','F','G','H','I','J','K','L','M',
            'N','O','P','Q','R','S','T','U','V','W','X','Y','Z',
            '0','1','2','3','4','5','6','7','8','9',' '
        };

        public FNA()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            IsMouseVisible = true;
            Window.Title = "Arkanoid - FNA Edition";

            graphics.PreferredBackBufferWidth = GameWidth;
            graphics.PreferredBackBufferHeight = GameHeight;
        }

        protected override void Initialize()
        {
            // Initialize game engine
            engine = new ArkanoidEngine(GameWidth, GameHeight);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Create pixel texture for drawing rectangles
            pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
            pixelTexture.SetData(new Color[] { Color.White });

            // Create ball texture (circle)
            ballTexture = CreateCircleTexture(15, Color.White);

            // Create platform texture (gradient)
            platformTexture = CreateGradientTexture(100, 20,
                new Color(149, 165, 166), new Color(127, 140, 141));

            // Create brick texture
            brickTexture = CreateGradientTexture(60, 20,
                new Color(255, 255, 255), new Color(200, 200, 200));

            // Create power-up texture
            powerUpTexture = CreateCircleTexture(20, Color.White);

            // Load fonts
            try
            {
                gameFont = Content.Load<SpriteFont>("GameFont");
                largeFont = Content.Load<SpriteFont>("LargeFont");
            }
            catch
            {
                // If fonts not found, we'll use default rendering
                gameFont = null;
                largeFont = null;
            }
        }

        private Texture2D CreateCircleTexture(int diameter, Color color)
        {
            Texture2D texture = new Texture2D(GraphicsDevice, diameter, diameter);
            Color[] data = new Color[diameter * diameter];

            for (int i = 0; i < diameter; i++)
            {
                for (int j = 0; j < diameter; j++)
                {
                    float dx = i - diameter / 2f;
                    float dy = j - diameter / 2f;
                    float distance = (float)Math.Sqrt(dx * dx + dy * dy);

                    if (distance <= diameter / 2f)
                    {
                        data[i * diameter + j] = color;
                    }
                    else
                    {
                        data[i * diameter + j] = Color.Transparent;
                    }
                }
            }

            texture.SetData(data);
            return texture;
        }

        private Texture2D CreateGradientTexture(int width, int height, Color topColor, Color bottomColor)
        {
            Texture2D texture = new Texture2D(GraphicsDevice, width, height);
            Color[] data = new Color[width * height];

            for (int i = 0; i < height; i++)
            {
                float t = i / (float)(height - 1);
                Color color = Color.Lerp(topColor, bottomColor, t);

                for (int j = 0; j < width; j++)
                {
                    data[i * width + j] = color;
                }
            }

            texture.SetData(data);
            return texture;
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            KeyboardState keyboardState = Keyboard.GetState();

            // Handle input based on game state
            if (showingScoreInput)
            {
                HandleScoreInput(keyboardState);
            }
            else if (showingLeaderboard)
            {
                if (keyboardState.IsKeyDown(Keys.Enter) ||
                    keyboardState.IsKeyDown(Keys.Space) ||
                    keyboardState.IsKeyDown(Keys.Escape))
                {
                    showingLeaderboard = false;
                    engine.RestartGame();
                }
            }
            else
            {
                HandleGameInput(keyboardState);
            }

            // Update game engine if not paused and not showing screens
            if (!showingScoreInput && !showingLeaderboard &&
                !engine.GameState.IsPaused && !engine.GameState.IsGameOver && !engine.GameState.IsGameWon)
            {
                engine.Update();
            }

            // Check for game end
            if ((engine.GameState.IsGameOver || engine.GameState.IsGameWon) && !showingScoreInput && !showingLeaderboard)
            {
                if (engine.GameState.Score > 0)
                {
                    showingScoreInput = true;
                    playerName = "";
                    cursorPosition = 0;
                }
                else
                {
                    showingLeaderboard = true;
                }
            }

            previousKeyboardState = keyboardState;
            base.Update(gameTime);
        }

        private void HandleGameInput(KeyboardState keyboardState)
        {
            // Platform movement
            float moveSpeed = 8f;

            if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A))
            {
                engine.SetPlatformPosition(engine.Platform.X - (int)moveSpeed);
            }
            if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))
            {
                engine.SetPlatformPosition(engine.Platform.X + (int)moveSpeed);
            }

            // Launch ball
            if (keyboardState.IsKeyDown(Keys.Space) && !engine.GameState.IsBallLaunched)
            {
                engine.LaunchBall();
            }

            // Pause
            if (keyboardState.IsKeyDown(Keys.P) && !previousKeyboardState.IsKeyDown(Keys.P))
            {
                engine.TogglePause();
            }

            // Restart
            if (keyboardState.IsKeyDown(Keys.R) && !previousKeyboardState.IsKeyDown(Keys.R))
            {
                engine.RestartGame();
            }
        }

        private void HandleScoreInput(KeyboardState keyboardState)
        {
            // Cursor movement
            if (keyboardState.IsKeyDown(Keys.Left) && !previousKeyboardState.IsKeyDown(Keys.Left))
            {
                if (cursorPosition > 0)
                {
                    cursorPosition--;
                }
            }
            if (keyboardState.IsKeyDown(Keys.Right) && !previousKeyboardState.IsKeyDown(Keys.Right))
            {
                if (cursorPosition < playerName.Length)
                {
                    cursorPosition++;
                }
            }

            // Character input
            if (keyboardState.IsKeyDown(Keys.Back) && !previousKeyboardState.IsKeyDown(Keys.Back))
            {
                if (playerName.Length > 0 && cursorPosition > 0)
                {
                    playerName = playerName.Remove(cursorPosition - 1, 1);
                    cursorPosition--;
                }
            }

            if (keyboardState.IsKeyDown(Keys.Enter) && !previousKeyboardState.IsKeyDown(Keys.Enter))
            {
                if (playerName.Length >= 3)
                {
                    string endType = engine.GameState.IsGameWon ? "Victory" : "Game Over";
                    ScoreManager.AddScore(playerName, engine.GameState.Score, engine.GameState.Lives, endType);
                    showingScoreInput = false;
                    showingLeaderboard = true;
                }
            }

            // Letter input
            foreach (Keys key in keyboardState.GetPressedKeys())
            {
                if (!previousKeyboardState.IsKeyDown(key))
                {
                    char? ch = KeyToChar(key);
                    if (ch.HasValue && playerName.Length < 10)
                    {
                        playerName = playerName.Insert(cursorPosition, ch.Value.ToString());
                        cursorPosition++;
                    }
                }
            }
        }

        private char? KeyToChar(Keys key)
        {
            KeyboardState state = Keyboard.GetState();
            bool shift = state.IsKeyDown(Keys.LeftShift) || state.IsKeyDown(Keys.RightShift);

            if (key >= Keys.A && key <= Keys.Z)
            {
                char c = (char)('A' + (key - Keys.A));
                return shift ? c : c; // Always uppercase for simplicity
            }

            if (key >= Keys.D0 && key <= Keys.D9)
            {
                return (char)('0' + (key - Keys.D0));
            }

            if (key == Keys.Space)
            {
                return ' ';
            }

            return null;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(20, 20, 30));

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            // Draw game elements
            if (engine != null)
            {
                DrawBricks();
                DrawPowerUps();
                DrawPlatform();
                DrawBalls();
                DrawUI();

                // Overlays
                if (engine.GameState.IsPaused)
                {
                    DrawPauseScreen();
                }

                if (engine.GameState.IsGameOver)
                {
                    DrawGameOverScreen();
                }

                if (engine.GameState.IsGameWon)
                {
                    DrawVictoryScreen();
                }
            }

            // Score input screen
            if (showingScoreInput)
            {
                DrawScoreInputScreen();
            }

            // Leaderboard screen
            if (showingLeaderboard)
            {
                DrawLeaderboardScreen();
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawBricks()
        {
            foreach (var brick in engine.Bricks)
            {
                if (!brick.IsActive)
                {
                    continue;
                }

                // Get color based on row
                Color brickColor = brick.Row < brickColors.Length ? brickColors[brick.Row] : Color.White;

                // Adjust brightness based on health
                float healthRatio = (float)brick.Health / brick.MaxHealth;
                brickColor = new Color(
                    (int)(brickColor.R * healthRatio),
                    (int)(brickColor.G * healthRatio),
                    (int)(brickColor.B * healthRatio)
                );

                // Draw brick with border
                Rectangle brickRect = new Rectangle(brick.X, brick.Y, brick.Width, brick.Height);

                // Main brick
                spriteBatch.Draw(pixelTexture, brickRect, brickColor);

                // Highlight (top)
                spriteBatch.Draw(pixelTexture, new Rectangle(brick.X, brick.Y, brick.Width, 2),
                    Color.White * 0.5f);

                // Shadow (bottom)
                spriteBatch.Draw(pixelTexture, new Rectangle(brick.X, brick.Y + brick.Height - 2, brick.Width, 2),
                    Color.Black * 0.3f);

                // Hit effect
                if (brick.IsHit)
                {
                    Color hitColor = Color.White * (brick.HitFrames / (float)Arkanoid.Logic.Constants.ArkanoidConstants.HitEffectDuration);
                    spriteBatch.Draw(pixelTexture, brickRect, hitColor);
                }

                // Draw health number for multi-hit bricks
                if (brick.MaxHealth > 1 && gameFont != null)
                {
                    Vector2 textPos = new Vector2(
                        brick.X + brick.Width / 2f - gameFont.MeasureString(brick.Health.ToString()).X / 2f,
                        brick.Y + brick.Height / 2f - gameFont.MeasureString(brick.Health.ToString()).Y / 2f
                    );
                    spriteBatch.DrawString(gameFont, brick.Health.ToString(), textPos, Color.White);
                }
            }
        }

        private void DrawPlatform()
        {
            var platform = engine.Platform;

            // Draw platform with gradient
            Rectangle platformRect = new Rectangle(platform.X, platform.Y, platform.Width, platform.Height);
            spriteBatch.Draw(platformTexture, platformRect, Color.White);

            // Draw platform border
            spriteBatch.Draw(pixelTexture, new Rectangle(platform.X - 2, platform.Y - 2, platform.Width + 4, 2), Color.Gray);
            spriteBatch.Draw(pixelTexture, new Rectangle(platform.X - 2, platform.Y + platform.Height, platform.Width + 4, 2), Color.Gray);
            spriteBatch.Draw(pixelTexture, new Rectangle(platform.X - 2, platform.Y, 2, platform.Height), Color.Gray);
            spriteBatch.Draw(pixelTexture, new Rectangle(platform.X + platform.Width, platform.Y, 2, platform.Height), Color.Gray);
        }

        private void DrawBalls()
        {
            foreach (var ball in engine.Balls)
            {
                if (!ball.IsActive)
                {
                    continue;
                }

                Rectangle ballRect = new Rectangle(ball.X, ball.Y, ball.Size, ball.Size);

                // Draw ball with glow effect
                Color ballColor = ball.Damage > 1 ? new Color(231, 76, 60) : Color.White;

                // Glow
                for (int i = 3; i > 0; i--)
                {
                    Rectangle glowRect = new Rectangle(
                        ball.X - i, ball.Y - i,
                        ball.Size + i * 2, ball.Size + i * 2);
                    spriteBatch.Draw(ballTexture, glowRect, ballColor * (0.3f / i));
                }

                // Main ball
                spriteBatch.Draw(ballTexture, ballRect, ballColor);

                // Highlight
                spriteBatch.Draw(pixelTexture,
                    new Rectangle(ball.X + 3, ball.Y + 3, ball.Size / 3, ball.Size / 3),
                    Color.White * 0.8f);
            }
        }

        private void DrawPowerUps()
        {
            foreach (var powerUp in engine.PowerUps)
            {
                if (!powerUp.IsActive)
                {
                    continue;
                }

                Rectangle powerUpRect = new Rectangle(powerUp.X, powerUp.Y, powerUp.Size, powerUp.Size);

                // Get color based on type
                Color powerUpColor = powerUpColors.ContainsKey(powerUp.Type) ?
                    powerUpColors[powerUp.Type] : Color.Purple;

                // Draw power-up with pulsing effect
                float pulse = (float)Math.Sin(DateTime.Now.Millisecond / 100.0) * 0.2f + 0.8f;

                spriteBatch.Draw(powerUpTexture, powerUpRect, powerUpColor * pulse);

                // Draw border
                spriteBatch.Draw(pixelTexture, new Rectangle(powerUp.X, powerUp.Y, powerUp.Size, 2), Color.White);
                spriteBatch.Draw(pixelTexture, new Rectangle(powerUp.X, powerUp.Y + powerUp.Size - 2, powerUp.Size, 2), Color.White);
                spriteBatch.Draw(pixelTexture, new Rectangle(powerUp.X, powerUp.Y, 2, powerUp.Size), Color.White);
                spriteBatch.Draw(pixelTexture, new Rectangle(powerUp.X + powerUp.Size - 2, powerUp.Y, 2, powerUp.Size), Color.White);
            }
        }

        private void DrawUI()
        {
            var gameState = engine.GameState;

            // Draw score
            string scoreText = $"SCORE: {gameState.Score}";
            Vector2 scorePos = new Vector2(10, 10);

            if (gameFont != null)
            {
                spriteBatch.DrawString(gameFont, scoreText, scorePos + new Vector2(2, 2), Color.Black);
                spriteBatch.DrawString(gameFont, scoreText, scorePos, Color.White);
            }

            // Draw lives
            string livesText = $"LIVES: {gameState.Lives}";
            Vector2 livesPos = new Vector2(GameWidth - gameFont.MeasureString(livesText).X - 10, 10);

            if (gameFont != null)
            {
                spriteBatch.DrawString(gameFont, livesText, livesPos + new Vector2(2, 2), Color.Black);
                spriteBatch.DrawString(gameFont, livesText, livesPos, Color.White);
            }

            // Draw instructions if ball not launched
            if (!gameState.IsBallLaunched && !gameState.IsGameOver && !gameState.IsGameWon)
            {
                string instructionText = "Press SPACE to Launch";
                Vector2 instructionPos = new Vector2(
                    GameWidth / 2f - gameFont.MeasureString(instructionText).X / 2f,
                    GameHeight / 2f + 100
                );

                spriteBatch.DrawString(gameFont, instructionText, instructionPos + new Vector2(2, 2), Color.Black);
                spriteBatch.DrawString(gameFont, instructionText, instructionPos, Color.Yellow);
            }
        }

        private void DrawPauseScreen()
        {
            // Semi-transparent overlay
            spriteBatch.Draw(pixelTexture, new Rectangle(0, 0, GameWidth, GameHeight),
                Color.Black * 0.7f);

            string pauseText = "PAUSED";
            Vector2 pausePos = new Vector2(
                GameWidth / 2f - largeFont.MeasureString(pauseText).X / 2f,
                GameHeight / 2f - 50
            );

            string resumeText = "Press P to Resume";
            Vector2 resumePos = new Vector2(
                GameWidth / 2f - gameFont.MeasureString(resumeText).X / 2f,
                GameHeight / 2f + 20
            );

            spriteBatch.DrawString(largeFont, pauseText, pausePos + new Vector2(4, 4), Color.Black);
            spriteBatch.DrawString(largeFont, pauseText, pausePos, Color.White);

            spriteBatch.DrawString(gameFont, resumeText, resumePos + new Vector2(2, 2), Color.Black);
            spriteBatch.DrawString(gameFont, resumeText, resumePos, Color.Yellow);
        }

        private void DrawGameOverScreen()
        {
            spriteBatch.Draw(pixelTexture, new Rectangle(0, 0, GameWidth, GameHeight),
                Color.Black * 0.7f);

            string gameOverText = "GAME OVER";
            Vector2 gameOverPos = new Vector2(
                GameWidth / 2f - largeFont.MeasureString(gameOverText).X / 2f,
                GameHeight / 2f - 50
            );

            string scoreText = $"Final Score: {engine.GameState.Score}";
            Vector2 scorePos = new Vector2(
                GameWidth / 2f - gameFont.MeasureString(scoreText).X / 2f,
                GameHeight / 2f + 20
            );

            spriteBatch.DrawString(largeFont, gameOverText, gameOverPos + new Vector2(4, 4), Color.Black);
            spriteBatch.DrawString(largeFont, gameOverText, gameOverPos, Color.Red);

            spriteBatch.DrawString(gameFont, scoreText, scorePos + new Vector2(2, 2), Color.Black);
            spriteBatch.DrawString(gameFont, scoreText, scorePos, Color.White);
        }

        private void DrawVictoryScreen()
        {
            spriteBatch.Draw(pixelTexture, new Rectangle(0, 0, GameWidth, GameHeight),
                Color.Black * 0.7f);

            string victoryText = "VICTORY!";
            Vector2 victoryPos = new Vector2(
                GameWidth / 2f - largeFont.MeasureString(victoryText).X / 2f,
                GameHeight / 2f - 50
            );

            string scoreText = $"Final Score: {engine.GameState.Score}";
            Vector2 scorePos = new Vector2(
                GameWidth / 2f - gameFont.MeasureString(scoreText).X / 2f,
                GameHeight / 2f + 20
            );

            spriteBatch.DrawString(largeFont, victoryText, victoryPos + new Vector2(4, 4), Color.Black);
            spriteBatch.DrawString(largeFont, victoryText, victoryPos, Color.Gold);

            spriteBatch.DrawString(gameFont, scoreText, scorePos + new Vector2(2, 2), Color.Black);
            spriteBatch.DrawString(gameFont, scoreText, scorePos, Color.White);
        }

        private void DrawScoreInputScreen()
        {
            spriteBatch.Draw(pixelTexture, new Rectangle(0, 0, GameWidth, GameHeight),
                Color.Black * 0.8f);

            string titleText = engine.GameState.IsGameWon ? "VICTORY!" : "GAME OVER";
            Color titleColor = engine.GameState.IsGameWon ? Color.Gold : Color.Red;

            Vector2 titlePos = new Vector2(
                GameWidth / 2f - largeFont.MeasureString(titleText).X / 2f,
                GameHeight / 2f - 100
            );

            string scoreText = $"Score: {engine.GameState.Score}";
            Vector2 scorePos = new Vector2(
                GameWidth / 2f - gameFont.MeasureString(scoreText).X / 2f,
                GameHeight / 2f - 40
            );

            string inputText = "Enter Your Name:";
            Vector2 inputPos = new Vector2(
                GameWidth / 2f - gameFont.MeasureString(inputText).X / 2f,
                GameHeight / 2f + 10
            );

            // Draw name box
            Rectangle nameBox = new Rectangle(
                GameWidth / 2 - 150, GameHeight / 2 + 40, 300, 40);
            spriteBatch.Draw(pixelTexture, nameBox, Color.DarkGray);
            spriteBatch.Draw(pixelTexture, nameBox, null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);

            // Draw name
            Vector2 namePos = new Vector2(
                GameWidth / 2f - gameFont.MeasureString(playerName + "_").X / 2f,
                GameHeight / 2f + 45
            );
            string displayName = playerName + (DateTime.Now.Millisecond < 500 ? "_" : "");
            spriteBatch.DrawString(gameFont, displayName, namePos, Color.White);

            string hint1 = "Use Arrow Keys to Move Cursor";
            string hint2 = "Press Enter to Submit (min 3 chars)";

            Vector2 hint1Pos = new Vector2(
                GameWidth / 2f - gameFont.MeasureString(hint1).X / 2f,
                GameHeight / 2f + 100
            );
            Vector2 hint2Pos = new Vector2(
                GameWidth / 2f - gameFont.MeasureString(hint2).X / 2f,
                GameHeight / 2f + 125
            );

            spriteBatch.DrawString(largeFont, titleText, titlePos + new Vector2(4, 4), Color.Black);
            spriteBatch.DrawString(largeFont, titleText, titlePos, titleColor);

            spriteBatch.DrawString(gameFont, scoreText, scorePos + new Vector2(2, 2), Color.Black);
            spriteBatch.DrawString(gameFont, scoreText, scorePos, Color.White);

            spriteBatch.DrawString(gameFont, inputText, inputPos + new Vector2(2, 2), Color.Black);
            spriteBatch.DrawString(gameFont, inputText, inputPos, Color.Yellow);

            spriteBatch.DrawString(gameFont, hint1, hint1Pos, Color.Gray);
            spriteBatch.DrawString(gameFont, hint2, hint2Pos, Color.Gray);
        }

        private void DrawLeaderboardScreen()
        {
            spriteBatch.Draw(pixelTexture, new Rectangle(0, 0, GameWidth, GameHeight),
                new Color(20, 20, 30));

            string titleText = "LEADERBOARD";
            Vector2 titlePos = new Vector2(
                GameWidth / 2f - largeFont.MeasureString(titleText).X / 2f,
                30
            );

            spriteBatch.DrawString(largeFont, titleText, titlePos + new Vector2(4, 4), Color.Black);
            spriteBatch.DrawString(largeFont, titleText, titlePos, Color.Gold);

            var scores = ScoreManager.GetScores();

            if (scores.Count == 0)
            {
                string noScoresText = "No scores yet!";
                Vector2 noScoresPos = new Vector2(
                    GameWidth / 2f - gameFont.MeasureString(noScoresText).X / 2f,
                    GameHeight / 2f
                );
                spriteBatch.DrawString(gameFont, noScoresText, noScoresPos, Color.Gray);
            }
            else
            {
                // Draw headers
                string header = "Rank  Name           Score    Lives  Type";
                Vector2 headerPos = new Vector2(50, 100);
                spriteBatch.DrawString(gameFont, header, headerPos, Color.Yellow);

                // Draw line
                spriteBatch.Draw(pixelTexture, new Rectangle(50, 125, GameWidth - 100, 2), Color.Gray);

                // Draw scores
                for (int i = 0; i < scores.Count; i++)
                {
                    var score = scores[i];
                    string rank = (i + 1).ToString("D2");
                    string name = score.PlayerName.PadRight(15);
                    string scoreStr = score.Score.ToString().PadLeft(8);
                    string lives = score.Lives.ToString().PadLeft(6);
                    string type = score.GameEndType;

                    string line = $"{rank}  {name} {scoreStr}  {lives}  {type}";
                    Vector2 linePos = new Vector2(50, 135 + i * 30);

                    Color lineColor = i == 0 ? Color.Gold :
                                     i == 1 ? Color.Silver :
                                     i == 2 ? Color.Brown : Color.White;

                    spriteBatch.DrawString(gameFont, line, linePos, lineColor);
                }
            }

            string restartText = "Press ENTER or SPACE to Restart";
            Vector2 restartPos = new Vector2(
                GameWidth / 2f - gameFont.MeasureString(restartText).X / 2f,
                GameHeight - 50
            );
            spriteBatch.DrawString(gameFont, restartText, restartPos, Color.Gray);
        }
    }
}
