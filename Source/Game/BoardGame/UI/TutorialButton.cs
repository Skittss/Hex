using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Engine.Input;
using Engine.UI;
using Hex.BoardGame;
using Engine.GameStates;
using Hex.GameStates;

namespace Hex.UI
{
    //Button UI element which increases a players unit cap.
    // largely similar to other single press buttons.
    // information drawn to button is different, however.

    public class TutorialButton : RectBoxButton<SinglePressEventArgs>
    {
        private string text = "help";
        private GameState underlyingScreen;
        private GraphicsDevice graphicsDevice;

        public TutorialButton(GraphicsDevice graphicsDevice, GameState screen, Vector2 pos, int width, int height) : base(graphicsDevice, pos, width, height)
        {
            underlyingScreen = screen;
            this.graphicsDevice = graphicsDevice;
            buttonPressed += AddTutorialScreen;
        }

        public override void Update(GameTime gameTime)
        {
            isHovered = false;
            if (ContainsPoint(InputManager.Instance.MousePos))
            {
                isHovered = true;
                if (InputManager.Instance.IsLeftMouseToggled())
                {
                    //send button press event
                    leftAction(new SinglePressEventArgs(false, true));
                }
            }
        }

        private void AddTutorialScreen(object sender, SinglePressEventArgs args)
        {
            GameStateManager.Instance.AddState(new TutorialScreen(graphicsDevice, underlyingScreen));
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font, float fontScale)
        {
            base.Draw(spriteBatch);

            //draw overlaying text
            Vector2 textSize = fontScale * font.MeasureString(text);
            Vector2 basePos = GetPos() + new Vector2(rect.Width / 2, GetHeight() / 2);
            spriteBatch.DrawString(font, text, basePos - textSize/2, Color.White, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);
        }
    }
}
