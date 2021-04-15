using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Engine.UI
{
    public class Line
    {
        //line properties
        private Texture2D texture;
        private Color colour;

        private Vector2 start;
        private Vector2 end;

        private int width;
        public int Width
        {
            set { width = value; CalculateDrawInfo(); }
        }

        //calculated draw values
        private Rectangle destinationRect;
        private float angle;

        public Line(GraphicsDevice graphicsDevice, Vector2 start, Vector2 end, int width, Color colour)
        {
            texture = UiTools.GeneratePixelTexture(graphicsDevice);
            this.start = start;
            this.end = end;
            this.width = width;
            this.colour = colour;

            CalculateDrawInfo();
        }

        private void CalculateDrawInfo()
        {
            //calculate rectangle and angle it needs to be drawn at to create an angled line
            Vector2 leftMost = (end.X > start.X) ? start : end;
            Vector2 rightMost = (leftMost == start) ? end : start;
            Vector2 segment = rightMost - leftMost; //start - end;
       
            //use inverse tan to find the desired angle (opposite, adjacent)
            angle = (float)Math.Atan2(segment.Y, segment.X); //(float)Math.Atan2(segment.Y, segment.X);
            destinationRect = new Rectangle((int)leftMost.X, (int)leftMost.Y, (int)segment.Length(), width);
        }

        public void SetPoints(Vector2 start, Vector2 end)
        {
            this.start = start;
            this.end = end;
            CalculateDrawInfo();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            //draw rectangle at an angle to create an angled line
            //draw from origin (0, 0.5) to centre line about pixel specified
            //(i.e. the width is now distributed on either side)
            spriteBatch.Draw(texture, destinationRect, null, colour, angle, new Vector2(0f, 0.5f), SpriteEffects.None, 0f);
        }
    }
}
