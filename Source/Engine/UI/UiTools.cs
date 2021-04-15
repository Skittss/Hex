using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using System.Collections.Generic;

namespace Engine.UI
{
    public static class UiTools
    {
        public static Vector2 BoundsToVector(Rectangle rectangle)
        {
            return new Vector2(rectangle.Width, rectangle.Height);
        }


        /// <summary>
        /// Generates a single pixel texture coloured white.
        /// </summary>
        public static Texture2D GeneratePixelTexture(GraphicsDevice graphicsDevice)
        {
            Texture2D texture = new Texture2D(graphicsDevice, 1, 1);
            texture.SetData(new Color[] { Color.White });
            return texture;
        }

        /// <summary>
        /// Generates a mask of the given texture that contains the colour of each
        /// pixel in the texture at the respective [x, y] coordinate in the returned array.
        /// </summary>
        public static Color[,] CreateColorMask(Texture2D target)
        {
            Color[] colorData = new Color[target.Width * target.Height];
            target.GetData(colorData);
            Color[,] colorMask = new Color[target.Width, target.Height];
            for (int x = 0; x < target.Width; x++)
            {
                for (int y = 0; y < target.Height; y++)
                {
                    colorMask[x, y] = colorData[x + y * target.Width];
                }
            }

            return colorMask;

        }

        /// <summary>
        /// Generates a mask of the given texture that contains a bool for each pixel
        /// in the texture - true if the pixel has non-zero alpha (visible).
        /// </summary>
        public static bool[,] CreateBoolMask(Texture2D target)
        {
            Color[,] colorMask = CreateColorMask(target);
            bool[,] boolMask = new bool[target.Width, target.Height];

            for (int x = 0; x < target.Width; x++)
            {
                for (int y = 0; y < target.Height; y++)
                {
                    if (colorMask[x, y].A == 0)
                    {
                        boolMask[x, y] = false;
                    }
                    else
                    {
                        boolMask[x, y] = true;
                    }
                }
            }

            return boolMask;
        }

        /// <summary>
        /// Text wrapper. used in character select. given a long string of words, split them
        /// onto new lines if the line width with the word exceeds that allowed.
        /// </summary>
        public static string[] WrapText(SpriteFont font, string text, float width, float padding, float scale)
        {
            //calculate the maximum width of a line of text, considering padding on Left and Right.
            float maximumLineWidth = width - 2 * padding;
            //measure the width of a space character as well, this will make up part of the width of a line.
            float spaceWidth = scale * font.MeasureString(" ").X;
            //lines after wrapping are initially stored as a 2d list, each sublist containing words.
            List<List<string>> lines = new List<List<string>>();
            lines.Add(new List<string>());
            //split the text by each word.
            string[] words = text.Split(' ');

            int currentLine = 0;
            foreach (string word in words)
            {
                //using the width of the current word
                float wordWidth = scale * font.MeasureString(word).X;
                // ... and preceeding characters on the line.
                float currentLineWidth = lines[currentLine].Sum(x => font.MeasureString(x).X) + (lines[currentLine].Count - 1) * spaceWidth;
                if (currentLineWidth + wordWidth < maximumLineWidth)
                {
                    //add the word if the max width is not exceeded.
                    lines[currentLine].Add(word);
                }
                else
                {
                    //otherwise add it to the next line, increment the counter.
                    lines.Add(new List<string>());
                    currentLine += 1;
                    lines[currentLine].Add(word);
                }
            }

            //convert the 2d list to an array of strings for each line.
            string[] wrappedText = new string[lines.Count];
            for (int i = 0; i < lines.Count; i++)
            {
                List<string> line = lines[i];
                //join each word in each line with spaces.
                string result = string.Join(" ", line);
                wrappedText[i] = result;
            }
            return wrappedText;
        }
    }
}
