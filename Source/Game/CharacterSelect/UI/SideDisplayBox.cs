using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Engine.UI;
using System;

namespace Hex.UI
{
    /// <summary>
    /// large UI element used to display each player's character,
    /// its name, and ability description.
    /// </summary>
    public class SideDisplayBox
    {
        private static int nameBoxHeight = (int)WindowTools.GetUpscaledValue(60f);
        private static int spriteBoxHeight = (int)WindowTools.GetUpscaledValue(160f);
        private static int infoBoxHeight = (int)WindowTools.GetUpscaledValue(300f);
        private static float spriteScale = WindowTools.GetUpscaledValue(4f);
        private static Vector2 spriteScaleVector = new Vector2(spriteScale);

        private SpriteFont textFont;
        private int width;
        private Vector2 anchorPoint = Vector2.Zero;
        private Vector2 dimensions;
        private Vector2 pos;
        private Vector2 destinationPos
        {
            get { return new Vector2(pos.X - anchorPoint.X, pos.Y - anchorPoint.Y); }
        }

        private RectBox playerNameBox;
        private Line divisorA;
        private RectBox characterSpriteBox;
        private Line divisorB;
        private RectBox characterInfoBox;

        private Texture2D characterSprite;
        private Type characterType;

        //text
        private static int textPadding = 10;
        private static int lineSpacing = 10;
        private string playerName;
        private string abilityDescription;
        private string[] descriptionLines;

        public SideDisplayBox(GraphicsDevice graphicsDevice, SpriteFont textFont, Vector2 pos, float width)
        {
            this.width = (int)width;
            this.pos = pos;
            this.textFont = textFont;

            playerNameBox = new RectBox(graphicsDevice, 
                this.pos, 
                this.width, nameBoxHeight);

            divisorA = new Line(graphicsDevice,
                new Vector2(pos.X, playerNameBox.GetBottom()), 
                new Vector2(pos.X + width, playerNameBox.GetBottom()), 
                5, Color.White);

            characterSpriteBox = new RectBox(graphicsDevice, 
                new Vector2(pos.X, playerNameBox.GetBottom()),
                this.width, spriteBoxHeight);

            divisorB = new Line(graphicsDevice, 
                new Vector2(pos.X, characterSpriteBox.GetBottom()), 
                new Vector2(pos.X + width, characterSpriteBox.GetBottom()), 
                5, Color.White);

            characterInfoBox = new RectBox(graphicsDevice,
                new Vector2(pos.X, characterSpriteBox.GetBottom()), 
                this.width, infoBoxHeight);

            dimensions = new Vector2(width,
                playerNameBox.GetHeight() + characterSpriteBox.GetHeight() + characterInfoBox.GetHeight());
        }

        public void SetCharacterInformation(Texture2D sprite, Type characterType)
        {
            characterSprite = sprite;
            this.characterType = characterType;
            abilityDescription = (characterType == null)? null : (string)characterType.GetField("AbilityDescription").GetRawConstantValue();
            //long string - wrap it using the text wrapper.
            descriptionLines = (characterType == null)? null : UiTools.WrapText(textFont, abilityDescription, width, textPadding, 1f);
        }

        public void SetPlayerName(string name)
        {
            playerName = name;
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
            playerNameBox.SetPosition(destinationPos);

            //update the position of elements dependent on another elements position based on the new draw position.
            divisorA.SetPoints(
                new Vector2(destinationPos.X, playerNameBox.DestinationRect.Bottom), 
                new Vector2(destinationPos.X + width, playerNameBox.DestinationRect.Bottom)
                );

            characterSpriteBox.SetPosition(new Vector2(destinationPos.X, playerNameBox.DestinationRect.Bottom));
            divisorB.SetPoints(
                new Vector2(destinationPos.X, characterSpriteBox.DestinationRect.Bottom), 
                new Vector2(destinationPos.X + width, characterSpriteBox.DestinationRect.Bottom)
                );

            characterInfoBox.SetPosition(new Vector2(destinationPos.X, characterSpriteBox.DestinationRect.Bottom));
        }

        public void Draw(SpriteBatch spriteBatch, Color textColor)
        {
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            //draw underlying alpha box
            playerNameBox.Draw(spriteBatch);
            //calculate the size of the players name
            Vector2 nameSize = textFont.MeasureString(playerName);
            //calculate the position where the name needs to be drawn to be central
            //  to the player name box.
            Vector2 namePos = playerNameBox.GetCentrePos() - 0.5f * nameSize;
            //draw the player name
            spriteBatch.DrawString(textFont, playerName, namePos, textColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);

            characterSpriteBox.Draw(spriteBatch);
            characterInfoBox.Draw(spriteBatch);

            //draw dividing lines
            divisorA.Draw(spriteBatch);
            divisorB.Draw(spriteBatch);

            //draw respective information about character if a character is selected
            if (characterType != null)
            {
                //draw selected character sprite
                Vector2 spritePos = characterSpriteBox.GetCentrePos() - 0.5f * spriteScale * UiTools.BoundsToVector(characterSprite.Bounds);
                spriteBatch.Draw(characterSprite, position: spritePos, scale: spriteScaleVector);

                //draw character name
                string chrName = characterType.Name;
                Vector2 basePos = characterInfoBox.GetPos() + new Vector2(characterInfoBox.DestinationRect.Width / 2, textPadding + textFont.MeasureString(chrName).Y/2);
                namePos = basePos - textFont.MeasureString(chrName) / 2;
                spriteBatch.DrawString(textFont, chrName, namePos, textColor);

                string ability = $"Ability:";
                Vector2 abilitySize = textFont.MeasureString(ability);
                Vector2 abilityPos = basePos
                    + new Vector2(0, 5 * lineSpacing + textFont.MeasureString(chrName).Y / 2);
                spriteBatch.DrawString(textFont, ability, abilityPos - abilitySize / 2, Color.Red);

                //draw the ability description.
                // this is a long string being drawn to a small box, so
                // the text is wrapped according to the width of the box.
                // (See uitools for how text wrapping works).
                float drawGap = lineSpacing + textFont.MeasureString(abilityDescription).Y/2;
                Vector2 linesOrigin = abilityPos + new Vector2(0 , abilitySize.Y/2 + lineSpacing);
                for (int i = 0; i < descriptionLines.Length; i++)
                {
                    spriteBatch.DrawString(textFont, 
                        descriptionLines[i],
                        linesOrigin + new Vector2(0, i * drawGap) - textFont.MeasureString(descriptionLines[i])/2,
                        Color.White);
                }
            }

            spriteBatch.End();
        }
    }
}
