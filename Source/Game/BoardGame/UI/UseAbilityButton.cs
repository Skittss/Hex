using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Engine.Input;
using Engine.UI;
using Hex.BoardGame;

namespace Hex.UI
{
    public class UseAbilityButton : RectBoxButton<SinglePressEventArgs>
    {
        private Color CantUseColor = new Color(139, 0, 0, 200);

        public UseAbilityButton(GraphicsDevice graphicsDevice, Vector2 pos, int width, int height) : base(graphicsDevice, pos, width, height) { }

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
        public void Draw(SpriteBatch spriteBatch, Player turnPlayer, SpriteFont font, float fontScale)
        {
            string text;
            if (turnPlayer.AbilityUsed)
            {
                spriteBatch.Draw(texture, DestinationRect, CantUseColor);
                text = $"Ability Used!";
            }
            else
            {
                text = $"Use Ability";
                base.Draw(spriteBatch);
            }

            //draw overlaying text.
            Vector2 textSize = fontScale * font.MeasureString(text);
            Vector2 basePos = GetPos() + new Vector2(rect.Width / 2, GetHeight() / 2);
            spriteBatch.DrawString(font, text, basePos - textSize/2, Color.White, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);
        }
    }
}
