using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Engine.UI
{
    //Rectangular button, makes use of the Rectbox UI element.
    // pass in a type to the class, it specifies the arguments that
    // accompany the button press.
    public abstract class RectBoxButton<T> : RectBox
    {
        public event EventHandler<T> buttonPressed;

        protected bool isHovered;
        public bool IsHovered { get { return isHovered; } }
        protected Color hoveredColor = new Color(100, 100, 100, 200);

        public RectBoxButton(GraphicsDevice graphicsDevice, Vector2 pos, int width, int height) : base(graphicsDevice, pos, width, height)
        {

        }

        public abstract void Update(GameTime gameTime);

        protected void leftAction(T args)
        {
            //respond on left click.
            buttonPressed(this, args);
        }

        protected bool ContainsPoint(Vector2 point)
        {
            return DestinationRect.Contains(point);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (isHovered)
            {
                spriteBatch.Draw(texture, DestinationRect, hoveredColor);
            }
            else
            {
                spriteBatch.Draw(texture, DestinationRect, color);
            }

        }

    }
}
