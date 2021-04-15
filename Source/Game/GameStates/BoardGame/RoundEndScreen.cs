using System.Collections.Generic;
using System.Linq;
using Engine.Input;
using Engine.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Engine.GameStates;


namespace Hex.GameStates
{
    public class RoundEndScreen : OverlayScreen
    {
        private static int lineSpacing = 50;
        private string[] lines;
        private Vector2[] drawPositions;
        private string continueText;
        private Vector2 continuePos;
        private SpriteFont font;

        public RoundEndScreen(GraphicsDevice graphicsDevice, GameState underlyingScreen, string[] lines) : base(graphicsDevice, underlyingScreen)
        {
            this.lines = lines;
        }

        public override void LoadContent()
        {
            font = contentManager.Load<SpriteFont>("Fonts/BebasNeue_Small");

            //calculate drawing metrics one time only.
            //calculate the size of each line (x,y)
            Vector2[] lineSizes = lines.Select(line => font.MeasureString(line)).ToArray();
            //get their heights.
            float[] lineHeights = lineSizes.Select(size => size.Y).ToArray();
            //calculate a height offset - offset so that the strings are drawn centred about the midpoint of the screen
            // in the y axis.
            float heightOffset = ((lines.Length - 1) * lineSpacing + lineHeights.Sum(x => x/2))/2;
            Vector2 screenMidPoint = WindowTools.WindowDimensions / 2;
            var pos = new List<Vector2>();
            for (int i = 0; i < lines.Length; i++)
            {
                //calculate the draw position using the midpoint, height offset and line spacings.
                pos.Add(screenMidPoint - new Vector2(lineSizes[i].X / 2, heightOffset - i * (lineSpacing - 1)));
            }
            drawPositions = pos.ToArray();

            continueText = $"Press {Keybinds.GetKey(Keybinds.Actions.Continue)} to continue...";
            continuePos = WindowTools.PaddingToPixelCoordinate(0.5f, 0.95f, 0, 0) - font.MeasureString(continueText) / 2;
        }

        public override void Update(GameTime gameTime)
        {
            if (InputManager.Instance.IsKeyToggled(Keybinds.Actions.Continue))
            {
                GameStateManager.Instance.PopState();
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            spriteBatch.Begin();
            for (int i = 0; i < lines.Length; i++)
            {
                spriteBatch.DrawString(font, lines[i], drawPositions[i], Color.White);
            }
            spriteBatch.DrawString(font, continueText, continuePos, Color.Green);
            spriteBatch.End();
        }
    }
}
