using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Engine.Input;

namespace Engine.UI
{
    //A slightly more complex button.
    //works in a similar way to rectangular buttons in terms of event handling.
    //however, detection of hovering/clicks are determined using a mask of the
    //texture, and comparing the mouse pos relative to the mask.
    public abstract class SpriteButton
    {
        protected bool[,] spriteMask;
        protected Vector2 position;
        protected Texture2D buttonTexture;
        protected Texture2D hoverTexture;
        protected float scale;
        protected bool isHovered;

        public Texture2D ButtonTexture { get { return buttonTexture; } }
        public Texture2D HoverTexture { get { return hoverTexture; } }
        public Vector2 Position { get { return position; } }
        public bool IsHovered { get { return isHovered; } }
        public Vector2 ScaleVector { get { return new Vector2(scale); } }


        public SpriteButton(Vector2 position, Texture2D buttonTexture , bool[,] spriteMask, float scale)
        {
            //pass in a rectangular array of bools - these represent if
            // there is a visible pixel present in the mask.(Alpha =/= 0)
            this.buttonTexture = buttonTexture;
            this.spriteMask = spriteMask;
            this.position = position;
            this.scale = scale;
        }

        public SpriteButton(Vector2 position, Texture2D buttonTexture, Texture2D hoverTexture, bool[,] spriteMask, float scale)
        {
            this.buttonTexture = buttonTexture;
            this.hoverTexture = hoverTexture;
            this.spriteMask = spriteMask;
            this.position = position;
            this.scale = scale;
        }

        protected abstract void leftAction();
        protected abstract void rightAction();

        public virtual void Update(GameTime gameTime) 
        {
            isHovered = false;
            if (ContainsPoint(InputManager.Instance.MousePos))
            {
                isHovered = true;
                if (InputManager.Instance.IsLeftMouseToggled())
                {
                    //send button press event
                    leftAction();
                }
                else if (InputManager.Instance.IsRightMouseToggled())
                {
                    //send button press event
                    rightAction();
                }
            }
        }

        protected bool ContainsPoint(Vector2 point)
        {
            //calculate the mouse's position in the mask. check if the bool is true.
            Vector2 maskPoint = (point - position) / scale;
            maskPoint.X = (float)Math.Floor(maskPoint.X);
            maskPoint.Y = (float)Math.Floor(maskPoint.Y);

            if (maskPoint.X >= 0 && maskPoint.X < spriteMask.GetLength(0) && maskPoint.Y >= 0 && maskPoint.Y < spriteMask.GetLength(1))
            {
                bool result = spriteMask[(int)maskPoint.X, (int)maskPoint.Y];
                return result;
            }
            else
            {
                //if mouse pos is not in the mask, return false.
                return false;
            }
        }
    }
}
