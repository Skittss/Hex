using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Engine.Input;
using Engine.UI;
using Hex.BoardGame;

namespace Hex.UI
{
    //containing class for shop button press
    public class ShopButtonArgs
    {
        //stores entry number, when received by the shop class,
        // it can determine the index of the entry to sell using this value.
        public readonly int entryNumber;

        public ShopButtonArgs(int entryNumber)
        {
            this.entryNumber = entryNumber;
        }
    }

    public class UnitShopButton : RectBoxButton<ShopButtonArgs>
    {

        private int number;
        private Color unableToBuyColor;

        public UnitShopButton(int number, GraphicsDevice graphicsDevice, Vector2 pos, int width, int height) : base(graphicsDevice, pos, width, height)
        {
            this.number = number;
            unableToBuyColor = Color.DarkRed;
            unableToBuyColor.A = 200;
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
                    leftAction(new ShopButtonArgs(number));
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Player owner, UnitShop.ShopEntry[] entries)
        {
            if (entries[number].Sold || owner.IsBenchFull() || owner.Money < entries[number].cost)
            {
                spriteBatch.Draw(texture, DestinationRect, unableToBuyColor);
            }
            else
            {
                base.Draw(spriteBatch);
            }

        }
    }
}
