using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Arkanoid.FNA
{
    public class BitmapFont
    {
        private Texture2D texture;
        private Dictionary<char, Microsoft.Xna.Framework.Rectangle> charRects = new Dictionary<char, Microsoft.Xna.Framework.Rectangle>();
        private int charWidth, charHeight;

        public BitmapFont(GraphicsDevice graphicsDevice, string fontName, int size)
        {
            // Создаём bitmap с символами
            using (var bmp = new Bitmap(256, 256))
            using (var g = System.Drawing.Graphics.FromImage(bmp))
            {
                var font = new System.Drawing.Font(fontName, size, FontStyle.Regular);
                g.Clear(System.Drawing.Color.Transparent);
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                charWidth = 0;
                charHeight = 0;
                int x = 0, y = 0;
                int lineHeight = 0;

                // Генерируем символы
                string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 .,!?-;:'\"()[]{}@#$%^&*+=<>|~`_";

                foreach (char c in chars)
                {
                    var sizef = g.MeasureString(c.ToString(), font);
                    int w = (int)Math.Ceiling(sizef.Width);
                    int h = (int)Math.Ceiling(sizef.Height);

                    if (x + w > 256)
                    {
                        x = 0;
                        y += lineHeight;
                        lineHeight = 0;
                    }

                    if (h > lineHeight)
                    {
                        lineHeight = h;
                    }

                    if (w > charWidth)
                    {
                        charWidth = w;
                    }

                    if (h > charHeight)
                    {
                        charHeight = h;
                    }

                    g.DrawString(c.ToString(), font, Brushes.White, x, y);
                    charRects[c] = new Microsoft.Xna.Framework.Rectangle(x, y, w, h);

                    x += w + 2; // отступ между символами
                }

                // Конвертируем в Texture2D
                using (var stream = new MemoryStream())
                {
                    bmp.Save(stream, ImageFormat.Png);
                    stream.Position = 0;
                    texture = Texture2D.FromStream(graphicsDevice, stream);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, string text, Vector2 position, Microsoft.Xna.Framework.Color color)
        {
            float x = position.X;
            float y = position.Y;

            foreach (char c in text)
            {
                if (c == '\n')
                {
                    x = position.X;
                    y += charHeight + 2;
                    continue;
                }

                if (charRects.ContainsKey(c))
                {
                    var rect = charRects[c];
                    spriteBatch.Draw(texture, new Microsoft.Xna.Framework.Rectangle((int)x, (int)y, rect.Width, rect.Height),
                        rect, color);
                    x += rect.Width + 1;
                }
            }
        }

        public Vector2 MeasureString(string text)
        {
            float maxWidth = 0;
            float height = 0;
            float currentWidth = 0;

            foreach (char c in text)
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

                if (charRects.ContainsKey(c))
                {
                    currentWidth += charRects[c].Width + 1;
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
