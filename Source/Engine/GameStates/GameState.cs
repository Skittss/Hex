using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Engine.GameStates
{
    public abstract class GameState : GameStateInterface
    {

        protected GraphicsDevice graphicsDevice;
        protected ContentManager contentManager;
        //each self-contained gamestate needs a GraphicsDevice to draw with.

        public GameState(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
            contentManager = new ContentManager(GameStateManager.Instance.Content.ServiceProvider);
            contentManager.RootDirectory = "Content";
        }

        public abstract void Initialize();
        public abstract void LoadContent();
        public virtual void UnloadContent()
        {
            contentManager.Unload();
        }
        public abstract void Update(GameTime gameTime);
        public abstract void Draw(SpriteBatch spriteBatch);
    }
}
