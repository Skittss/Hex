using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.UI
{
    //UI element - a rectangular box with anchorpoints.
    public class RectBox
    {
        protected Color color = new Color(0, 0, 0, 200); //by default
        protected Rectangle rect;

        public Rectangle DestinationRect
        {
            get { return new Rectangle(rect.X - (int)anchorPoint.X, rect.Y - (int)anchorPoint.Y, rect.Width, rect.Height); }
        }
        //where the rectangle is in 2d pixel space (with consideration of anchorpoint)

        protected Texture2D texture;
        protected Vector2 anchorPoint = Vector2.Zero;

        public RectBox(GraphicsDevice graphicsDevice, Vector2 pos, int width, int height)
        {
            texture = UiTools.GeneratePixelTexture(graphicsDevice);
            rect = new Rectangle((int)pos.X, (int)pos.Y, width, height);
        }

        //extra constructor for if colour is specified.
        public RectBox(GraphicsDevice graphicsDevice, Vector2 pos, int width, int height, Color color)
        {
            texture = UiTools.GeneratePixelTexture(graphicsDevice);
            rect = new Rectangle((int)pos.X, (int)pos.Y, width, height);
            this.color = color;
        }

        /// <summary>
        /// Takes a fractional offset from the top left of the box and will draw the
        /// box from the offset given.
        /// </summary>
        public void SetAnchorPoint(float offsetX, float offsetY)
        {
            //define an anchorpoint relative to the top left of the box
            //expects between 0 and 1
            anchorPoint = new Vector2(offsetX * rect.Width, offsetY * rect.Height);
        }

        public void SetPosition(Vector2 pos)
        {
            rect.Location = pos.ToPoint();
        }

        public Vector2 GetPos()
        {
            return new Vector2(DestinationRect.X, DestinationRect.Y);
        }

        public Vector2 GetCentrePos()
        {
            return new Vector2((2*GetPos().X + rect.Width)/2, (2*GetPos().Y + rect.Height)/2);
        }

        public int GetBottom()
        {
            return DestinationRect.Bottom;
        }

        public int GetHeight()
        {
            return DestinationRect.Height;
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, DestinationRect, color);
        }
    }
}
