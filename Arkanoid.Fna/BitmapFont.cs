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
        private readonly Dictionary<char, Microsoft.Xna.Framework.Rectangle> charRects = new();
        private readonly int charWidth;
        private readonly int charHeight;

        /// <summary>
        /// Создаёт новый растровый шрифт из системного шрифта
        /// </summary>
        /// <param name="graphicsDevice">Графическое устройство</param>
        /// <param name="fontName">Имя системного шрифта (например "Arial")</param>
        /// <param name="size">Размер шрифта в пунктах</param>
        public BitmapFont(GraphicsDevice graphicsDevice, string fontName, int size)
        {
            using var bmp = new Bitmap(512, 512);
            using var g = Graphics.FromImage(bmp);

            var font = new Font(fontName, size, FontStyle.Regular);
            g.Clear(System.Drawing.Color.Transparent);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            var x = 0;
            var y = 0;
            var lineHeight = 0;
            var maxWidth = 0;
            var maxHeight = 0;

            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz" +
            "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ" +
            "абвгдеёжзийклмнопрстуфхцчшщъыьэюя" +
            "0123456789 .,!?-;:'\"()[]{}@#$%^&*+=<>|~`_/\\=●▲◼■★♦♣♠♥";

            foreach (var c in chars)
            {
                var sizeF = g.MeasureString(c.ToString(), font);
                var w = (int)Math.Ceiling(sizeF.Width);
                var h = (int)Math.Ceiling(sizeF.Height);

                if (x + w > 512)
                {
                    x = 0;
                    y += lineHeight;
                    lineHeight = 0;
                }

                if (h > lineHeight)
                {
                    lineHeight = h;
                }

                if (w > maxWidth)
                {
                    maxWidth = w;
                }

                if (h > maxHeight)
                {
                    maxHeight = h;
                }

                g.DrawString(c.ToString(), font, Brushes.White, x, y);
                charRects[c] = new Microsoft.Xna.Framework.Rectangle(x, y, w, h);
                x += w + 2;
            }

            charWidth = maxWidth;
            charHeight = maxHeight;

            using var stream = new MemoryStream();
            bmp.Save(stream, ImageFormat.Png);
            stream.Position = 0;
            texture = Texture2D.FromStream(graphicsDevice, stream);
        }

        /// <summary>
        /// Отрисовывает текст в указанной позиции
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, string text, Vector2 position, Color color)
        {
            var x = position.X;
            var y = position.Y;

            foreach (var c in text)
            {
                if (c == '\n')
                {
                    x = position.X;
                    y += charHeight + 2;
                    continue;
                }

                if (charRects.TryGetValue(c, out var rect))
                {
                    spriteBatch.Draw(
                        texture,
                        new Microsoft.Xna.Framework.Rectangle(
                            (int)x,
                            (int)y,
                            rect.Width,
                            rect.Height),
                        rect,
                        color
                    );
                    x += rect.Width + 1;
                }
            }
        }

        /// <summary>
        /// Измеряет размеры строки текста
        /// </summary>
        public Vector2 MeasureString(string text)
        {
            var maxWidth = 0f;
            var height = 0f;
            var currentWidth = 0f;

            foreach (var c in text)
            {
                if (c == '\n')
                {
                    if (currentWidth > maxWidth)
                    {
                        maxWidth = currentWidth;
                    }

                    currentWidth = 0;
                    height += charHeight + 2;
                    continue;
                }

                if (charRects.TryGetValue(c, out var rect))
                {
                    currentWidth += rect.Width + 1;
                }
            }

            if (currentWidth > maxWidth)
            {
                maxWidth = currentWidth;
            }

            return new Vector2(maxWidth, height + charHeight);
        }
    }
}
