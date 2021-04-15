using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Hex.GameStates;
using Hex.Pools;
using Engine.GameStates;
using Engine.Input;
using Engine.UI;

namespace Hex
{

    public class Hex : Game
    {

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public Hex()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferHeight = (int)WindowTools.WindowDimensions.Y;
            graphics.PreferredBackBufferWidth = (int)WindowTools.WindowDimensions.X;
            //set hardware switch to false to allow for borderless fullscreen
            graphics.HardwareModeSwitch = false;
            graphics.IsFullScreen = true;
            

            Content.RootDirectory = "Content";
        }

        /// Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        protected override void Initialize()
        {
            IsMouseVisible = true;
            ProbabilityPools.AssignPools();
            base.Initialize();
        }

        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            GameStateManager.Instance.SetContent(Content);  //this instatiates a GameStateManager (as it is a singleton)
            GameStateManager.Instance.AddState(new EntryScreen(GraphicsDevice));
            // TODO: use this.Content to load your game content here
        }

        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        protected override void UnloadContent()
        {
            //Unload any content not handled through the Content Manager from Monogame.
            GameStateManager.Instance.UnloadAllStateContent();
            Content.Unload();
        }

        protected override void Update(GameTime gameTime)
        {
            //update the input.
            InputManager.Instance.Update();

            //update the running game state.
            GameStateManager.Instance.Update(gameTime);

            if (InputManager.Instance.IsKeyPressed(Keybinds.Actions.Exit))
            {
                Exit();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            //Draw from the currently running game state.
            GameStateManager.Instance.Draw(spriteBatch);
            base.Draw(gameTime);
        }
    }
}
