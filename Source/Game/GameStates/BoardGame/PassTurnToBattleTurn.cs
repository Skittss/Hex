using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Engine.UI;
using Hex.BoardGame;


namespace Hex.GameStates
{
    //implementation of pass turn screen, only disparity
    // other screen is what information is displayed (Ai/player round)
    public class PassTurnToBattleTurn : PassTurnScreen
    {
        private Player[] battlePlayers;
        private string text;
        private int nonAiPlayerIndex;
        private bool vsAi;

        //separation distance of the two character icons
        private static Vector2 sepDistance = new Vector2(WindowTools.GetUpscaledValue(150f), 0);

        public PassTurnToBattleTurn(GraphicsDevice graphicsDevice, Player[] battlePlayers, bool vsAi) : base(graphicsDevice)  //end bit here calls the base constructor.
        {
            this.battlePlayers = battlePlayers;
            this.vsAi = vsAi;
            if (vsAi)
            {
                nonAiPlayerIndex = (battlePlayers[0] == null) ? 1 : 0;
                text = $"{((nonAiPlayerIndex == 0) ? $"Player {battlePlayers[nonAiPlayerIndex].Id + 1} Vs Ai" : $"Ai Vs Player {battlePlayers[nonAiPlayerIndex].Id + 1}")}";
            }
            else
            {
                nonAiPlayerIndex = 0;
                text = $"Player {battlePlayers[0].Id + 1} Vs Player {battlePlayers[1].Id + 1}";
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            graphicsDevice.Clear(Color.Black);

            Vector2 spriteOffset = uiScale / 2 * UiTools.BoundsToVector(battlePlayers[nonAiPlayerIndex].CharacterSprite.Bounds);
            Vector2 pos = WindowTools.PaddingToPixelCoordinate(0.5f, 0.4f, 0, 0);
            if (vsAi)
            {
                spriteBatch.Draw(
                    texture: boundary,
                    position: pos - spriteOffset - new Vector2(3 * uiScale, 0),
                    scale: uiScaleVector,
                    color: battlePlayers[nonAiPlayerIndex].Colour
                    );

                spriteBatch.Draw(
                    texture: battlePlayers[nonAiPlayerIndex].CharacterSprite,
                    position: pos - spriteOffset,
                    scale: uiScaleVector);
            }
            else
            {
                spriteBatch.Draw(
                    texture: boundary,
                    position: pos - spriteOffset - sepDistance - new Vector2(3 * uiScale, 0),
                    scale: uiScaleVector,
                    color: battlePlayers[0].Colour
                    );

                spriteBatch.Draw(
                    texture: battlePlayers[0].CharacterSprite, 
                    position: pos - spriteOffset - sepDistance, 
                    scale: uiScaleVector);

                spriteBatch.Draw(
                    texture: boundary,
                    position: pos - spriteOffset + sepDistance + new Vector2(3 * uiScale, 0),
                    scale: uiScaleVector,
                    color: battlePlayers[1].Colour
                    );

                spriteBatch.Draw(
                    texture: battlePlayers[1].CharacterSprite,
                    position: pos - spriteOffset + sepDistance,
                    scale: uiScaleVector);

                spriteBatch.Draw(
                    texture: vsPlayerIcon,
                    position: pos - uiScale / 2 * UiTools.BoundsToVector(vsPlayerIcon.Bounds),
                    scale: uiScaleVector
                    );

            }


            //draw pass turn button
            if (passTurnButton.IsHovered)
            {
                spriteBatch.Draw(texture: passTurnButton.HoverTexture, position: passTurnButton.Position, scale: passTurnButton.ScaleVector);
            }
            else
            {
                spriteBatch.Draw(texture: passTurnButton.ButtonTexture, position: passTurnButton.Position, scale: passTurnButton.ScaleVector);
            }

            spriteBatch.DrawString(
                bebasSmall,
                "Begin",
                passTurnButton.MidPoint - bebasSmall.MeasureString("Begin") / 2 + new Vector2(0f, 3f),
                Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);

            spriteBatch.DrawString(
                bebas,
                text,
                WindowTools.PaddingToPixelCoordinate(0.5f, 0.20f, 0, 0) - 0.35f*bebas.MeasureString(text) / 2,
                Color.White, 0f, Vector2.Zero, 0.35f, SpriteEffects.None, 0);
                
            spriteBatch.End();
        }
    }
}
