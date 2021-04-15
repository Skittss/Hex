using Engine.Input;
using Engine.UI;
using Engine.GameStates;
using Hex.BoardGame;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hex.GameStates
{
    //final screen to show who won the game overall.
    public class GameEndScreen : OverlayScreen
    {
        private string newGameText;
        private Vector2 newGamePos;
        private SpriteFont font;
        private SpriteFont largeFont;

        private Player winner;
        private Player loser;

        private Texture2D boundary;
        protected static float uiScale = WindowTools.GetUpscaledValue(6f);
        protected Vector2 uiScaleVector = new Vector2(uiScale);

        //separation distance of the two character sprites
        private static Vector2 sepDistance = new Vector2(WindowTools.GetUpscaledValue(150f), 0);


        public GameEndScreen(GraphicsDevice graphicsDevice, GameState underlyingScreen, Player winner, Player loser) : base(graphicsDevice, underlyingScreen)
        {
            this.winner = winner;
            this.loser = loser;
        }

        public override void LoadContent()
        {
            largeFont = contentManager.Load<SpriteFont>("Fonts/BebasNeue");
            font = contentManager.Load<SpriteFont>("Fonts/BebasNeue_Small");

            boundary = contentManager.Load<Texture2D>("CharacterSelect/Buttons/ButtonBoundaryLR");


            newGameText = $"Press {Keybinds.GetKey(Keybinds.Actions.Continue)} to start a new game or {Keybinds.GetKey(Keybinds.Actions.Exit)} to quit";
            newGamePos = WindowTools.PaddingToPixelCoordinate(0.5f, 0.95f, 0, 0) - font.MeasureString(newGameText) / 2;
        }

        public override void Update(GameTime gameTime)
        {
            //start a new game if continue key pressed
            if (InputManager.Instance.IsKeyToggled(Keybinds.Actions.Continue))
            {
                GameStateManager.Instance.PopAllStates();
                //achieved by returning to character select.
                GameStateManager.Instance.AddState(new CharacterSelect(graphicsDevice));
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            //base: draw the overlay.
            base.Draw(spriteBatch);
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            //draw a string telling the player how to continue/quit.
            spriteBatch.DrawString(font,
                newGameText,
                newGamePos,
                Color.Green
                );

            //draw a string to show who won.
            string winText = $"Player {winner.Id + 1} Wins!";
            spriteBatch.DrawString(largeFont,
                winText,
                WindowTools.PaddingToPixelCoordinate(0.5f, 0.05f, 0, 0) - 
                new Vector2(largeFont.MeasureString(winText).X/2,0),
                winner.Colour
                );

            //draw the final score alongside each character sprite.
            Vector2 spriteOffset = uiScale / 2 * UiTools.BoundsToVector(winner.CharacterSprite.Bounds);
            Vector2 pos = WindowTools.PaddingToPixelCoordinate(0.5f, 0.4f, 0, 0);

            //(character sprites)
            spriteBatch.Draw(
                    texture: boundary,
                    position: pos - spriteOffset - sepDistance - new Vector2(3 * uiScale, 0),
                    scale: uiScaleVector,
                    color: winner.Colour
                    );

            spriteBatch.Draw(
                texture: winner.CharacterSprite,
                position: pos - spriteOffset - sepDistance,
                scale: uiScaleVector);

            spriteBatch.Draw(
                texture: boundary,
                position: pos - spriteOffset + sepDistance + new Vector2(3 * uiScale, 0),
                scale: uiScaleVector,
                color: loser.Colour
                );

            spriteBatch.Draw(
                texture: loser.CharacterSprite,
                position: pos - spriteOffset + sepDistance,
                scale: uiScaleVector);


            //(score)
            string text = $"{winner.WinProgress} - {loser.WinProgress}";
            Vector2 size = font.MeasureString(text);

            spriteBatch.DrawString(font,
                text,
                pos - size / 2,
                Color.White
                );

            spriteBatch.End();
        }
    }
}
