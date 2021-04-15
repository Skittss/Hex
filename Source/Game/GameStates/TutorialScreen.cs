using System.Collections.Generic;
using System.Linq;
using Engine.Input;
using Engine.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Engine.GameStates;
using System.IO;

namespace Hex.GameStates
{
    public class TutorialScreen : OverlayScreen
    {
        private Texture2D[] subScreens;
        private int currentScreen = 0;
        private SpriteFont font;

        public TutorialScreen(GraphicsDevice graphicsDevice, GameState underlyingScreen) : base(graphicsDevice, underlyingScreen)
        {

        }

        public override void LoadContent()
        {
            font = contentManager.Load<SpriteFont>("Fonts/BebasNeue_Small");
            //Get the tutorials

            //try to load content from the directory, if any fails, exit out.
            try
            {
                //load from directory: Content/Tutorials/{underlyingStateName}
                string fileName = underlyingScreen.GetType().Name;
                //format the file path to correspond to content directory
                string[] contentNames = 
                    Directory.GetFiles($"{contentManager.RootDirectory}/Tutorials/{fileName}")
                    .Select(name => name.Replace('\\', '/')
                    .Substring(contentManager.RootDirectory.Length + 1, name.Length - contentManager.RootDirectory.Length - 5))
                    .OrderBy(name => name.Last())
                    .ToArray();

                List<Texture2D> images = new List<Texture2D>();
                foreach (string name in contentNames)
                {
                    images.Add(contentManager.Load<Texture2D>(name));
                }
                subScreens = images.ToArray();
            }
            catch (DirectoryNotFoundException e)
            {
                GameStateManager.Instance.PopState();
            }

        }

        public override void Update(GameTime gameTime)
        {
            if (InputManager.Instance.IsLeftMouseToggled())
            {
                currentScreen += 1;
                if (currentScreen > subScreens.Length - 1)
                {
                    GameStateManager.Instance.PopState();
                }
            } 
            else if (InputManager.Instance.IsRightMouseToggled() && currentScreen > 0)
            {
                currentScreen -= 1;
            }

            if (InputManager.Instance.IsKeyToggled(Keybinds.Actions.Continue))
            {
                GameStateManager.Instance.PopState();
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            spriteBatch.Begin();
            spriteBatch.Draw(subScreens[currentScreen], position: Vector2.Zero);

            string navigateText = $"(Right Click) <-- {currentScreen+1}/{subScreens.Length} --> (Left  Click)";
            Vector2 size = font.MeasureString(navigateText);
            Vector2 pos = WindowTools.PaddingToPixelCoordinate(0.5f, 0.95f, 0, 0) - size / 2;
            spriteBatch.DrawString(font, navigateText, pos, Color.White);
            spriteBatch.End();
        }
    }
}
