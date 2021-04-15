using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Engine.Input;
using Engine.UI;
using Hex.BoardGame;

namespace Hex.UI
{
    //Button UI element which increases a players unit cap.
    // largely similar to other single press buttons.
    // information drawn to button is different, however.

    public class LevelUnitCapButton : RectBoxButton<SinglePressEventArgs>
    {
        private Color CantLevelColor = new Color(139, 0, 0, 200);

        public LevelUnitCapButton(GraphicsDevice graphicsDevice, Vector2 pos, int width, int height) : base(graphicsDevice, pos, width, height) { }

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
        public void Draw(SpriteBatch spriteBatch, Player turnPlayer, Texture2D moneyIcon, float spriteScale, SpriteFont font, float fontScale)
        {
            if (turnPlayer.Money < turnPlayer.LevelUnitCapCost)
            {
                spriteBatch.Draw(texture, DestinationRect, CantLevelColor);
            }
            else
            {
                base.Draw(spriteBatch);
            }

            //draw overlaying text.
            string text = $"Increase Unit Cap: {turnPlayer.LevelUnitCapCost}";
            Vector2 textSize = fontScale * font.MeasureString(text);
            Vector2 basePos = GetPos() + new Vector2(rect.Width / 2, GetHeight() / 2);
            spriteBatch.DrawString(font, text, basePos - textSize/2, Color.White, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);

            spriteBatch.Draw(moneyIcon,
                position: basePos + new Vector2(textSize.X/2 + rect.Width / 50, 0) - spriteScale / 2 * new Vector2(0, moneyIcon.Height),
                scale: new Vector2(spriteScale)
                );
        }
    }
}
