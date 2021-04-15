using System;
using System.Collections.Generic;
using System.Linq;
using Engine.UI;
using Hex.BoardGame;
using Hex.GameStates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hex.UI
{
    /// <summary>
    /// Large UI element to display the action log to the player.
    /// </summary>
    public class ActionLogBox
    {
        private static int textBoxHeight = (int)WindowTools.GetUpscaledValue(60f);
        private static int actionBoxHeight = (int)WindowTools.GetUpscaledValue(70f);
        private static int width = (int)WindowTools.GetUpscaledValue(250f);
        private static float spriteScale = WindowTools.GetUpscaledValue(3f);
        private static float iconScale = WindowTools.GetUpscaledValue(2f);
        private static Vector2 spriteScaleVector = new Vector2(spriteScale);
        private static Vector2 iconScaleVector = new Vector2(iconScale);

        private SpriteFont textFont;
        private Vector2 anchorPoint = Vector2.Zero;
        private Vector2 dimensions;
        private Vector2 pos;
        private Vector2 destinationPos
        {
            get { return new Vector2(pos.X - anchorPoint.X, pos.Y - anchorPoint.Y); }
        }

        //Element structure - one text box followed by a dividing line,
        //  and a number of boxes to display each action.
        private RectBox textBox;
        private Line divisorA;
        private RectBox[] actionBoxes;

        private Texture2D emptyRect;
        private string text = "Battle Log:";

        public ActionLogBox(GraphicsDevice graphicsDevice, int capacity, SpriteFont textFont, Vector2 pos)
        {
            this.pos = pos;
            this.textFont = textFont;

            //generate a colourless pixel to draw colour blocks with.
            emptyRect = UiTools.GeneratePixelTexture(graphicsDevice);


            textBox = new RectBox(graphicsDevice,
                this.pos,
                width, textBoxHeight);

            divisorA = new Line(graphicsDevice,
                new Vector2(pos.X, textBox.GetBottom()),
                new Vector2(pos.X + width, textBox.GetBottom()),
                5, Color.White);

            actionBoxes = new RectBox[capacity];

            for (int i = 0; i < capacity; i++)
            {
                actionBoxes[i] = new RectBox(graphicsDevice,
                    new Vector2(
                        pos.X,
                        (i == 0)? textBox.GetBottom() : actionBoxes[i-1].GetBottom()),
                    width, actionBoxHeight);
            }


            dimensions = new Vector2(width,
                textBox.GetHeight() + capacity * actionBoxHeight);
        }

        /// <summary>
        /// Takes a fractional offset from the top left of the box and will draw the
        /// box from the offset given.
        /// </summary>
        public void SetAnchorPoint(float offsetX, float offsetY)
        {
            //define an anchorpoint relative to the top left of the box
            //expects between 0 and 1
            anchorPoint = new Vector2(offsetX * dimensions.X, offsetY * dimensions.Y);
            textBox.SetPosition(destinationPos);
            divisorA.SetPoints(
                new Vector2(destinationPos.X, textBox.DestinationRect.Bottom),
                new Vector2(destinationPos.X + width, textBox.DestinationRect.Bottom)
                );

            actionBoxes[0].SetPosition(new Vector2(destinationPos.X, textBox.DestinationRect.Bottom));
            for (int i = 1; i < actionBoxes.Length; i++)
            {
                actionBoxes[i].SetPosition(new Vector2(destinationPos.X, actionBoxes[i - 1].DestinationRect.Bottom));
            }
        }

        //using a struct here to store icons that this class needs to draw with, makes the draw call less cluttered.
        public struct DrawParams
        {
            public readonly Texture2D attackIcon;
            public readonly Texture2D defenseIcon;
            public readonly Texture2D noMoveIcon;
            public readonly Texture2D moveIcon;
            public readonly Texture2D skullIcon;
            public readonly Texture2D targetIcon;
            public readonly Dictionary<Type, Texture2D> unitSprites;

            public DrawParams(Dictionary<Type, Texture2D> unitSprites, Texture2D attackIcon, Texture2D defenseIcon, Texture2D noMoveIcon, Texture2D moveIcon, Texture2D skullIcon, Texture2D targetIcon)
            {
                this.unitSprites = unitSprites;
                this.attackIcon = attackIcon;
                this.defenseIcon = defenseIcon;
                this.noMoveIcon = noMoveIcon;
                this.moveIcon = moveIcon;
                this.skullIcon = skullIcon;
                this.targetIcon = targetIcon;
            }
        }

        public void Draw(SpriteBatch spriteBatch, BattleTurn.ActionArgs[] actions, Player[] players, DrawParams textures)
        {
            //pass in the information about actions -- draw for each one. This information does not need to be stored here too.


            //draw underlying alpha box for the box containing text.
            textBox.Draw(spriteBatch);
            //calculate the size of the text
            Vector2 nameSize = textFont.MeasureString(text);
            //calculate the position where the text needs to be drawn to be central
            //  to the text box.
            Vector2 namePos = textBox.GetCentrePos() - 0.5f * new Vector2(nameSize.X, nameSize.Y);
            //draw the text
            spriteBatch.DrawString(textFont, text, namePos, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);

            foreach (RectBox box in actionBoxes)
            {
                box.Draw(spriteBatch);
            }

            //draw respective information about each action
            for (int i = 0; i < actions.Length; i++)
            {
                BattleTurn.ActionArgs action = actions[i];
                RectBox boxDrawingTo = actionBoxes[i];
                Vector2 boxPos = boxDrawingTo.GetPos();

                Color boxColor = (action.ActingUnit.OwnerId == 3) ?
                    Color.Gray :
                    players.Where(player => player.Id == action.ActingUnit.OwnerId).First().Colour; //set the color to Ai color/player color


                //draw a coloured block to specify which player the acting unit belongs to.
                //Using a single pixel texture instead of a non-disposable object (say my RectBox class for instance)
                // eliminates memory leak issues.
                spriteBatch.Draw(emptyRect, new Rectangle(boxDrawingTo.GetPos().ToPoint(), new Point(10, actionBoxHeight)), color: boxColor);

                Texture2D actingUnitSprite = action.ActingUnit.GetSprite(textures.unitSprites);


                //draw the units involved in each action, and the icons displaying the type of action.
                if (action.IsAttack)
                {
                    //draw the attacking unit
                    spriteBatch.Draw(actingUnitSprite,
                        position: boxPos + new Vector2(width / 5, boxDrawingTo.GetHeight() / 2) - spriteScale / 2 * UiTools.BoundsToVector(actingUnitSprite.Bounds),
                        scale: spriteScaleVector
                        );

                    //draw the attack icon
                    spriteBatch.Draw(textures.attackIcon,
                        position: boxPos + new Vector2(width / 5, boxDrawingTo.GetHeight() / 2),
                        scale: iconScaleVector
                        );

                    //draw the target unit
                    Texture2D targetUnitSprite = action.TargetUnit.GetSprite(textures.unitSprites);
                    spriteBatch.Draw(targetUnitSprite,
                        position: boxPos + new Vector2(2.5f * width / 5, boxDrawingTo.GetHeight() / 2) - spriteScale / 2 * UiTools.BoundsToVector(targetUnitSprite.Bounds),
                        scale: spriteScaleVector
                        );

                    //draw the defense icon
                    spriteBatch.Draw(textures.defenseIcon,
                        position: boxPos + new Vector2(2.5f * width / 5, boxDrawingTo.GetHeight() / 2) - iconScale * new Vector2(textures.defenseIcon.Width, 0),
                        scale: iconScaleVector
                        );

                    //draw the skull icon if the target unit died as a result of the action.
                    if (action.TargetFainted)
                    {
                        spriteBatch.Draw(textures.skullIcon,
                            position: boxPos + new Vector2(3.5f * width / 5, boxDrawingTo.GetHeight() / 2) - iconScale / 2 * UiTools.BoundsToVector(textures.skullIcon.Bounds),
                            scale: iconScaleVector * 2
                            );
                    }
                }
                else if (action.IsMove)
                {
                    //draw acting unit
                    spriteBatch.Draw(actingUnitSprite,
                        position: boxPos + new Vector2(width / 5, boxDrawingTo.GetHeight() / 2) - spriteScale / 2 * UiTools.BoundsToVector(actingUnitSprite.Bounds),
                        scale: spriteScaleVector
                        );

                    //draw move icon
                    spriteBatch.Draw(textures.moveIcon,
                        position: boxPos + new Vector2(width / 5, boxDrawingTo.GetHeight() / 2),
                        scale: iconScaleVector
                        );

                    //draw target unit
                    Texture2D targetUnitSprite = action.TargetUnit.GetSprite(textures.unitSprites);
                    spriteBatch.Draw(targetUnitSprite,
                        position: boxPos + new Vector2(2.5f * width / 5, boxDrawingTo.GetHeight() / 2) - spriteScale / 2 * UiTools.BoundsToVector(targetUnitSprite.Bounds),
                        scale: spriteScaleVector
                        );

                    //draw targetting icon
                    spriteBatch.Draw(textures.targetIcon,
                        position: boxPos + new Vector2(2.5f * width / 5, boxDrawingTo.GetHeight() / 2) - iconScale * new Vector2(textures.targetIcon.Width, 0),
                        scale: iconScaleVector
                        );

                }
                else //(No move)
                {
                    //Draw the unit trying to act
                    spriteBatch.Draw(actingUnitSprite,
                        position: boxPos + new Vector2(width / 5, boxDrawingTo.GetHeight() / 2) - spriteScale / 2 * UiTools.BoundsToVector(actingUnitSprite.Bounds),
                        scale: spriteScaleVector
                        );

                    //Draw no act icon (cant move)
                    spriteBatch.Draw(textures.noMoveIcon,
                        position: boxPos + new Vector2(width / 5, boxDrawingTo.GetHeight() / 2),
                        scale: iconScaleVector
                        );
                }
            }

            //draw dividing line
            divisorA.Draw(spriteBatch); 
        }
    }
}
