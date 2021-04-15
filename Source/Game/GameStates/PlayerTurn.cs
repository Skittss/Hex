using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Hex.Input;


namespace Hex.GameStates
{
    public class LoadingScreen : GameState
    {
        public LoadingScreen(GraphicsDevice graphicsDevice, GameWindow window) : base(graphicsDevice, window)  //end bit here calls the base constructor.
        {
            //add extra constructor business here
            //though now we have graphicsDevice is most likely unnecessary
        }

        public override void Initialize()
        {
            
        }

        public override void LoadContent(ContentManager Content)
        {

        }

        public override void UnloadContent()
        {

        }

        public override void Update(GameTime gameTime)
        {

        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.End();
        }
    }
}
