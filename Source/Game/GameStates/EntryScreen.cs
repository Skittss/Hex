using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Engine.Input;
using Engine.UI;
using Engine.GameStates;


namespace Hex.GameStates
{
    public class EntryScreen : GameState
    {

        private SpriteFont font;
        private string text = "Press Any Key to Start";

        public EntryScreen(GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {

        }

        public override void Initialize()
        {

        }

        public override void LoadContent()
        {
            font = contentManager.Load<SpriteFont>("Fonts/BebasNeue");
        }

        public override void Update(GameTime gameTime)
        {
            if (InputManager.Instance.GetCurrentPressed().Length != 0)
            {
                GameStateManager.Instance.ChangeState(new CharacterSelect(graphicsDevice));
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            graphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();

            spriteBatch.DrawString(font,
                "PROJECT HEX",
                WindowTools.PaddingToPixelCoordinate(0.5f, 0.5f, 0, 0),
                Color.White, 0f,
                font.MeasureString("PROJECT HEX") / 2,
                1f, SpriteEffects.None, 0f);

            spriteBatch.DrawString(font,
                text,
                WindowTools.PaddingToPixelCoordinate(0.5f, 0.85f, 0, 0),
                Color.White, 0f,
                font.MeasureString(text) / 2,
                0.5f, SpriteEffects.None, 0.5f);

            spriteBatch.End();
        }
    }
}
