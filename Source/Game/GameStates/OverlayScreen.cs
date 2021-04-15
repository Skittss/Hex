using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Engine.UI;
using Engine.GameStates;


namespace Hex.GameStates
{
    public abstract class OverlayScreen : GameState
    {
        protected GameState underlyingScreen;
        protected Texture2D overlayRectangle;
        protected Color overlayColor = new Color(0, 0, 0, 210);

        public OverlayScreen(GraphicsDevice graphicsDevice, GameState underlyingScreen) : base(graphicsDevice)
        {
            this.underlyingScreen = underlyingScreen;
        }

        public override void Initialize()
        {
            overlayRectangle = UiTools.GeneratePixelTexture(graphicsDevice);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            underlyingScreen.Draw(spriteBatch);
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            spriteBatch.Draw(overlayRectangle, destinationRectangle: new Rectangle(Vector2.Zero.ToPoint(), WindowTools.WindowDimensions.ToPoint()), color: overlayColor);
            spriteBatch.End();
        }
    }
}
