using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.UI
{
    //class holding a draw function to prevent both having to pass around graphics device through units/players
    // and storing information that just needs to be displayed. Instead the empty texture is passed in here and
    // used to draw with.
    public static class PercentageBar
    {
        public static void Draw(SpriteBatch spriteBatch, Texture2D emptyRect, Vector2 position, Vector2 dimensions, Color main, Color diff, float max, float value)
        {
            Rectangle diffDestination = new Rectangle(position.ToPoint(), dimensions.ToPoint());
            Rectangle mainDestination = new Rectangle(position.ToPoint(), new Vector2((value/max) * dimensions.X, dimensions.Y).ToPoint());
            spriteBatch.Draw(emptyRect, diffDestination, color: diff);
            spriteBatch.Draw(emptyRect, mainDestination, color: main);
            //draw two bars, one at the desired width of the UI element.
            // the other with width the percentage of the maximum.
        }

    }
}
