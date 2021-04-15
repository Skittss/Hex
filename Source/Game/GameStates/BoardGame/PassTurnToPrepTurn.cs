using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Engine.UI;
using Hex.BoardGame;


namespace Hex.GameStates
{
    //show the upcoming player and that it's their turn.
    public class PassTurnToPrepTurn : PassTurnScreen
    {
        private Player nextTurnPlayer;
        private string text;

        public PassTurnToPrepTurn(GraphicsDevice graphicsDevice, Player nextTurnPlayer) : base(graphicsDevice)  //end bit here calls the base constructor.
        {
            this.nextTurnPlayer = nextTurnPlayer;
            text = $"Player {nextTurnPlayer.Id + 1}'s Preparation Turn";
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            graphicsDevice.Clear(Color.Black);

            Vector2 pos = WindowTools.PaddingToPixelCoordinate(0.5f, 0.4f, 0, 0) - (uiScale / 2 * UiTools.BoundsToVector(nextTurnPlayer.CharacterSprite.Bounds));

            spriteBatch.Draw(
                texture: boundary,
                position: pos - new Vector2(3 * uiScale, 0),
                scale: uiScaleVector,
                color: nextTurnPlayer.Colour
                );

            spriteBatch.Draw(
                texture: nextTurnPlayer.CharacterSprite, 
                position: pos, 
                scale: uiScaleVector);

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
