using System;
using Engine.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hex.UI { 
    public class SinglePressEventArgs : EventArgs
    {
        public readonly bool rightClicked;
        public readonly bool leftClicked;

        public SinglePressEventArgs(bool rightClicked, bool leftClicked)
        {
            this.rightClicked = rightClicked;
            this.leftClicked = leftClicked;
        }
    }

    public class SinglePressSpriteButton : SpriteButton
    {
        public event EventHandler<SinglePressEventArgs> buttonPressed;
        public Vector2 MidPoint
        {
            get { return position + (0.5f * scale * UiTools.BoundsToVector(buttonTexture.Bounds)); }
        }

        public SinglePressSpriteButton(Vector2 position, Texture2D buttonTexture, Texture2D hoverTexture, bool[,] mask, float scale) : base(position, buttonTexture, hoverTexture, mask, scale)
        {

        }

        protected override void leftAction() 
        {
            //MUST be left clicked to perform action.
            buttonPressed(this, new SinglePressEventArgs(false, true));
        }
        protected override void rightAction() { }   //do nothing

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
    }
}
