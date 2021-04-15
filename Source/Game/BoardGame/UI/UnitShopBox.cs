using System;
using System.Collections.Generic;
using Engine.UI;
using Hex.BoardGame;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hex.UI
{
    public class UnitShopBox
    {
        private static int textBoxHeight = (int)WindowTools.GetUpscaledValue(60f);
        private static int unitBoxHeight = (int)WindowTools.GetUpscaledValue(70f);
        private static int width = (int)WindowTools.GetUpscaledValue(250f);
        private static float spriteScale = WindowTools.GetUpscaledValue(3f);
        private static Vector2 spriteScaleVector = new Vector2(spriteScale);

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
        private UnitShopButton[] unitBoxes;

        private string text = "Unit Shop:";
        private string soldText = "Sold!";
        private string fullText = "Bench Full!";

        public UnitShopBox(UnitShop parent, GraphicsDevice graphicsDevice, int capacity, Vector2 pos)
        {
            this.pos = pos;

            textBox = new RectBox(graphicsDevice,
                this.pos,
                width, textBoxHeight);

            divisorA = new Line(graphicsDevice,
                new Vector2(pos.X, textBox.GetBottom()),
                new Vector2(pos.X + width, textBox.GetBottom()),
                5, Color.White);

            unitBoxes = new UnitShopButton[capacity];

            for (int i = 0; i < capacity; i++)
            {
                unitBoxes[i] = new UnitShopButton(i, graphicsDevice,
                    new Vector2(
                        pos.X,
                        (i == 0) ? textBox.GetBottom() : unitBoxes[i - 1].GetBottom()),
                    width, unitBoxHeight);
                unitBoxes[i].buttonPressed += parent.SellEntry;
            }

            dimensions = new Vector2(width,
                textBox.GetHeight() + capacity * unitBoxHeight);
        }

        public void Update(GameTime gameTime)
        {
            foreach (UnitShopButton box in unitBoxes)
            {
                box.Update(gameTime);
            }
        }

        /// <summary>
        /// Takes a fractional offset from the top left of the box and will draw the
        /// box from the offset given.
        /// </summary>
        public void SetAnchorPoint(float offsetX, float offsetY)
        {
            //define an anchorpoint relative to the top left of the box between 0 and 1
            anchorPoint = new Vector2(offsetX * dimensions.X, offsetY * dimensions.Y);
            textBox.SetPosition(destinationPos);
            divisorA.SetPoints(
                new Vector2(destinationPos.X, textBox.DestinationRect.Bottom),
                new Vector2(destinationPos.X + width, textBox.DestinationRect.Bottom)
                );

            unitBoxes[0].SetPosition(new Vector2(destinationPos.X, textBox.DestinationRect.Bottom));
            for (int i = 1; i < unitBoxes.Length; i++)
            {
                unitBoxes[i].SetPosition(new Vector2(destinationPos.X, unitBoxes[i - 1].DestinationRect.Bottom));
            }
        }

        public void Draw(SpriteBatch spriteBatch, Player owner, UnitShop.ShopEntry[] entries, Dictionary<Type, Texture2D> unitSprites, Texture2D moneyIcon, SpriteFont textFont)
        {
            //pass in the information about shop entries -- draw for each one.

            //draw underlying alpha box for the box containing text.
            textBox.Draw(spriteBatch);
            //calculate the size of the text
            Vector2 textSize = textFont.MeasureString(text);
            //calculate the position where the text needs to be drawn to be central
            //  to the text box.
            Vector2 textPos = textBox.GetCentrePos() - 0.5f * textSize;
            //draw the text
            spriteBatch.DrawString(textFont, text, textPos, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);

            foreach (UnitShopButton box in unitBoxes)
            {
                box.Draw(spriteBatch, owner, entries);
            }

            //draw respective information about each entry
            for (int i = 0; i < entries.Length; i++)
            {
                UnitShop.ShopEntry entry = entries[i];
                UnitShopButton boxDrawingTo = unitBoxes[i];
                Vector2 boxPos = boxDrawingTo.GetPos();
                Texture2D unitSprite = unitSprites[entry.unitType];

                spriteBatch.Draw(unitSprite,
                    position: boxPos + new Vector2(width / 6, boxDrawingTo.GetHeight() / 2) - spriteScale / 2 * UiTools.BoundsToVector(unitSprite.Bounds),
                    scale: spriteScaleVector
                    );

                if (entry.Sold)
                {
                    Vector2 size = textFont.MeasureString(soldText);
                    Vector2 pos = boxPos + new Vector2(width / 3, (boxDrawingTo.GetHeight() - size.Y) / 2);
                    spriteBatch.DrawString(textFont, soldText, pos, Color.White);
                }
                else if (owner.IsBenchFull())
                {
                    Vector2 size = textFont.MeasureString(fullText);
                    Vector2 pos = boxPos + new Vector2(width / 3, (boxDrawingTo.GetHeight() - size.Y) / 2);
                    spriteBatch.DrawString(textFont, fullText, pos, Color.White);
                }
                else
                {
                    string text = $"Buy: {entry.cost}";
                    Vector2 size = textFont.MeasureString(text);
                    Vector2 pos = boxPos + new Vector2(width / 3, (boxDrawingTo.GetHeight() - size.Y) / 2);
                    spriteBatch.DrawString(textFont, text, pos, Color.White);
                    spriteBatch.Draw(moneyIcon,
                        position: boxPos + new Vector2(width / 3 + width/15 + size.X, boxDrawingTo.GetHeight()/2) - spriteScale / 2 * UiTools.BoundsToVector(moneyIcon.Bounds),
                        scale: spriteScaleVector
                        );
                }

            }

            //draw dividing line
            divisorA.Draw(spriteBatch);
        }
    }
}
