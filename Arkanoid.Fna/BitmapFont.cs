using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Color = Microsoft.Xna.Framework.Color;

namespace Arkanoid.FNA
{
    /// <summary>
    /// Растровый шрифт для отрисовки текста без Content Pipeline
    /// </summary>
    public class BitmapFont
    {
        private readonly Texture2D texture;
        private readonly Dictionary<char, Microsoft.Xna.Framework.Rectangle> charRectangles = new();
        private readonly int maxCharWidth;
        private readonly int maxCharHeight;

        /// <summary>
        /// Создаёт новый растровый шрифт из системного шрифта
        /// </summary>
        public BitmapFont(GraphicsDevice graphicsDevice, string fontName, int fontSize)
        {
            const int bitmapSize = 512;
            const int charSpacing = 2;

            using var bitmap = new Bitmap(bitmapSize, bitmapSize);
            using var graphics = Graphics.FromImage(bitmap);

            var font = new Font(fontName, fontSize, FontStyle.Regular);
            graphics.Clear(System.Drawing.Color.Transparent);
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            var xPosition = 0;
            var yPosition = 0;
            var currentLineHeight = 0;
            var maxWidth = 0;
            var maxHeight = 0;

            var characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz" +
                            "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ" +
                            "абвгдеёжзийклмнопрстуфхцчшщъыьэюя" +
                            "0123456789 .,!?-;:'\"()[]{}@#$%^&*+=<>|~`_/\\=●▲◼■★♦♣♠♥";

            foreach (var character in characters)
            {
                var characterSize = graphics.MeasureString(character.ToString(), font);
                var characterWidth = (int)Math.Ceiling(characterSize.Width);
                var characterHeight = (int)Math.Ceiling(characterSize.Height);

                if (xPosition + characterWidth > bitmapSize)
                {
                    xPosition = 0;
                    yPosition += currentLineHeight;
                    currentLineHeight = 0;
                }

                if (characterHeight > currentLineHeight)
                {
                    currentLineHeight = characterHeight;
                }

                if (characterWidth > maxWidth)
                {
                    maxWidth = characterWidth;
                }

                if (characterHeight > maxHeight)
                {
                    maxHeight = characterHeight;
                }

                graphics.DrawString(character.ToString(), font, Brushes.White, xPosition, yPosition);
                charRectangles[character] = new Microsoft.Xna.Framework.Rectangle(xPosition, yPosition, characterWidth, characterHeight);
                xPosition += characterWidth + charSpacing;
            }

            maxCharWidth = maxWidth;
            maxCharHeight = maxHeight;

            using var memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, ImageFormat.Png);
            memoryStream.Position = 0;
            texture = Texture2D.FromStream(graphicsDevice, memoryStream);
        }

        /// <summary>
        /// Отрисовывает текст в указанной позиции
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, string text, Vector2 position, Color color)
        {
            const int charSpacing = 2;
            const int additionalSpacing = 1;

            var xPosition = position.X;
            var yPosition = position.Y;

            foreach (var character in text)
            {
                if (character == '\n')
                {
                    xPosition = position.X;
                    yPosition += maxCharHeight + charSpacing;
                    continue;
                }

                if (charRectangles.TryGetValue(character, out var characterRectangle))
                {
                    spriteBatch.Draw(
                        texture,
                        new Microsoft.Xna.Framework.Rectangle(
                            (int)xPosition,
                            (int)yPosition,
                            characterRectangle.Width,
                            characterRectangle.Height),
                        characterRectangle,
                        color
                    );
                    xPosition += characterRectangle.Width + additionalSpacing;
                }
            }
        }

        /// <summary>
        /// Измеряет размеры строки текста
        /// </summary>
        public Vector2 MeasureString(string text)
        {
            const int charSpacing = 2;
            const int lineSpacing = 2;

            var maxWidth = 0f;
            var totalHeight = 0f;
            var currentLineWidth = 0f;

            foreach (var character in text)
            {
                if (character == '\n')
                {
                    if (currentLineWidth > maxWidth)
                    {
                        maxWidth = currentLineWidth;
                    }

                    currentLineWidth = 0;
                    totalHeight += maxCharHeight + lineSpacing;
                    continue;
                }

                if (charRectangles.TryGetValue(character, out var characterRectangle))
                {
                    currentLineWidth += characterRectangle.Width + charSpacing;
                }
            }

            if (currentLineWidth > maxWidth)
            {
                maxWidth = currentLineWidth;
            }

            return new Vector2(maxWidth, totalHeight + maxCharHeight);
        }
    }
}
