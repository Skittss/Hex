using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Engine.UI;
using System;

namespace Hex.UI
{

    //carries data about button press. must be specific for each button
    //  when different data being carried so just inherits from base EventArgs
    public class CharSelectButtonEventArgs : EventArgs
    {
        public readonly int linkId;
        public readonly bool rightClicked;
        public readonly bool leftClicked;
        public readonly CharSelectButton.buttonTypes type;

        public CharSelectButtonEventArgs(int linkId, CharSelectButton.buttonTypes type, bool rightClicked, bool leftClicked)
        {
            this.linkId = linkId;
            this.rightClicked = rightClicked;
            this.leftClicked = leftClicked;
            this.type = type;
        }
    }

    public class CharSelectButton : SpriteButton
    {
        //eventhandler must be a public field when referencing outside this class
        //- so cannot  inherit through spritebutton.
        public event EventHandler<CharSelectButtonEventArgs> buttonPressed;

        public enum buttonTypes
        {
            CharacterSelection,
            ConfirmButton,
        }

        protected int characterLinkId;  //to link the button to the respective character it represents.
        public int LinkId { get { return characterLinkId; } }

        public CharSelectButton(int linkId, Vector2 position, Texture2D buttonTexture, Texture2D hoverTexture, bool[,] mask, float scale) : base(position, buttonTexture, hoverTexture, mask, scale)
        {
            characterLinkId = linkId;
        }

        protected override void leftAction()
        {
            buttonPressed(this, new CharSelectButtonEventArgs(characterLinkId, buttonTypes.CharacterSelection, false, true));
        }

        protected override void rightAction()
        {
            buttonPressed(this, new CharSelectButtonEventArgs(characterLinkId, buttonTypes.CharacterSelection, true, false));
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
    }
}