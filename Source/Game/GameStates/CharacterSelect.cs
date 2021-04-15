using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Hex.UI;
using Engine.UI;
using System;
using Hex.Characters;
using Engine.GameStates;

namespace Hex.GameStates
{
    public class CharacterSelect : GameState
    {
        private SideDisplayBox[] infoBoxes;
        private CharSelectConfirmButton confirmButton;
        private CharSelectButton[] uiButtons;
        private CharSelectButton p1SelectedButton;
        private CharSelectButton p2SelectedButton;
        private TutorialButton helpButton;
        private SpriteFont font;
        private SpriteFont largeFont;
        private static Color[] pColors = new Color[] { Color.YellowGreen, Color.Teal };
        private static float buttonScale = WindowTools.GetUpscaledValue(4f);
        private static Vector2 buttonScaleVector = new Vector2(buttonScale);
        private static Vector2 buttonGridOrigin = WindowTools.PaddingToPixelCoordinate(0.225f, 0.4f, 0, 0);
        private static Vector2 buttonTesselateVector = new Vector2(24, 14);
        private static string titleString = "CHOOSE YOUR CHARACTER";

        private static int winThreshold = 5;

        public CharacterSelect(GraphicsDevice graphicsDevice) : base(graphicsDevice)  //end bit here calls the base constructor.
        {
            //add extra constructor business here
            //though now we have graphicsDevice is most likely unnecessary
        }

        public override void Initialize()
        {
            //length is number of characters
            uiButtons = new CharSelectButton[CharacterLinker.GetCharacterCount()];
            infoBoxes = new SideDisplayBox[2];
            //will have to read from the amount of selectable characters.
        }

        public override void LoadContent()
        {
            font = contentManager.Load<SpriteFont>("Fonts/BebasNeue_Small");
            largeFont = contentManager.Load<SpriteFont>("Fonts/BebasNeue");
            LoadContent_Buttons(contentManager);
            LoadContent_Boxes(contentManager);
            GameStateManager.Instance.AddState(new TutorialScreen(graphicsDevice, this));

        }

        private void LoadContent_Buttons(ContentManager Content)
        {
            //generating button for each available character.
            Vector2 prevDrawCoordinate = buttonGridOrigin;
            Vector2 translation;
            for (int i = 0; i < uiButtons.Length; i++)
            {
                //get the directory of respective button textures for characters from the ID;
                Type characterReference = CharacterLinker.GetCharacterReference(i);
                string directory = (string)characterReference.GetField("SpriteReference").GetRawConstantValue();
                //load the button texture(s)
                Texture2D tex = Content.Load<Texture2D>(directory);
                Texture2D outline = Content.Load<Texture2D>("CharacterSelect/Buttons/Outline");

                //create mask
                bool[,] mask = UiTools.CreateBoolMask(tex);
                //generate draw position
                if ((i & 1) == 1)   //if odd number
                {
                    translation = buttonTesselateVector;
                }
                else
                {
                    translation = new Vector2(buttonTesselateVector.X, -buttonTesselateVector.Y);
                }
                //instantiate button
                uiButtons[i] = new CharSelectButton(i, prevDrawCoordinate + (buttonScale * translation), tex, outline, mask, buttonScale);
                //update the draw coordinate for tesselation.
                prevDrawCoordinate = prevDrawCoordinate + (buttonScale * translation);

                //attach event handler to button
                uiButtons[i].buttonPressed += HandleButtonPress;
            }

            //generate help button
            helpButton = new TutorialButton(graphicsDevice, this,
                WindowTools.PaddingToPixelCoordinate(0, 0, 10, 10),
                (int)(20 * buttonScale),
                (int)(10 * buttonScale));

            //generating confirm button
            Texture2D confirmButtonTexture = Content.Load<Texture2D>("PreparationTurn/EndTurnButton");
            Texture2D confirmButtonOutline = Content.Load<Texture2D>("PreparationTurn/EndTurnButtonHovered");
            confirmButton = new CharSelectConfirmButton(WindowTools.PaddingToPixelCoordinate(0.45f, 0.6f, 0, 0) - buttonScale/2 * new Vector2(confirmButtonTexture.Width, 0), confirmButtonTexture, confirmButtonOutline, UiTools.CreateBoolMask(confirmButtonTexture), buttonScale * 2);
            confirmButton.buttonPressed += HandleButtonPress;
        }

        private void LoadContent_Boxes(ContentManager Content)
        {
            infoBoxes[0] = new SideDisplayBox(graphicsDevice, font, WindowTools.PaddingToPixelCoordinate(0f, 0.5f, 10, 0), WindowTools.WindowDimensions.X * 0.25f);
            infoBoxes[1] = new SideDisplayBox(graphicsDevice, font, WindowTools.PaddingToPixelCoordinate(1f, 0.5f, -10, 0), WindowTools.WindowDimensions.X * 0.25f);
            infoBoxes[0].SetAnchorPoint(0f, 0.5f);
            infoBoxes[1].SetAnchorPoint(1f, 0.5f);
            infoBoxes[0].SetPlayerName("Player 1");
            infoBoxes[1].SetPlayerName("Player 2");
        }

        private void HandleButtonPress(object sender, CharSelectButtonEventArgs eventArgs)
        {
            if (eventArgs.type == CharSelectButton.buttonTypes.ConfirmButton)
            {
                if (p1SelectedButton != null && p2SelectedButton != null)
                {
                    BoardGame.GameArgs args = new BoardGame.GameArgs(
                        p1SelectedButton.LinkId,
                        p2SelectedButton.LinkId,
                        pColors[0],
                        pColors[1],
                        winThreshold
                        );

                    GameStateManager.Instance.ChangeState(new BoardGame(graphicsDevice, args));
                }
            }
            else if (eventArgs.type == CharSelectButton.buttonTypes.CharacterSelection)
            {
                if (eventArgs.leftClicked)
                {
                    if (p1SelectedButton == uiButtons[eventArgs.linkId])
                    {
                        p1SelectedButton = null;
                        infoBoxes[0].SetCharacterInformation(null, null);
                    }
                    else
                    {
                        p1SelectedButton = uiButtons[eventArgs.linkId];
                        infoBoxes[0].SetCharacterInformation(p1SelectedButton.ButtonTexture, CharacterLinker.GetCharacterReference(p1SelectedButton.LinkId));
                    }

                }
                else if (eventArgs.rightClicked)
                {
                    if (p2SelectedButton == uiButtons[eventArgs.linkId])
                    {
                        p2SelectedButton = null;
                        infoBoxes[1].SetCharacterInformation(null, null);
                    }
                    else
                    {
                        p2SelectedButton = uiButtons[eventArgs.linkId];
                        infoBoxes[1].SetCharacterInformation(p2SelectedButton.ButtonTexture, CharacterLinker.GetCharacterReference(p2SelectedButton.LinkId));
                    }
                }
            }

        }

        public override void Update(GameTime gameTime)
        {
            //Update Buttons
            helpButton.Update(gameTime);
            confirmButton.Update(gameTime);
            foreach (CharSelectButton uiButton in uiButtons)
            {
                uiButton.Update(gameTime);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            graphicsDevice.Clear(Color.Turquoise);
            Draw_Buttons(spriteBatch);
            Draw_Infoboxes(spriteBatch);
        }

        private void Draw_Infoboxes(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < infoBoxes.Length; i++)
            {
                infoBoxes[i].Draw(spriteBatch, pColors[i]);
            }
        }

        private void Draw_Buttons(SpriteBatch spriteBatch)
        {
            //due to nature of layering and draw orders, it is more simple
            // to handle all calls within the game states draw call
            // instead of through methods in each object.
            
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);


            //CHOOSE YOUR CHARACTER
            Vector2 titleSize = 0.66f * largeFont.MeasureString(titleString);
            spriteBatch.DrawString(largeFont,
                titleString,
                WindowTools.PaddingToPixelCoordinate(0.5f, 0f, 0, 0) - new Vector2(titleSize.X/2, 0),
                Color.White,
                0f, Vector2.Zero, 0.66f, SpriteEffects.None, 0f
                );

            //draw help button
            helpButton.Draw(spriteBatch, font, 1f);

            //display roster of characters (this will be 3 or so)
            //draw character buttons
            for (int i = 0; i < uiButtons.Length; i++)
            {
                if (uiButtons[i] == p2SelectedButton && uiButtons[i] == p1SelectedButton)
                {
                    spriteBatch.Draw(uiButtons[i].ButtonTexture, position: uiButtons[i].Position, scale: buttonScaleVector, color: Color.Purple);
                }
                else if (uiButtons[i] == p1SelectedButton)
                {
                    spriteBatch.Draw(uiButtons[i].ButtonTexture, position: uiButtons[i].Position, scale: buttonScaleVector, color: pColors[0]);
                }
                else if (uiButtons[i] == p2SelectedButton)
                {
                    spriteBatch.Draw(uiButtons[i].ButtonTexture, position: uiButtons[i].Position, scale: buttonScaleVector, color: pColors[1]);

                }
                else
                {
                    spriteBatch.Draw(uiButtons[i].ButtonTexture, position: uiButtons[i].Position, scale: buttonScaleVector);

                }
            }

            //draw confirmation button + corresponding text
            string text = "start";
            Vector2 textSize = font.MeasureString(text);

            if (p1SelectedButton != null && p2SelectedButton != null)
            {
                if (confirmButton.IsHovered)
                {
                    spriteBatch.Draw(confirmButton.HoverTexture, position: confirmButton.Position, scale: buttonScaleVector * 2);
                    spriteBatch.DrawString(font, text, confirmButton.MidPoint - textSize/2, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);

                }
                else
                {
                    spriteBatch.Draw(confirmButton.ButtonTexture, position: confirmButton.Position, scale: buttonScaleVector * 2);
                    spriteBatch.DrawString(font, text, confirmButton.MidPoint - textSize / 2, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);

                }
            }
            else
            {
                spriteBatch.Draw(confirmButton.ButtonTexture, position: confirmButton.Position, scale: buttonScaleVector * 2, color: Color.Gray);
                spriteBatch.DrawString(font, text, confirmButton.MidPoint - textSize / 2, Color.Gray, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);

            }

            //outlines
            for (int i = 0; i < uiButtons.Length; i++)
            {
                if (uiButtons[i].IsHovered)
                {
                    spriteBatch.Draw(uiButtons[i].HoverTexture, position: uiButtons[i].Position - buttonScaleVector, scale: buttonScaleVector);
                }
            }
            //display character info on left or right of screen depending on which player has it selected.
            spriteBatch.End();
        }
    }
}