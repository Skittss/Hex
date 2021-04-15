using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.GameStates
{
    interface GameStateInterface
    {
        //follows the same structure as the base game class.
        //make sure to pass in correct params.
        //Each gamestate should be able to maintain itself.

        void Initialize();
        void LoadContent();
        void UnloadContent();
        void Update(GameTime gameTime);
        void Draw(SpriteBatch spriteBatch);
    }
}