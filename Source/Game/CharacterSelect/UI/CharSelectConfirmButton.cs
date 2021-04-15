using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Engine.UI;

namespace Hex.UI
{
    //inherit from charselectbutton (sort of lazy, but with
    // a flag of -1 for the linking id, this is no issue).
    public class CharSelectConfirmButton : CharSelectButton
    {
        public event EventHandler<CharSelectButtonEventArgs> buttonPressed;

        public Vector2 MidPoint { get { return position + scale/2 * UiTools.BoundsToVector(buttonTexture.Bounds); } }

        //note linkId is -1
        public CharSelectConfirmButton(Vector2 position, Texture2D buttonTexture, Texture2D hoverTexture, bool[,] mask, float scale) : base(-1, position, buttonTexture, hoverTexture, mask, scale)
        {

        }

        protected override void rightAction() { } //do nothing
        protected override void leftAction()
        {
            buttonPressed(this, new CharSelectButtonEventArgs(characterLinkId, buttonTypes.ConfirmButton, false, true));
        }
    }
}