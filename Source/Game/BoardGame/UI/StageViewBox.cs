using Engine.UI;
using Hex.BoardGame;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hex.UI
{
    /// <summary>
    /// large UI element to display the stage structure to the players.
    /// </summary>
    public class StageViewBox
    {
        private static int textBoxHeight = (int)WindowTools.GetUpscaledValue(60f);
        private static int stageIconBoxHeight = (int)WindowTools.GetUpscaledValue(50f);
        private static int width = (int)WindowTools.GetUpscaledValue(200f);
        private static float spriteScale = WindowTools.GetUpscaledValue(3f);
        private static Vector2 spriteScaleVector = new Vector2(spriteScale);

        private Stage stage;
        private int stageIndex;

        private SpriteFont textFont;
        private Vector2 pos;

        private RectBox textBox;
        private Line divisorA;
        private RectBox[] stageIconBoxes;

        private string text;

        public StageViewBox(GraphicsDevice graphicsDevice, Stage stage, SpriteFont textFont,  int stageNumber, int stageIndex, Vector2 pos)
        {
            this.stage = stage;
            this.pos = pos;
            this.textFont = textFont;
            this.stageIndex = stageIndex;
            text = $"Stage {stageNumber} : {stageIndex}";

            textBox = new RectBox(graphicsDevice,
                this.pos,
                width, textBoxHeight);

            divisorA = new Line(graphicsDevice,
                new Vector2(pos.X, textBox.GetBottom()),
                new Vector2(pos.X + width, textBox.GetBottom()),
                5, Color.White);

            stageIconBoxes = new RectBox[stage.Length];

            for (int i = 0; i < stage.Length; i++)
            {
                stageIconBoxes[i] = new RectBox(graphicsDevice,
                    new Vector2(
                        pos.X,
                        (i == 0)? textBox.GetBottom() : stageIconBoxes[i-1].GetBottom()),
                    width, stageIconBoxHeight);
            }
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D vsAiIcon, Texture2D vsPlayerIcon)
        {

            //draw underlying alpha box
            textBox.Draw(spriteBatch);
            //calculate the size of the text
            Vector2 textSize = textFont.MeasureString(text);
            //calculate the position where the text needs to be drawn to be central
            //  to the text box.
            Vector2 textPos = textBox.GetCentrePos() - 0.5f * new Vector2(textSize.X, textSize.Y);
            //draw the text
            spriteBatch.DrawString(textFont, text, textPos, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);

            //draw the underlying alpha boxes for each round.
            foreach (RectBox box in stageIconBoxes)
            {
                box.Draw(spriteBatch);
            }


            //draw dividing line
            divisorA.Draw(spriteBatch);

            //draw respective information about each round.
            for (int i = 0; i < stage.Length; i++)
            {
                if (stage.Rounds[i] == Stage.RoundTypes.vsAi)
                {
                    spriteBatch.Draw(vsAiIcon,
                        position: stageIconBoxes[i].GetCentrePos() - 0.5f * spriteScale * UiTools.BoundsToVector(vsAiIcon.Bounds),
                        scale: spriteScaleVector,
                        color: (i < stageIndex - 1)? Color.Gray : Color.White
                        );
                }
                else
                {
                    spriteBatch.Draw(vsPlayerIcon,
                        position: stageIconBoxes[i].GetCentrePos() - 0.5f * spriteScale * UiTools.BoundsToVector(vsPlayerIcon.Bounds),
                        scale: spriteScaleVector,
                        color: (i < stageIndex - 1) ? Color.DarkRed : Color.Red
                        );
                }
            }
        }
    }
}
